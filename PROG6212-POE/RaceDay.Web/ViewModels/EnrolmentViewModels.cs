using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string? ParticipantEmail { get; set; }
        public string? ParticipantPhone { get; set; }

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKM { get; set; }
        public decimal EntryFee { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string PaymentStatus { get; set; } = "Confirmed";
        public int? RaceNumber { get; set; }

        public ResultViewModel? Result { get; set; }

        public bool IsConfirmed => PaymentStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
                                   PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);

        public bool IsPending => PaymentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase);

        public string StatusBadgeClass => IsConfirmed ? "bg-success" : (IsPending ? "bg-warning text-dark" : "bg-danger");
    }

    public class EnrolRequestViewModel
    {
        [Required]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please select a category to enrol in.")]
        public int CategoryId { get; set; }

        public string? PaymentStatus { get; set; } = "Confirmed";
    }
}
