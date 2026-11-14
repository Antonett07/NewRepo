using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKM { get; set; }

        public string? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int TotalFinishers { get; set; }
        public int TotalCategoryFinishers { get; set; }

        public string Status { get; set; } = "Finished";
        public int? RaceNumber { get; set; }

        public string PositionFormatted
        {
            get
            {
                if (!OverallPosition.HasValue) return "Unranked";
                return $"{GetOrdinal(OverallPosition.Value)} out of {TotalFinishers} finishers";
            }
        }

        public string CategoryPositionFormatted
        {
            get
            {
                if (!CategoryPosition.HasValue) return "Unranked";
                return $"{GetOrdinal(CategoryPosition.Value)} in category";
            }
        }

        private static string GetOrdinal(int number)
        {
            if (number <= 0) return number.ToString();
            int rem100 = number % 100;
            if (rem100 >= 11 && rem100 <= 13) return $"{number}th";
            return (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };
        }
    }

    public class CreateResultViewModel
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required(ErrorMessage = "Finish time is required (e.g. 02:14:35).")]
        [RegularExpression(@"^([0-9]{1,2}:)?[0-5]?[0-9]:[0-5][0-9]$", ErrorMessage = "Please enter time in hh:mm:ss format.")]
        [Display(Name = "Finish Time (hh:mm:ss)")]
        public string? FinishTimeString { get; set; }

        [Required(ErrorMessage = "Overall position is required.")]
        [Range(1, 100000, ErrorMessage = "Position must be 1 or greater.")]
        [Display(Name = "Overall Finishing Position")]
        public int? OverallPosition { get; set; }

        [Range(1, 100000, ErrorMessage = "Category position must be 1 or greater.")]
        [Display(Name = "Category Position")]
        public int? CategoryPosition { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Finished";
    }

    public class EventResultsEntryViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

        public List<ParticipantResultRowViewModel> Participants { get; set; } = new();
    }

    public class ParticipantResultRowViewModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int? RaceNumber { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;

        public int? ExistingResultId { get; set; }
        public string? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public string Status { get; set; } = "Finished";
    }
}
