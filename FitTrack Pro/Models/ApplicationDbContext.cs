using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace FitTrack_Pro.Models
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
	{
        public DbSet<Member> Members { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<MemberSubscription> MembersSubscription { get; set; }
        public DbSet<MemberVisit> MemberVisits { get; set; }
        public DbSet<ClassAttendance> classAttendances { get; set; }
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
