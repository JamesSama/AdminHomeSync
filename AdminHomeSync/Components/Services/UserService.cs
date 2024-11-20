using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore; 
using System.Collections.Generic;
using System.Linq;
using FirebaseAdmin;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;



namespace AdminHomeSync.Components.Services
{
    public class UserService : IUserService
    {
        private readonly FirebaseClient firebaseClient;
        public UserService()
        {
            firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/users");

        }

        public async Task AddUserAsync(User user)
            {
                await firebaseClient
                    .Child("users")
                    .PostAsync(user);
            }
        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();

            // Fetch data from db
            var userItems = await firebaseClient
                .Child("users")
                .OnceAsync<User>();

            // Add fetched users to the list
            foreach (var userItem in userItems)
            {
                users.Add(userItem.Object);
            }

            return users;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var userItems = await firebaseClient
                .Child("users")
                .OnceAsync<User>();

            var user = userItems.FirstOrDefault(u => u.Object.UserId == userId.ToString());

            return user?.Object;  // Use null-conditional operator
        }

        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            try
            {
                // Update user in the 'users' node using their UserId as the key
                await firebaseClient
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

        // For filtering users in search bar 
        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            var users = await GetUsersAsync();
            var filteredUsers = users.AsQueryable();

            // if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
            // {
            //     filteredUsers = filteredUsers.Where(u =>
            //         u.Role != null 
            //         // &&
            //         u.Role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase)
            //         );
            // }

         
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredUsers = filteredUsers.Where(u =>
                !string.IsNullOrEmpty(u.Name) && u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return filteredUsers.ToList();
        }
        
    }



    public class User
    {
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name => $"{FirstName} {LastName}";
        public string? Role { get; set; }
        public int DevicesConnected { get; set; }
        public string? Status { get; set; }
        public string? Sex { get; set; }
        public DateTime LastLogin { get; set; }
        public string? Actions { get; set; }
    }


    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<List<User>> SearchUsersAsync(string searchTerm);  // New search method
    }
}
