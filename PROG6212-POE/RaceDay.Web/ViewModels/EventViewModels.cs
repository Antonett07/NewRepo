using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;

        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Location { get; set; } = string.Empty;

        [Display(Name = "Event Date & Time")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Type")]
        public string EventType { get; set; } = "Road Running";

        public string? Distance { get; set; }

        public string? BannerImageUrl { get; set; }

        public int TotalEnrolments { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();

        public bool IsUpcoming => EventDate >= DateTime.Now;

        public string FormattedDate => EventDate.ToString("dddd, dd MMMM yyyy 'at' HH:mm");
        public string ShortDate => EventDate.ToString("dd MMM yyyy");
    }

    public class EventDetailViewModel
    {
        public EventViewModel Event { get; set; } = new();
        public bool IsUserEnrolled { get; set; }
        public int? EnrolledCategoryId { get; set; }
        public string? UserEnrolmentStatus { get; set; }
        public string? UserRole { get; set; }
    }

    public class CreateEventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(150, ErrorMessage = "Event name cannot exceed 150 characters.")]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(150, ErrorMessage = "Location cannot exceed 150 characters.")]
        [Display(Name = "Location / Venue")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [Display(Name = "Event Date & Start Time")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(14);

        [Required(ErrorMessage = "Event type is required.")]
        [Display(Name = "Sport / Event Type")]
        public string EventType { get; set; } = "Road Running";

        [Display(Name = "Primary Distance (e.g. 42.2km, 21km, 109km)")]
        public string? Distance { get; set; }

        public string? ExistingBannerUrl { get; set; }

        [Display(Name = "Event Banner Image")]
        public IFormFile? BannerImage { get; set; }
    }

    public class EventFilterViewModel
    {
        public string? Search { get; set; }
        public string? Location { get; set; }
        public string? EventType { get; set; }
        public List<EventViewModel> Events { get; set; } = new();
    }
}
