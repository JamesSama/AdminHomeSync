namespace AdminHomeSync.Components.Services
{

    public class NotificationService
    {
        public List<NotificationItem> GetNotifications()
        {
            return new List<NotificationItem>
        {
            new NotificationItem { Message = "first message", Date = "October 24, 2024 • 8:15 PM" },
            new NotificationItem { Message = "second message", Date = "October 24, 2024 • 8:15 PM" },
        };
        }

        public class NotificationItem
        {
            public string Message { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
        }
    }

}
