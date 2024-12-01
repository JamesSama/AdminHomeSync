using AdminHomeSync.Components.Model;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public class AuthenticatedPage : ComponentBase
{
    [Inject] protected ILocalStorageService localStorage { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }

    protected UserData userProfile;

    // Ensure asynchronous initialization before rendering the page
    protected override async Task OnInitializedAsync()
    {
        await CheckUserSessionAsync();

        // If the session check fails, we want to stop the initialization and redirect.
        if (userProfile == null)
        {
            return;
        }
    }

    // Call CheckUserSession asynchronously
    private async Task CheckUserSessionAsync()
    {
        try
        {
            // Retrieve user profile from local storage
            userProfile = await localStorage.GetItemAsync<UserData>("userProfile");

            // If userProfile is null, redirect to login page
            if (userProfile == null)
            {
                NavigationManager.NavigateTo("/", true); // true forces an immediate navigation
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user profile: {ex.Message}");
        }
    }
}
