namespace RaceDay.Web.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        public string OrganiserName { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int CompletedEventsCount { get; set; }

        public List<EventViewModel> Events { get; set; } = new();
    }
}
