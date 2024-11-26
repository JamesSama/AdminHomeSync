using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace AdminHomeSync.Components.Services
{
    public class ActivityService
    {
        private readonly FirebaseClient firebaseClient;

        public ActivityService()
        {
            firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/");
            Console.WriteLine("Firebase connection initialized.");
        }

        public async Task<List<UserActivity>> FetchActivitiesFromFirebase()
        {
            try
            {
                var activities = new List<UserActivity>();

                var firebaseActivities = await firebaseClient
                    .Child("activity")
                    .OnceAsync<Dictionary<string, Dictionary<string, object>>>();

                foreach (var userNode in firebaseActivities)
                {
                    var userActivities = userNode.Object;

                    foreach (var activityEntry in userActivities)
                    {
                        var activityData = activityEntry.Value;

                        if (activityData.ContainsKey("Date") && activityData.ContainsKey("Time") &&
                            activityData.ContainsKey("Action") && activityData.ContainsKey("FirstName"))
                        {
                            string date = activityData["Date"].ToString();
                            string time = activityData["Time"].ToString();
                            string action = activityData["Action"].ToString();
                            string firstName = activityData["FirstName"].ToString();
                            string lastName = activityData.ContainsKey("LastName") ? activityData["LastName"].ToString() : "";
                            string role = activityData.ContainsKey("Role") ? activityData["Role"].ToString() : "Unknown";
                            string status = "Success";

                            try
                            {
                                if (activityData.ContainsKey("Status"))
                                {
                                    status = activityData["Status"].ToString();
                                }
                            }
                            catch (Exception)
                            {
                                status = "Unknown"; // If any error occurs, status shows Unknown
                            }

                            string combinedDateTime = $"{date} {time}";
                            if (DateTime.TryParse(combinedDateTime, out DateTime parsedDateTime))
                            {
                                var userActivity = new UserActivity
                                {
                                    DateTime = parsedDateTime,
                                    FirstName = firstName,
                                    LastName = lastName,
                                    Role = role,
                                    Action = action,
                                    Status = status
                                };

                                activities.Add(userActivity);
                            }
                        }
                    }
                }

                return activities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data from Firebase: {ex.Message}");
                return new List<UserActivity>(); // Return an empty list on error
            }
        }

    }

    public class UserActivity
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName => $"{FirstName} {LastName}";

        public UserActivity() { }

        public UserActivity(string date, string time, string firstName, string lastName, string role, string action)
        {
            Date = date;
            Time = time;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            Action = action;

            // Parse the date and time
            if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
            {
                string combinedDateTime = $"{date} {time}";
                if (DateTime.TryParseExact(combinedDateTime,
                                           new[] { "MMMM d, yyyy h:mm tt", "MMMM d, yyyy h:mm tt" },
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out DateTime parsedDateTime))
                {
                    DateTime = parsedDateTime;
                }
                else
                {
                    Console.WriteLine($"Failed to parse DateTime: {combinedDateTime}");
                    DateTime = DateTime.Now; // Fallback
                }
            }
            else
            {
                Console.WriteLine("Date or time missing");
                DateTime = DateTime.Now; // Fallback
            }

            Status = "Success"; // Default value
        }
    }
}

