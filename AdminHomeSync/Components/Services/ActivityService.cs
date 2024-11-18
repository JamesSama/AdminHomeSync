namespace AdminHomeSync.Components.Services
{
    public class ActivityService
    {
        private List<UserActivity> activities = new List<UserActivity>();

        // Method to add an activity
        public void AddActivity(DateTime dateTime, string userName, string role, string action, string status)
        {
            activities.Add(new UserActivity(dateTime, userName, role, action, status));
        }

        public List<UserActivity> GetActivities()
        {
            return activities;
        }
    }

    public class UserActivity
    {
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }

        // Constructor
        public UserActivity(DateTime dateTime, string userName, string role, string action, string status)
        {
            DateTime = dateTime;
            UserName = userName;
            Role = role;
            Action = action;
            Status = status;
        }
    }
}
