namespace AdminHomeSync.Components.Services
{
    public class UserService : IUserService
    {
        private List<User> users;

        public UserService()
        {
            // Simulate fetching data, palitan later
            users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Name = "Jobert Batumbakal",
                    Role = "User",
                    DevicesConnected = 2,
                    Status = "Active",
                    LastLogin = DateTime.Now.AddHours(-1),
                    Actions = "Logged in"
                },
                new User
                {
                    Id = 2,
                    Name = "Meow M. Meow",
                    Role = "Admin",
                    DevicesConnected = 5,
                    Status = "Inactive",
                    LastLogin = DateTime.Now.AddDays(-1),
                    Actions = "Logged out"
                },
                new User
                {
                    Id = 3,
                    Name = "Ed Caluag",
                    Role = "Admin",
                    DevicesConnected = 2,
                    Status = "Inactive",
                    LastLogin = DateTime.Now.AddDays(-1),
                    Actions = "Logged out"
                },
                new User
                {
                    Id = 4,
                    Name = "Arman",
                    Role = "User",
                    DevicesConnected = 1,
                    Status = "Active",
                    LastLogin = DateTime.Now.AddHours(-3),
                    Actions = "New User"
                },
                new User
                {
                    Id = 5,
                    Name = "Meow M. Meow",
                    Role = "Admin",
                    DevicesConnected = 5,
                    Status = "Inactive",
                    LastLogin = DateTime.Now.AddDays(-1),
                    Actions = "Logged out"
                },
                new User
                {
                    Id = 6,
                    Name = "Ed Caluag",
                    Role = "Admin",
                    DevicesConnected = 2,
                    Status = "Inactive",
                    LastLogin = DateTime.Now.AddDays(-1),
                    Actions = "Logged out"
                },
                new User
                {
                    Id = 7,
                    Name = "Arman",
                    Role = "User",
                    DevicesConnected = 1,
                    Status = "Active",
                    LastLogin = DateTime.Now.AddHours(-3),
                    Actions = "New User"
                }
            };
        }

        public async Task<List<User>> GetUsersAsync()
        {
            // Simulate asynchronous data fetch
            return await Task.FromResult(users);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await Task.FromResult(users.Find(u => u.Id == userId) ?? new User());
        }

        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            // Simulate updating user details
            var userIndex = users.FindIndex(u => u.Id == updatedUser.Id);
            if (userIndex >= 0)
            {
                users[userIndex] = updatedUser;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // For filtering users in search bar 
        public async Task<List<User>> SearchUsersAsync(string searchTerm, string roleFilter)
        {
            var filteredUsers = users.AsQueryable();

            // Filter by role if specified
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
            {
                filteredUsers = filteredUsers.Where(u =>
                    u.Role != null &&
                    u.Role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by search term for both Name and Role
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredUsers = filteredUsers.Where(u =>
                    (u.Name != null && u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Role != null && u.Role.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            return await Task.FromResult(filteredUsers.ToList());
        }

    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public int DevicesConnected { get; set; }
        public string? Status { get; set; }
        public DateTime LastLogin { get; set; }
        public string? Actions { get; set; }
    }


    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<List<User>> SearchUsersAsync(string searchTerm, string roleFilter);  // New search method
    }
}
