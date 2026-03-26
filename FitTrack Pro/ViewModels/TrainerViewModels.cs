using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.ViewModels
{
    // ════════════════════════════════════════════════════════════════
    //  INDEX  –  paged list
    // ════════════════════════════════════════════════════════════════
    public class TrainerIndexViewModel
    {
        public IEnumerable<TrainerRowViewModel> Trainers { get; set; } = [];
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public string? SearchQuery { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    // ────────────────────────────────────────────────────────────────
    //  Single row in the trainers table
    // ────────────────────────────────────────────────────────────────
    public class TrainerRowViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public decimal SalaryOrPercentage { get; set; }
        public int ClassCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  DETAILS
    // ════════════════════════════════════════════════════════════════
    public class TrainerDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public decimal SalaryOrPercentage { get; set; }
        public DateTime CreatedAt { get; set; }

        public IEnumerable<TrainerClassViewModel> AssignedClasses { get; set; } = [];
    }

    // ════════════════════════════════════════════════════════════════
    //  CREATE / EDIT FORM
    // ════════════════════════════════════════════════════════════════
    public class TrainerFormViewModel
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

        [Required(ErrorMessage = "Specialty is required")]
        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        [Display(Name = "Specialty")]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary / Percentage is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Must be a positive value")]
        [Display(Name = "Salary / Percentage")]
        public decimal SalaryOrPercentage { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  Class summary (used inside Trainer Details)
    // ════════════════════════════════════════════════════════════════
    public class TrainerClassViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ScheduleTime { get; set; }
        public int DurationInMinutes { get; set; }
        public int MaxCapacity { get; set; }
    }
}
