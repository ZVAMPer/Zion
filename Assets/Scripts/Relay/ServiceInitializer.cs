using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class ServicesInitializer : MonoBehaviour
{
    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            // Check if the user is already signed in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // Sign in anonymously
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in Anonymously");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
        }
    }
}