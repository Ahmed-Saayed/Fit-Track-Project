using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.ViewModels
{
	// ════════════════════════════════════════════════════════════════
	//  INDEX  –  paged list
	// ════════════════════════════════════════════════════════════════
	public class MemberIndexViewModel
	{
		public IEnumerable<MemberRowViewModel> Members { get; set; } = [];
		public int CurrentPage { get; set; } = 1;
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
		public int PageSize { get; set; } = 10;
		public string? SearchQuery { get; set; }

		public bool HasPrevious => CurrentPage > 1;
		public bool HasNext => CurrentPage < TotalPages;
	}

	// ────────────────────────────────────────────────────────────────
	//  Single row in the members table
	// ────────────────────────────────────────────────────────────────
	public class MemberRowViewModel
	{
		public int Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = string.Empty;
		public string? Gender { get; set; }
		public string? Barcode { get; set; }
		public int Age { get; set; }
		public string? ActivePlanName { get; set; }
		public DateTime? SubscriptionEnd { get; set; }
		public List<string> JoinedClasses { get; set; } = [];
		public MemberStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
	}



	// ════════════════════════════════════════════════════════════════
	//  DETAILS
	// ════════════════════════════════════════════════════════════════
	public class MemberDetailsViewModel
	{
		public int Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = string.Empty;
		public DateTime BirthDate { get; set; }
		public string? Gender { get; set; }
		public string? Barcode { get; set; }
		public string? MedicalNotes { get; set; }
		public DateTime CreatedAt { get; set; }

		public MemberSubscriptionViewModel? ActiveSubscription { get; set; }
		public IEnumerable<MemberSubscriptionViewModel> SubscriptionHistory { get; set; } = [];
		public IEnumerable<string> JoinedClasses { get; set; } = [];
		public int Age => (int)((DateTime.Today - BirthDate).TotalDays / 365.25);
		public MemberStatus Status => ActiveSubscription is { } s && s.EndDate >= DateTime.Today
			? MemberStatus.Active : MemberStatus.Expired;
	}

	// ════════════════════════════════════════════════════════════════
	//  CREATE / EDIT FORM
	// ════════════════════════════════════════════════════════════════
	public class MemberFormViewModel
	{
		public int Id { get; set; }   // 0 = create

		[Required(ErrorMessage = "Full Name is required")]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
		[Display(Name = "Full Name")]
		public string FullName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Phone Number is required")]
		[Phone(ErrorMessage = "Invalid phone number")]
		[StringLength(20)]
		[Display(Name = "Phone Number")]
		public string PhoneNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Birth Date is required")]
		[DataType(DataType.Date)]
		[Display(Name = "Date of Birth")]
		public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-25);

		[Required(ErrorMessage = "Gender is required")]
		[Display(Name = "Gender")]
		public string Gender { get; set; } = string.Empty;

		[Required(ErrorMessage = "Barcode is required")]
		[StringLength(50)]
		[Display(Name = "Barcode")]
		public string Barcode { get; set; } = string.Empty;

		[Display(Name = "Medical Notes")]
		public string? MedicalNotes { get; set; }

		// Account fields (for Create mode)
		[EmailAddress(ErrorMessage = "Invalid email address")]
		[Display(Name = "Email/Username")]
		public string? UserName { get; set; }

		[DataType(DataType.Password)]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		public string? Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string? ConfirmPassword { get; set; }
	}

	// ════════════════════════════════════════════════════════════════
	//  Subscription summary (used inside Details)
	// ════════════════════════════════════════════════════════════════
	public class MemberSubscriptionViewModel
	{
		public int Id { get; set; }
		public string PlanName { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal PaidAmount { get; set; }
		public bool IsActive { get; set; }
		public int RemainingDays => Math.Max(0, (EndDate - DateTime.Today).Days);
		public decimal PlanPrice { get; set; } //  plan standred price
		public decimal RemainingAmount => PlanPrice - PaidAmount; 
	}
	

	// ════════════════════════════════════════════════════════════════
	//  Dashboard stats card
	//  ════════════════════════════════════════════════════════════════
	public class MemberDashboardStatsViewModel
	{
		public int TotalMembers { get; set; }
		public int ActiveMembers { get; set; }
		public int ExpiredMembers { get; set; }
		public int ExpiringIn7Days { get; set; }
		public int NewThisMonth { get; set; }
	}

	// ════════════════════════════════════════════════════════════════
	//  ASSIGN PLAN
	// ════════════════════════════════════════════════════════════════

		public class AssignPlanViewModel
		{
			public int MemberId { get; set; }
			public string MemberName { get; set; } = string.Empty;

			public string? CurrentPlanName { get; set; }
			public DateTime? CurrentPlanEndDate { get; set; }

			[Required(ErrorMessage = "Please select a plan")]
			[Display(Name = "Subscription Plan")]
			public int SelectedPlanId { get; set; }

			[Required(ErrorMessage = "Start Date is required")]
			[DataType(DataType.Date)]
			[Display(Name = "Start Date")]
			public DateTime StartDate { get; set; } = DateTime.Today;

			public IEnumerable<PlanResponseViewModel> AvailablePlans { get; set; } = [];

			[Display(Name = "Initial Payment (EGP)")]
			[Range(0, double.MaxValue, ErrorMessage = "Payment amount cannot be negative.")]
			public decimal InitialPayment { get; set; }

			[Display(Name = "Payment Method")]
			public string PaymentMethod { get; set; } = "Cash";
		}

	// ════════════════════════════════════════════════════════════════
	//  MEMBER DASHBOARD
	// ════════════════════════════════════════════════════════════════
	public class MemberDashboardViewModel
	{
		public int MemberId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Barcode { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public int Age { get; set; }

		public MemberSubscriptionViewModel? ActiveSubscription { get; set; }
		public IEnumerable<AttendedClassViewModel> AttendedClasses { get; set; } = [];
	}

	public class AttendedClassViewModel
	{
		public string ClassName { get; set; } = string.Empty;
		public string TrainerName { get; set; } = string.Empty;
		public DateTime AttendanceDate { get; set; }
		public int DurationInMinutes { get; set; }
	}

	// ════════════════════════════════════════════════════════════════
	//  Shared enum
	// ════════════════════════════════════════════════════════════════
	public enum MemberStatus
	{
		Active,
		Expired,
		NeverSubscribed
	}
}