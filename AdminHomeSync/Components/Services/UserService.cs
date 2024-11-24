using Firebase.Database;
using Firebase.Database.Query;
using System.Globalization;  // For CultureInfo and DateTimeStyles

namespace AdminHomeSync.Components.Services
{
    public class UserService : IUserService
    {
        private readonly FirebaseClient firebaseClient;

        public UserService()
        {
            firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/");
        }

        public async Task AddUserAsync(User user)
        {
            await firebaseClient
                .Child("users")
                .Child("users")
                .PostAsync(user);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();

            var userItems = await firebaseClient
                .Child("users")
                .Child("users")
                .OnceAsync<User>();

            var usersWithSequentialIds = userItems
                .Select(userItem => new User
                {
                    UserId = userItem.Object.UserId,
                    FirstName = userItem.Object.FirstName,
                    LastName = userItem.Object.LastName,
                    Role = userItem.Object.Role,
                    LastLogin = userItem.Object.LastLogin,
                    Actions = userItem.Object.Actions,
                })
                .OrderByDescending(u => u.LastLogin)
                .Select((user, index) => new User
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    LastLogin = user.LastLogin,
                    Actions = user.Actions,
                    SequentialId = userItems.Count() - index
                })
                .ToList();

            return usersWithSequentialIds;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            var userItems = await firebaseClient
                .Child("users")
                .Child("users")
                .OnceAsync<User>();

            var user = userItems.FirstOrDefault(u => u.Object.UserId == userId);

            return user?.Object;
        }


        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            try
            {
                await firebaseClient
                    .Child("users")
                    .Child("users")
                    .Child(updatedUser.UserId)
                    .PutAsync(updatedUser);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            var users = await GetUsersAsync();
            var filteredUsers = users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredUsers = filteredUsers.Where(u =>
                !string.IsNullOrEmpty(u.Name) && u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return filteredUsers.ToList();
        }

        public async Task<UserSummation> GetUserSummationAsync()
        {
            var users = await GetUsersAsync();
            Console.WriteLine($"Fetched {users.Count} users.");

            int totalUsers = users.Count;

            return new UserSummation
            {
                TotalUsers = totalUsers
            };
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                // Get the user details before deletion
                var userToDelete = await GetUserByIdAsync(userId);

                if (userToDelete != null)
                {
                    // Delete the user from the 'users' node
                    await firebaseClient
                        .Child("users")
                        .Child("users")
                        .Child(userId)
                        .DeleteAsync();

                    // Append deletion activity in the 'activity' node
                    await AppendDeletionActivityAsync(userToDelete);

                    return true;
                }

                return false; // User not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }


        // Append the deletion activity in the 'activity' node
        private async Task AppendDeletionActivityAsync(User deletedUser)
        {
            var actionDate = DateTime.Now.ToString("MMMM dd, yyyy"); // Format the current date
            var actionTime = DateTime.Now.ToString("hh:mm tt");   // Format the current time in 12-hour format with AM/PM

            var activity = new
            {
                Action = "User Deleted",
                Date = actionDate,
                Time = actionTime,
                UserID = deletedUser.UserId,
                FirstName = deletedUser.FirstName,
                LastName = deletedUser.LastName,
                Role = deletedUser.Role,
                Sex = deletedUser.Sex,
                Email = deletedUser.Email,
                BirthDate = deletedUser.Birthdate
            };

            // Append activity log in the 'activity' node
            await firebaseClient
                .Child("activity")
                .PostAsync(activity);
        }

        public async Task<string?> GetLatestLoginForUserAsync(string userId)
        {
            try
            {
                // Fetch all activities for the given userId from Firebase
                var activityItems = await firebaseClient
                    .Child("activity")
                    .Child(userId)  // Querying under the userId node
                    .OnceAsync<object>();  // Using object to handle different data types

                // If no activities are found for the user
                if (activityItems == null || !activityItems.Any())
                {
                    return "No login activity found for this user.";
                }

                // List to store parsed login activities
                var loginActivities = new List<dynamic>();

                // Iterate over each activity and filter out "User logged in" actions
                foreach (var activityItem in activityItems)
                {
                    // Treat activityItem.Object as a dynamic object
                    dynamic activity = activityItem.Object;

                    // Ensure that the required fields exist in the activity
                    if (activity == null ||
                        activity.Action == null ||
                        activity.Date == null ||
                        activity.Time == null ||
                        activity.userId == null)
                    {
                        continue;
                    }

                    // Extract fields from the dynamic object
                    string action = activity.Action;
                    string date = activity.Date;
                    string time = activity.Time;
                    string activityUserId = activity.userId;

                    // Ensure the action is "User logged in" and the date/time are valid
                    if (action != null && date != null && time != null && activityUserId == userId && action.Contains("User logged in"))
                    {
                        // Trim whitespace from date and time
                        date = date.Trim();
                        time = time.Trim();

                        // Try parsing the date in the "MMMM dd, yyyy" format
                        DateTime activityDate = DateTime.MinValue;
                        DateTime activityTime = DateTime.MinValue;

                        bool dateParsed = DateTime.TryParseExact(date,
                            "MMMM dd, yyyy",  // Only supporting this format
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out activityDate);

                        // Normalize time format (e.g., convert "pm" to "PM")
                        time = time.ToUpper(); // Ensure time is in uppercase (e.g., "3:00 PM")

                        // Attempt to parse the time using the "hh:mm tt" format (case-insensitive)
                        bool timeParsed = DateTime.TryParseExact(time, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out activityTime);

                        if (dateParsed && timeParsed)
                        {
                            var loginDateTime = activityDate.Date.Add(activityTime.TimeOfDay);
                            loginActivities.Add(new
                            {
                                LoginDateTime = loginDateTime,
                                Action = action
                            });
                        }
                    }
                }

                // If no login activities were found
                if (!loginActivities.Any())
                {
                    return "No login activity found for this user.";
                }

                // Get the most recent login by sorting the activities by LoginDateTime in descending order
                var latestLogin = loginActivities
                    .OrderByDescending(a => a.LoginDateTime)
                    .First();

                return $"{latestLogin.LoginDateTime:MMMM dd, yyyy} at {latestLogin.LoginDateTime:hh:mm tt}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching login activity: {ex.Message}");
                return null;
            }
        }



    }

    public class User
    {
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name => $"{FirstName} {LastName}";
        public string? Role { get; set; }
        public DateTime LastLogin { get; set; }
        public string? Actions { get; set; }
        public int SequentialId { get; set; }
        public string? Sex { get; set; }
        public string? Email { get; set; }
        public string? Birthdate { get; set; }
    }

    public class UserSummation
    {
        public int TotalUsers { get; set; }
    }

    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<List<User>> SearchUsersAsync(string searchTerm);
        Task<UserSummation> GetUserSummationAsync();
        Task<bool> DeleteUserAsync(string userId);
        Task<string?> GetLatestLoginForUserAsync(string userId);
    }

}
