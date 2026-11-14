using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (KM)")]
        public decimal DistanceKM { get; set; }

        [Required(ErrorMessage = "Entry fee is required.")]
        [Range(0, 10000, ErrorMessage = "Entry fee must be positive.")]
        [Display(Name = "Entry Fee (ZAR)")]
        public decimal EntryFee { get; set; }

        [Required(ErrorMessage = "Maximum participants is required.")]
        [Range(1, 100000, ErrorMessage = "Capacity must be at least 1.")]
        [Display(Name = "Maximum Participants")]
        public int MaxParticipants { get; set; }

        public int EnrolmentsCount { get; set; }
        public int AvailableSpots { get; set; }
    }

    public class CreateCategoryViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category name is required (e.g. 42.2km Full Marathon).")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (KM)")]
        public decimal DistanceKM { get; set; }

        [Required(ErrorMessage = "Entry fee is required.")]
        [Range(0, 10000, ErrorMessage = "Entry fee must be zero or greater.")]
        [Display(Name = "Entry Fee (ZAR)")]
        public decimal EntryFee { get; set; }

        [Required(ErrorMessage = "Maximum capacity is required.")]
        [Range(1, 100000, ErrorMessage = "Capacity must be at least 1.")]
        [Display(Name = "Capacity (Max Participants)")]
        public int MaxParticipants { get; set; } = 1000;
    }
}
