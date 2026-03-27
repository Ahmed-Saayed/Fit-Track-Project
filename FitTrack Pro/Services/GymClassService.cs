using Common;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Services
{
    public class GymClassService(
        IUnitOfWork uow,
        ITrainerRepository trainerRepo) : IGymClassService
    {
        // ────────────────────────────────────────────────────────────
        //  PAGED LIST  (with optional search)
        // ────────────────────────────────────────────────────────────
        public async Task<GymClassIndexViewModel> GetPagedGymClassesAsync(
            int page, int pageSize, string? search)
        {
            IQueryable<GymClass> query = uow.GymClasses
                .GetAllAsync()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Trainer)
                .Include(c => c.Attendees);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(kw) ||
                    (c.Trainer != null && c.Trainer.FullName.ToLower().Contains(kw)));
            }

            int total = await query.CountAsync();

            var classes = await query
                .OrderBy(c => c.ScheduleTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new GymClassIndexViewModel
            {
                GymClasses = classes.Select(MapToRow),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };
        }

        // ────────────────────────────────────────────────────────────
        //  DETAILS
        // ────────────────────────────────────────────────────────────
        public async Task<GymClassDetailsViewModel?> GetGymClassDetailsAsync(int id)
        {
            var gymClass = await uow.GymClasses
                .GetAllAsync()
                .Where(c => c.Id == id && !c.IsDeleted)
                .Include(c => c.Trainer)
                .Include(c => c.Attendees)
                    .ThenInclude(a => a.Member)
                .FirstOrDefaultAsync();

            if (gymClass is null) return null;

            return new GymClassDetailsViewModel
            {
                Id = gymClass.Id,
                Name = gymClass.Name,
                TrainerId = gymClass.TrainerId,
                TrainerName = gymClass.Trainer?.FullName ?? "—",
                ScheduleTime = gymClass.ScheduleTime,
                DurationInMinutes = gymClass.DurationInMinutes,
                MaxCapacity = gymClass.MaxCapacity,
                AttendeeCount = gymClass.Attendees?.Count(a => !a.IsDeleted) ?? 0,
                CreatedAt = gymClass.CreatedAt,
                Attendees = gymClass.Attendees?
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AttendeeViewModel
                    {
                        MemberId = a.MemberId,
                        MemberName = a.Member?.FullName ?? "Unknown",
                        MemberPhoneNumber = a.Member?.PhoneNumber ?? "—",
                        AttendanceDate = a.AttendanceDate
                    }).ToList() ?? []
            };
        }

        // ────────────────────────────────────────────────────────────
        //  FORM  (for both Create and Edit)
        // ────────────────────────────────────────────────────────────
        public async Task<GymClassFormViewModel> GetCreateFormAsync()
        {
            return new GymClassFormViewModel
            {
                TrainerOptions = await BuildTrainerOptionsAsync()
            };
        }

        public async Task<GymClassFormViewModel?> GetEditFormAsync(int id)
        {
            var gymClass = await uow.GymClasses.GetByIdAsync(id);
            if (gymClass is null || gymClass.IsDeleted) return null;

            return new GymClassFormViewModel
            {
                Id = gymClass.Id,
                Name = gymClass.Name,
                TrainerId = gymClass.TrainerId,
                ScheduleTime = gymClass.ScheduleTime,
                DurationInMinutes = gymClass.DurationInMinutes,
                MaxCapacity = gymClass.MaxCapacity,
                TrainerOptions = await BuildTrainerOptionsAsync(gymClass.TrainerId)
            };
        }

        // ────────────────────────────────────────────────────────────
        //  CREATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error, int NewId)> CreateGymClassAsync(
            GymClassFormViewModel model)
        {
            if (model.ScheduleTime.Hour < 8)
                return (false, "Classes cannot start before 08:00 AM.", 0);

            var gymClass = new GymClass
            {
                Name = model.Name.Trim(),
                TrainerId = model.TrainerId,
                ScheduleTime = model.ScheduleTime,
                DurationInMinutes = model.DurationInMinutes,
                MaxCapacity = model.MaxCapacity,
                CreatedAt = DateTime.Now
            };

            await uow.GymClasses.AddAsync(gymClass);
            await uow.CompleteAsync();

            return (true, null, gymClass.Id);
        }

        // ────────────────────────────────────────────────────────────
        //  UPDATE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> UpdateGymClassAsync(
            GymClassFormViewModel model)
        {
            if (model.ScheduleTime.Hour < 8)
                return (false, "Classes cannot start before 08:00 AM.");

            var gymClass = await uow.GymClasses.GetByIdAsync(model.Id);
            if (gymClass is null || gymClass.IsDeleted)
                return (false, "Gym class not found.");

            gymClass.Name = model.Name.Trim();
            gymClass.TrainerId = model.TrainerId;
            gymClass.ScheduleTime = model.ScheduleTime;
            gymClass.DurationInMinutes = model.DurationInMinutes;
            gymClass.MaxCapacity = model.MaxCapacity;

            uow.GymClasses.Update(gymClass);
            await uow.CompleteAsync();

            return (true, null);
        }

        // ────────────────────────────────────────────────────────────
        //  SOFT DELETE
        // ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string? Error)> DeleteGymClassAsync(int id)
        {
            var gymClass = await uow.GymClasses.GetByIdAsync(id);
            if (gymClass is null || gymClass.IsDeleted)
                return (false, "Gym class not found.");

            gymClass.IsDeleted = true;
            uow.GymClasses.Update(gymClass);
            await uow.CompleteAsync();

            return (true, null);
        }

        // ────────────────────────────────────────────────────────────
        //  WEEKLY SCHEDULE
        // ────────────────────────────────────────────────────────────
        public async Task<WeeklyScheduleViewModel> GetWeeklyScheduleAsync(DateTime? startDate = null)
        {
            var start = startDate ?? GetStartOfWeek(DateTime.Today);
            var end = start.AddDays(7);

            var classes = await uow.GymClasses.GetAllAsync()
                .Where(c => !c.IsDeleted && c.ScheduleTime >= start && c.ScheduleTime < end)
                .Include(c => c.Trainer)
                .Include(c => c.Attendees)
                .OrderBy(c => c.ScheduleTime)
                .ToListAsync();

            var vm = new WeeklyScheduleViewModel
            {
                WeekStart = start,
                WeekEnd = end.AddDays(-1),
                Days = []
            };

            for (int i = 0; i < 7; i++)
            {
                var dayDate = start.AddDays(i);
                vm.Days.Add(new DayScheduleViewModel
                {
                    Date = dayDate,
                    Classes = classes
                        .Where(c => c.ScheduleTime.Date == dayDate.Date)
                        .Select(MapToRow)
                        .ToList()
                });

                // Calculate overlaps for this day
                CalculateOverlaps(vm.Days[i].Classes);
            }

            return vm;
        }

        // ────────────────────────────────────────────────────────────
        //  ASSIGN MEMBER
        // ────────────────────────────────────────────────────────────
        public async Task<GymClassAssignMemberViewModel?> GetAssignMemberFormAsync(int gymClassId)
        {
            var gymClass = await uow.GymClasses.GetByIdAsync(gymClassId);
            if (gymClass is null || gymClass.IsDeleted) return null;

            var members = await uow.Members.GetAllAsync()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.FullName)
                .ToListAsync();

            return new GymClassAssignMemberViewModel
            {
                GymClassId = gymClass.Id,
                GymClassName = gymClass.Name,
                AttendanceDate = gymClass.ScheduleTime,
                MemberOptions = members.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.FullName} ({m.PhoneNumber})"
                })
            };
        }

        public async Task<(bool Success, string? Error)> AssignMemberAsync(GymClassAssignMemberViewModel model)
        {
            var gymClass = await uow.GymClasses.GetAllAsync()
                .Include(c => c.Attendees)
                .FirstOrDefaultAsync(c => c.Id == model.GymClassId && !c.IsDeleted);

            if (gymClass is null) return (false, "Gym class not found.");

            if (gymClass.Attendees.Count(a => !a.IsDeleted) >= gymClass.MaxCapacity)
                return (false, "This class has reached its maximum capacity.");

            if (gymClass.Attendees.Any(a => a.MemberId == model.MemberId && !a.IsDeleted))
                return (false, "This member is already assigned to this class.");

            // Create a minimal MemberVisit as required by ClassAttendance
            var visit = new MemberVisit
            {
                CreatedAt = DateTime.Now,
                Height = 0,
                Weight = 0,
                BMI = 0,
                Notes = $"Assigned to class: {gymClass.Name}"
            };

            await uow.MemberVisits.AddAsync(visit);

            var attendance = new ClassAttendance
            {
                GymClassId = model.GymClassId,
                MemberId = model.MemberId,
                MemberVisit = visit,
                AttendanceDate = model.AttendanceDate,
                CreatedAt = DateTime.Now
            };

            await uow.ClassAttendaces.AddAsync(attendance);
            await uow.CompleteAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RemoveMemberFromClassAsync(int gymClassId, int memberId)
        {
            var attendance = await uow.ClassAttendaces.GetAllAsync()
                .Include(a => a.MemberVisit)
                .FirstOrDefaultAsync(a => a.GymClassId == gymClassId && a.MemberId == memberId && !a.IsDeleted);

            if (attendance is null)
                return (false, "Attendance record not found.");

            // Soft delete both attendance and the unique visit linked to it
            attendance.IsDeleted = true;
            uow.ClassAttendaces.Update(attendance);

            if (attendance.MemberVisit != null)
            {
                attendance.MemberVisit.IsDeleted = true;
                uow.MemberVisits.Update(attendance.MemberVisit);
            }

            await uow.CompleteAsync();

            return (true, null);
        }

        private void CalculateOverlaps(List<GymClassRowViewModel> dayClasses)
        {
            if (!dayClasses.Any()) return;

            var sorted = dayClasses.OrderBy(c => c.ScheduleTime).ToList();
            var clumps = new List<List<GymClassRowViewModel>>();

            foreach (var c in sorted)
            {
                var clump = clumps.FirstOrDefault(cl => cl.Any(overlap =>
                    c.ScheduleTime < overlap.ScheduleTime.AddMinutes(overlap.DurationInMinutes) &&
                    overlap.ScheduleTime < c.ScheduleTime.AddMinutes(c.DurationInMinutes)));

                if (clump == null)
                {
                    clump = new List<GymClassRowViewModel>();
                    clumps.Add(clump);
                }
                clump.Add(c);
            }

            foreach (var clump in clumps)
            {
                var lanes = new List<DateTime>();
                foreach (var c in clump.OrderBy(x => x.ScheduleTime))
                {
                    int laneIndex = lanes.FindIndex(end => end <= c.ScheduleTime);
                    if (laneIndex == -1)
                    {
                        laneIndex = lanes.Count;
                        lanes.Add(c.ScheduleTime.AddMinutes(c.DurationInMinutes));
                    }
                    else
                    {
                        lanes[laneIndex] = c.ScheduleTime.AddMinutes(c.DurationInMinutes);
                    }
                    c.ColIndex = laneIndex;
                }
                foreach (var c in clump)
                {
                    c.ColCount = lanes.Count;
                }
            }
        }

        // ────────────────────────────────────────────────────────────
        //  PRIVATE HELPERS
        // ────────────────────────────────────────────────────────────
        private static DateTime GetStartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private async Task<IEnumerable<SelectListItem>> BuildTrainerOptionsAsync(
            int selectedId = 0)
        {
            var trainers = await trainerRepo
                .GetPagedAsync(1, 1000, t => !t.IsDeleted);

            return trainers
                .OrderBy(t => t.FullName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FullName,
                    Selected = t.Id == selectedId
                });
        }

        private static GymClassRowViewModel MapToRow(GymClass c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            TrainerName = c.Trainer?.FullName ?? "—",
            ScheduleTime = c.ScheduleTime,
            DurationInMinutes = c.DurationInMinutes,
            MaxCapacity = c.MaxCapacity,
            AttendeeCount = c.Attendees?.Count(a => !a.IsDeleted) ?? 0,
            CreatedAt = c.CreatedAt
        };
    }
}
