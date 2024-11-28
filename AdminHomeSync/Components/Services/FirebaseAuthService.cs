public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseApiKey = "AIzaSyBcyyz9rrgkx7Uby4YN6qWXQlImlEWxHjw";
    private const string FirebaseAuthUrlSignUp = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
    private const string FirebaseAuthUrlSignIn = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";

    public FirebaseAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
            return result;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Sign-in failed: {error}");
        }
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
}