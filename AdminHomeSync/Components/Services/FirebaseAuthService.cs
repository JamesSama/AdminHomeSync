using Blazored.LocalStorage;

public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private const string FirebaseApiKey = "AIzaSyBcyyz9rrgkx7Uby4YN6qWXQlImlEWxHjw";
    private const string FirebaseAuthUrlSignUp = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
    private const string FirebaseAuthUrlSignIn = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
    public const string FirebaseDatabaseUrl = "https://homesync-3be92-default-rtdb.firebaseio.com/";
    private const string FirebaseAuthUrlDeleteUser = "https://identitytoolkit.googleapis.com/v1/accounts:delete";

    private string _idToken;
    private string _userId;
    private UserData _userProfile;

    public FirebaseAuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public HttpClient HttpClient => _httpClient;

    // Sign-up method
    public async Task<FirebaseSignUpResponse> SignUpAsync(string email, string password, string firstName, string lastName, string sex)
    {
        var request = new { email, password, returnSecureToken = true };
        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlSignUp}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseSignUpResponse>();

            // Add the user activity logging after successful sign-up
            await LogSignUpActivityAsync(result.LocalId, email, firstName, lastName);

            return result;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sign-up failed: {error}");
        }
    }

    // Add user to the database
    public async Task AddUserToDatabaseAsync(string userId, object userData, string idToken)
    {   
        // Make sure to use the correct Firebase Realtime Database path
        var requestUrl = $"{FirebaseDatabaseUrl}/users/users/{userId}.json?auth={idToken}";

        // Send data to Firebase Realtime Database
        var response = await _httpClient.PutAsJsonAsync(requestUrl, userData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to add user to database: {error}");
        }
    }

    // Log user activity (new admin registration, etc.)
    public async Task LogSignUpActivityAsync(string userId, string Email, string FirstName, string LastName)
    {
        // Get current date and time
        var Date = DateTime.Now.ToString("MMMM dd, yyyy");  // Date format "December 01, 2024"
        var Time = DateTime.Now.ToString("hh:mm tt");  // Time format "08:45 AM"

        // Create activity data
        var activityData = new
        {
            Action = "New Admin",  // Action representing the event
            Date = Date,           // Date when the activity occurred
            Time = Time,           // Time when the activity occurred
            Email = Email,         // User's email address
            FirstName = FirstName, // User's first name
            LastName = LastName,   // User's last name
            Role = "Admin",        // Default role for new users
            UserId = userId        // Firebase user ID
        };

        // Convert the activityData to a dictionary and capitalize the keys
        var capitalizedActivityData = activityData.GetType()
            .GetProperties()
            .ToDictionary(
                prop => CapitalizeFirstLetter(prop.Name), // Capitalize the property name
                prop => prop.GetValue(activityData, null) // Get the property value
            );

        // Construct request URL for Firebase activity logging
        var requestUrl = $"{FirebaseDatabaseUrl}/activity/{userId}.json";

        // Send POST request to Firebase to log the activity
        var response = await _httpClient.PostAsJsonAsync(requestUrl, capitalizedActivityData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to log user activity: {error}");
        }
    }

    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }


    public async Task<FirebaseSignInResponse> SignInWithEmailAndPasswordAsync(string email, string password)
    {
        var request = new { email, password, returnSecureToken = true };
        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlSignIn}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
            _idToken = result.IdToken;
            _userId = result.LocalId;

            // Store token and user ID in local storage
            await _localStorage.SetItemAsync("authToken", _idToken);
            await _localStorage.SetItemAsync("userId", _userId);

            // Fetch user data
            _userProfile = await GetUserDataFromDatabaseAsync(_userId, _idToken);

            // Store user profile in local storage
            await _localStorage.SetItemAsync("userProfile", _userProfile);

            // Log admin login activity in Firebase
            await LogAdminLoginActivityAsync(_userId, email, _userProfile.FirstName, _userProfile.LastName);

            return result;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sign-in failed: {error}");
        }
    }

    // Log admin login activity method
    private async Task LogAdminLoginActivityAsync(string userId, string email, string firstName, string lastName)
    {
        // Get current date and time
        var date = DateTime.Now.ToString("MMMM dd, yyyy");  // e.g., "December 01, 2024"
        var time = DateTime.Now.ToString("hh:mm tt");       // e.g., "08:45 AM"

        // Create activity data
        var loginActivityData = new
        {
            Action = "Admin Logged In",  // Action representing the event
            Date = date,                 // Date when the login occurred
            Time = time,                 // Time when the login occurred
            Email = email,               // Admin's email address
            FirstName = firstName,       // Admin's first name
            LastName = lastName,         // Admin's last name
            Role = "Admin",              // Admin role
            UserId = userId              // Firebase user ID
        };

        // Convert the activityData to a dictionary and capitalize the keys
        var capitalizedLoginActivityData = loginActivityData.GetType()
            .GetProperties()
            .ToDictionary(
                prop => CapitalizeFirstLetter(prop.Name), // Capitalize the property name
                prop => prop.GetValue(loginActivityData, null) // Get the property value
            );

        // Construct request URL for Firebase activity logging
        var requestUrl = $"{FirebaseDatabaseUrl}/activity/{userId}.json";

        // Send POST request to Firebase to log the login activity
        var response = await _httpClient.PostAsJsonAsync(requestUrl, capitalizedLoginActivityData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to log admin login activity: {error}");
        }
    }

    public async Task<UserData> GetUserDataFromDatabaseAsync(string userId, string idToken)
    {
        var requestUrl = $"{FirebaseDatabaseUrl}/users/users/{userId}.json?auth={idToken}";
        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
            var userData = await response.Content.ReadFromJsonAsync<UserData>();
            return userData;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve user data: {error}");
        }
    }

    public async Task<UserData> GetCurrentUserAsync()
    {
        if (_userProfile != null)
            return _userProfile;

        // Try to load from local storage
        _idToken = await _localStorage.GetItemAsync<string>("authToken");
        _userId = await _localStorage.GetItemAsync<string>("userId");

        Console.WriteLine($"ID Token: {_idToken}, User ID: {_userId}");

        if (!string.IsNullOrEmpty(_idToken) && !string.IsNullOrEmpty(_userId))
        {
            _userProfile = await _localStorage.GetItemAsync<UserData>("userProfile");
            Console.WriteLine($"User Profile: {_userProfile?.FirstName}, {_userProfile?.LastName}");
        }

        return _userProfile;
    }

    // Method to log admin logout activity
    public async Task LogAdminLogoutActivityAsync(string userId, string email, string firstName, string lastName)
    {
        var date = DateTime.Now.ToString("MMMM dd, yyyy");
        var time = DateTime.Now.ToString("hh:mm tt");

        var logoutActivityData = new
        {
            Action = "Admin Logged Out",
            Date = date,
            Time = time,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = "Admin",
            UserId = userId
        };

        var capitalizedLogoutActivityData = logoutActivityData.GetType()
            .GetProperties()
            .ToDictionary(
                prop => CapitalizeFirstLetter(prop.Name),
                prop => prop.GetValue(logoutActivityData, null)
            );

        var requestUrl = $"{FirebaseDatabaseUrl}/activity/{userId}.json";

        var response = await _httpClient.PostAsJsonAsync(requestUrl, capitalizedLogoutActivityData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to log admin logout activity: {error}");
        }
    }

    // Delete user account from Firebase Authentication and Firebase Database
    public async Task<bool> DeleteUserAccountAsync(string idToken)
    {
        var request = new { idToken };
        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlDeleteUser}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to delete user: {error}");
            return false;
        }
    }

    // Delete user data from Firebase Realtime Database
    public async Task<bool> DeleteUserDataFromDatabaseAsync(string userId)
    {
        try
        {
            // Construct the URLs to delete the specific user data
            var usersRequestUrl = $"{FirebaseDatabaseUrl}/users/users/{userId}.json";
            var activityRequestUrl = $"{FirebaseDatabaseUrl}/activity/{userId}.json";

            // Send DELETE requests to remove the user's data from the 'users' and 'activity' nodes
            var deleteUserResponse = await _httpClient.DeleteAsync(usersRequestUrl);
            var deleteActivityResponse = await _httpClient.DeleteAsync(activityRequestUrl);

            // Check if both delete requests were successful
            if (deleteUserResponse.IsSuccessStatusCode && deleteActivityResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Optionally, you can log specific errors for each request
                var userError = await deleteUserResponse.Content.ReadAsStringAsync();
                var activityError = await deleteActivityResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to delete user data: {userError}");
                Console.WriteLine($"Failed to delete activity data: {activityError}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user data from database: {ex.Message}");
            return false;
        }
    }

}

// Response classes
public class FirebaseSignUpResponse
{
    public string IdToken { get; set; }
    public string LocalId { get; set; }
}

// Response and data models
public class FirebaseSignInResponse
{
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public string LocalId { get; set; }
}

public class UserData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Sex { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
