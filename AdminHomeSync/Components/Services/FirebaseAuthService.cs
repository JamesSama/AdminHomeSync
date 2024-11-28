public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseApiKey = "AIzaSyBcyyz9rrgkx7Uby4YN6qWXQlImlEWxHjw";
    private const string FirebaseAuthUrlSignUp = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
    private const string FirebaseAuthUrlSignIn = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
    private const string FirebaseDatabaseUrl = "https://homesync-3be92-default-rtdb.firebaseio.com/";

    public FirebaseAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task AddUserToDatabaseAsync(string userId, object userData, string idToken)
    {
        var requestUrl = $"{FirebaseDatabaseUrl}/users/users/{userId}.json?auth={idToken}";
        var response = await _httpClient.PutAsJsonAsync(requestUrl, userData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to add user to database: {error}");
        }
    }

    // SignUp method
    public async Task<FirebaseSignUpResponse> SignUpAsync(string email, string password)
    {
        var request = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlSignUp}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseSignUpResponse>();
            return result;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sign-up failed: {error}");
        }
    }

    // SignIn method
    public async Task<FirebaseSignInResponse> SignInWithEmailAndPasswordAsync(string email, string password)
    {
        var request = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlSignIn}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();

            // Retrieve FirstName and LastName from Firebase
            var userData = await GetUserDataFromDatabaseAsync(result.LocalId, result.IdToken);

            // Log activity when admin logs in
            await LogAdminActivityAsync(result.LocalId, email, "Admin logged in", result.IdToken, userData.FirstName, userData.LastName);

            return result;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sign-in failed: {error}");
        }
    }

    // Get user data from Firebase Database
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

    // Method to log admin activity
    public async Task LogAdminActivityAsync(string userId, string email, string action, string idToken, string firstName, string lastName)
    {
        var currentDate = DateTime.Now;
        var formattedDate = currentDate.ToString("MMMM dd, yy");
        var formattedTime = currentDate.ToString("hh:mm tt");

        // Original activity data with FirstName and LastName retrieved from Firebase
        var activityData = new
        {
            Action = action,
            Date = formattedDate,
            Email = email,
            FirstName = firstName,  // Retrieved from the database
            LastName = lastName,    // Retrieved from the database
            Role = "Admin",
            Time = formattedTime,
            UserId = userId
        };

        // Convert the activityData to a dictionary and capitalize the keys
        var capitalizedActivityData = activityData.GetType()
            .GetProperties()
            .ToDictionary(
                prop => prop.Name,  // Keep the original key
                prop => prop.GetValue(activityData, null)?.ToString() // Get the value of each property
            )
            .ToDictionary(
                kvp => CapitalizeFirstLetter(kvp.Key), // Capitalize the key
                kvp => kvp.Value
            );

        var requestUrl = $"{FirebaseDatabaseUrl}/activity/{userId}.json?auth={idToken}";
        var response = await _httpClient.PostAsJsonAsync(requestUrl, capitalizedActivityData);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to log activity: {error}");
        }
    }

    // Helper function to capitalize the first letter of a string
    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    // Response classes
    public class FirebaseSignUpResponse
    {
        public string IdToken { get; set; }
        public string LocalId { get; set; }
    }

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
    }
}
