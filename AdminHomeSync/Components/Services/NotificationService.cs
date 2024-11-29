using System.Net.Http.Json;
using System.Globalization;

namespace AdminHomeSync.Components.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Public method that components will call
        public async Task<List<NotificationItem>> GetNotificationsAsync(string userId)
        {
            // Fetch notifications and sort them
            var notifications = await FetchNotificationsFromFirebaseAsync(userId);

            // Sort notifications by Date and Time (newest first)
            notifications = notifications
                .OrderByDescending(n => ParseDateTime(n.Date, n.Time))  // Sorting by DateTime
                .ToList();

            return notifications;
        }

        // Parse Date and Time to DateTime
        private DateTime ParseDateTime(string date, string time)
        {
            var combinedDateTime = $"{date} {time}"; // Combine Date and Time for sorting

            // Try parsing the combined DateTime string
            if (DateTime.TryParseExact(
                combinedDateTime,
                new[] { "MMMM d, yyyy h:mm tt", "MMM d, yyyy h:mm tt" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            return DateTime.MinValue; // Return the minimum value if parsing fails
        }

        // Child method that fetches data from Firebase
        private async Task<List<NotificationItem>> FetchNotificationsFromFirebaseAsync(string userId)
        {
            var firebaseUrl = $"https://homesync-3be92-default-rtdb.firebaseio.com/notifications/{userId}.json";
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, NotificationItem>>(firebaseUrl);

            return response?.Values.ToList() ?? new List<NotificationItem>();
        }

        // Notification data model
        public class NotificationItem
        {
            public string Message { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;

            // Combining Date and Time into a FullDateTime string for display
            public string FullDateTime => $"{Date} • {Time}";
        }
    }
}
