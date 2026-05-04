using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public static class UnityServicesInit
{
    private static bool initialized;

    public static async Task InitializeServices()
    {
        if (initialized)
        {
            return;
        }

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);

#if !UNITY_WEBGL
            await AuthenticationService.Instance.UpdatePlayerNameAsync("Player" + Random.Range(1000, 9999));
#endif
        }

        initialized = true;
    }
}