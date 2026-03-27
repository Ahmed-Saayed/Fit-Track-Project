using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack_Pro.ViewModels
{
    // ════════════════════════════════════════════════════════════════
    //  INDEX  –  paged list
    // ════════════════════════════════════════════════════════════════
    public class GymClassIndexViewModel
    {
        public IEnumerable<GymClassRowViewModel> GymClasses { get; set; } = [];
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public string? SearchQuery { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    // ────────────────────────────────────────────────────────────────
    //  Single row in the gym classes table/schedule
    // ────────────────────────────────────────────────────────────────
    public class GymClassRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TrainerName { get; set; } = string.Empty;
        public DateTime ScheduleTime { get; set; }
        public int DurationInMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public int AttendeeCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Positioning for schedule view
        public int ColIndex { get; set; } = 0;
        public int ColCount { get; set; } = 1;
    }

    // ════════════════════════════════════════════════════════════════
    //  DETAILS
    // ════════════════════════════════════════════════════════════════
    public class GymClassDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public DateTime ScheduleTime { get; set; }
        public int DurationInMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public int AttendeeCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<AttendeeViewModel> Attendees { get; set; } = [];
    }

    public class AttendeeViewModel
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberPhoneNumber { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  CREATE / EDIT FORM
    // ════════════════════════════════════════════════════════════════
    public class GymClassFormViewModel
    {
        public int Id { get; set; }   // 0 = create

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Class Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a trainer")]
        [Display(Name = "Trainer")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Schedule date & time is required")]
        [Display(Name = "Schedule Date & Time")]
        public DateTime ScheduleTime { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Duration is required")]
        [Range(5, 480, ErrorMessage = "Duration must be between 5 and 480 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int DurationInMinutes { get; set; } = 60;

        [Required(ErrorMessage = "Max capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; } = 20;

        // Populated by the service for the trainer dropdown
        public IEnumerable<SelectListItem> TrainerOptions { get; set; } = [];
    }

    // ════════════════════════════════════════════════════════════════
    //  WEEKLY SCHEDULE
    // ════════════════════════════════════════════════════════════════
    public class WeeklyScheduleViewModel
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DayScheduleViewModel> Days { get; set; } = [];
        
        // For navigation
        public DateTime PreviousWeek => WeekStart.AddDays(-7);
        public DateTime NextWeek => WeekStart.AddDays(7);
    }

    public class DayScheduleViewModel
    {
        public DateTime Date { get; set; }
        public string DayName => Date.ToString("ddd");
        public int DayNumber => Date.Day;
        public List<GymClassRowViewModel> Classes { get; set; } = [];
    }

    // ════════════════════════════════════════════════════════════════
    //  ASSIGN MEMBER FORM
    // ════════════════════════════════════════════════════════════════
    public class GymClassAssignMemberViewModel
    {
        public int GymClassId { get; set; }
        public string GymClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Attendance date is required")]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; } = DateTime.Now;

        public IEnumerable<SelectListItem> MemberOptions { get; set; } = [];
    }
}
