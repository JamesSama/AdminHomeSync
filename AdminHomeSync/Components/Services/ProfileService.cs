using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminHomeSync.Components.Services
{
    public class ProfileService
    {
        private readonly FirebaseClient _firebaseClient;

        public ProfileService()
        {
            // Initialize Firebase client with your Firebase Realtime Database URL
            _firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/");
        }

        // Fetch profile data using the user ID
        public async Task<UserProfile> GetUserProfileAsync(string userId)
        {
            try
            {
                // Navigate to the 'activity' node and fetch data for the given userId
                var userProfile = await _firebaseClient
                    .Child("activity") // Access 'activity' node
                    .Child(userId)    // Access the specific user's node by ID
                    .OnceAsync<UserProfile>();

                // Check if data exists and return the first user profile
                var profile = userProfile.Select(u => u.Object).FirstOrDefault();

                if (profile == null)
                    throw new Exception("User profile not found.");

                return profile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching user profile: {ex.Message}");
            }
        }

        // User profile class to map the Firebase JSON structure
        public class UserProfile
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Birthdate { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string Sex { get; set; }
            public string UserId { get; set; }
        }
    }
}
