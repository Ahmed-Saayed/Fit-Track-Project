
using FitTrack_Pro.Models;

namespace Common
{
	public interface IUnitOfWork : IDisposable
	{
		IGenericRepository<Member> Members { get; }
		IGenericRepository<Trainer> Trainers { get; }
		IGenericRepository<ClassAttendance> ClassAttendaces { get; }
		IGenericRepository<GymClass> GymClasses { get; }
		IGenericRepository<MemberSubscription> MemberSubscriptions { get; }
		IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; }
		IGenericRepository<MemberVisit> MemberVisits { get; }


		Task<int> CompleteAsync();
	}
}
