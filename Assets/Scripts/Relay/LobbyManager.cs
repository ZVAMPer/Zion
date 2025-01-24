using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private async void OnHostClicked()
    {
        statusText.text = "Hosting...";
        string code = await RelayManager.Instance.StartHost();
        if (!string.IsNullOrEmpty(code))
        {
            statusText.text = $"Hosting... Join Code: {code}";
            // Optionally, display the code to the host
        }
        else
        {
            statusText.text = "Failed to Host.";
        }
    }

    private async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Enter a valid join code.";
            return;
        }

        statusText.text = "Joining...";
        await RelayManager.Instance.JoinGame(code);
        statusText.text = "Joined Successfully!";
    }
}