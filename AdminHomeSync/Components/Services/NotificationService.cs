using Firebase.Database;
using Firebase.Database.Query;
using System.Globalization;

namespace AdminHomeSync.Components.Services
{
    public class NotificationService
    {
        private readonly FirebaseClient _firebaseClient;

        public NotificationService()
        {
            _firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/");
        }

        // Fetch all notifications initially
        public async Task<List<NotificationItem>> GetAllNotificationsAsync()
        {
            var notifications = await FetchAllNotificationsFromFirebaseAsync();

            return notifications
                .OrderByDescending(n => ParseDateTime(n.Date, n.Time))
                .ToList();
        }

        // Real-time listener for notifications across all users
        public void ListenForAllNotifications(Action<NotificationItem> onNewNotification)
        {
            _firebaseClient
                .Child("notifications") // Listen to all notifications
                .AsObservable<NotificationItem>()
                .Subscribe(firebaseEvent =>
                {
                    if (firebaseEvent.Object != null)
                    {
                        onNewNotification(firebaseEvent.Object);
                    }
                });
        }

        // Fetch notifications for all users from Firebase
        private async Task<List<NotificationItem>> FetchAllNotificationsFromFirebaseAsync()
        {
            // Fetch all user notification nodes
            var userNotifications = await _firebaseClient
                .Child("notifications")
                .OnceAsync<Dictionary<string, NotificationItem>>();

            // Flatten notifications into a single list
            var notifications = userNotifications
                .SelectMany(userNode => userNode.Object.Values)
                .ToList();

            return notifications;
        }

        // Helper method to parse DateTime
        private DateTime ParseDateTime(string date, string time)
        {
            var combinedDateTime = $"{date} {time}";
            if (DateTime.TryParseExact(
                combinedDateTime,
                new[] { "MMMM d, yyyy h:mm tt", "MMM d, yyyy h:mm tt" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            return DateTime.MinValue;
        }

        public class NotificationItem
        {
            public string Action { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;

            public string FullDateTime => $"{Date} {Time}";
        }
    }
}
