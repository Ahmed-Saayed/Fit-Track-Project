using Common;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Repositories
{
    public class TrainerRepository(ApplicationDbContext context)
        : GenericRepository<Trainer>(context), ITrainerRepository
    {
        private readonly ApplicationDbContext _db = context;

        // ──────────────────────────────────────────────────────────────
        //  Get a single trainer with their assigned classes
        // ──────────────────────────────────────────────────────────────
        public async Task<Trainer?> GetWithClassesAsync(int trainerId)
        {
            return await _db.Trainers
                .Where(t => t.Id == trainerId && !t.IsDeleted)
                .Include(t => t.Classes)
                .FirstOrDefaultAsync();
        }

        // ──────────────────────────────────────────────────────────────
        //  Full-text search (name / phone / specialty)
        // ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Trainer>> SearchAsync(string keyword)
        {
            var kw = keyword.Trim().ToLower();

            return await _db.Trainers
                .Where(t => !t.IsDeleted &&
                            (t.FullName.ToLower().Contains(kw) ||
                             t.PhoneNumber.Contains(kw) ||
                             t.Specialty.ToLower().Contains(kw)))
                .Include(t => t.Classes)
                .OrderBy(t => t.FullName)
                .Take(50)
                .ToListAsync();
        }
    }
}
