using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            UnityEngine.Debug.LogWarning("Already authenticating, waiting for completion...");
            await Authenticating();
            return AuthState;
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxTries)
    {
        AuthState = AuthState.Authenticating;
        int retries = 0;
        while (AuthState == AuthState.Authenticating && retries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException ex)
            {
                UnityEngine.Debug.LogError($"Authentication failed: {ex.Message}");
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException requestFailedEx)
            {
                UnityEngine.Debug.LogError($"Request failed: {requestFailedEx.Message}");
                AuthState = AuthState.Error;
            }

            retries++;
            await Task.Delay(1000); // prevent reach rate limit
        }

        if (AuthState != AuthState.Authenticated)
        {
            AuthState = AuthState.TimeOut;
            UnityEngine.Debug.LogWarning($"Player was not signed in successfully. after {retries} retries");
        }
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
