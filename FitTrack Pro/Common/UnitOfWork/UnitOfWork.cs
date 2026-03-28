
using FitTrack_Pro.Models;

namespace Common
{
	public class UnitOfWork(ApplicationDbContext _context) : IUnitOfWork
	{
		public IGenericRepository<Member> Members { get; private set; } = new GenericRepository<Member>(_context);
		public IGenericRepository<Trainer> Trainers { get; private set; } = new GenericRepository<Trainer>(_context);
		public IGenericRepository<ClassAttendance> ClassAttendaces { get; private set; } = new GenericRepository<ClassAttendance>(_context);
		public IGenericRepository<GymClass> GymClasses { get; } = new GenericRepository<GymClass>(_context);
		public IGenericRepository<MemberSubscription> MemberSubscriptions { get; } = new GenericRepository<MemberSubscription>(_context);
		public IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; } = new GenericRepository<SubscriptionPlan>(_context);
		public IGenericRepository<MemberVisit> MemberVisits { get; } = new GenericRepository<MemberVisit>(_context);
		public IGenericRepository<SubscriptionPayment> SubscriptionPayments { get; private set; }= new GenericRepository<SubscriptionPayment>(_context);
		public async Task<int> CompleteAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
	}