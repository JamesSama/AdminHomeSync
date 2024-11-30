public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseApiKey = "AIzaSyBcyyz9rrgkx7Uby4YN6qWXQlImlEWxHjw";
    private const string FirebaseAuthUrlSignUp = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
    private const string FirebaseAuthUrlSignIn = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
    private const string FirebaseDatabaseUrl = "https://homesync-3be92-default-rtdb.firebaseio.com/";

    private string _idToken;
    private string _userId;
    private UserData _userProfile;

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

    // Sign-up method
    public async Task<FirebaseSignUpResponse> SignUpAsync(string email, string password)
    {
        var request = new { email, password, returnSecureToken = true };
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

    // Sign-in method
    public async Task<FirebaseSignInResponse> SignInWithEmailAndPasswordAsync(string email, string password)
    {
        var request = new { email, password, returnSecureToken = true };
        var response = await _httpClient.PostAsJsonAsync($"{FirebaseAuthUrlSignIn}?key={FirebaseApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
            _idToken = result.IdToken;
            _userId = result.LocalId;

            // Fetch user data after sign-in
            _userProfile = await GetUserDataFromDatabaseAsync(_userId, _idToken);
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

    // Get current user
    public UserData GetCurrentUser()
    {
        return _userProfile;
    }

    // Logout method
    public void Logout()
    {
        _idToken = null;
        _userId = null;
        _userProfile = null;
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
        public string Email { get; set; }
        public string Birthdate { get; set; }
        public string Sex { get; set; }
        public string IdToken { get; set; }
    }
}
