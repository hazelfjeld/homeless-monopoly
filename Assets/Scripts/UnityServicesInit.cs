using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class UnityServicesInit : MonoBehaviour
{
    async void Awake()
    {
        await InitializeServices();
    }

    async Task InitializeServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }
}