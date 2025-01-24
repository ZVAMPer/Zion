using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text ownJoinCodeText;       // Displays the player's own join code
    public TMP_Text hostStatusText;        // Displays hosting status
    public Button leaveButton;             // Button to leave the current game

    public TMP_InputField joinCodeInput;   // Input field for entering a join code
    public Button joinButton;              // Button to join a game

    public TMP_Text statusText;            // General status messages

    [Header("Overlay Settings")]
    public Canvas lobbyOverlay;         // The lobby overlay panel to toggle

    [Header("Other")]
    public bool autoHostRetry = true;      // Automatically retry hosting on start
    private bool isOverlayActive = false;   // Tracks the current state of the overlay

    private async void Start()
    {
        // Assign button listeners
        joinButton.onClick.AddListener(OnJoinClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);
        leaveButton.gameObject.SetActive(false);

        // Initialize the lobby overlay to be active at start
        if (lobbyOverlay != null)
        {
            lobbyOverlay.enabled = true;
            isOverlayActive = true;
            EnableCursor();
        }
        else
        {
            Debug.LogWarning("Lobby Overlay is not assigned in the Inspector.");
        }

        // Automatically start hosting on start
        await Task.Delay(1000); // Delay to ensure RelayManager is initialized
        await Host();
    }

    /// <summary>
    /// Hosts a game when the player starts the application.
    /// </summary>
    private async Task Host()
    {
        statusText.text = "Hosting your game...";
        hostStatusText.text = "Hosting...";
        string code = await RelayManager.Instance.StartHost();
        if (!string.IsNullOrEmpty(code))
        {
            ownJoinCodeText.text = $"{code}";
            statusText.text = "Hosting started successfully.";
        }
        else
        {
            ownJoinCodeText.text = "";
            statusText.text = "Failed to host your game.";
            if (autoHostRetry)
            {
                await Task.Delay(2000);
                statusText.text += " Retrying...";
                await Host();
            }
        }
    }

    /// <summary>
    /// Handles the Join button click event.
    /// </summary>
    private async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Please enter a valid join code.";
            return;
        }

        statusText.text = "Attempting to join...";
        bool success = await RelayManager.Instance.JoinGame(code);
        if (success)
        {
            statusText.text = "Successfully joined the game.";
            // Optionally, update UI elements based on the new state
            ownJoinCodeText.text = "";
            joinButton.gameObject.SetActive(false);
            joinCodeInput.gameObject.SetActive(false);
            leaveButton.gameObject.SetActive(true);
        }
        else
        {
            statusText.text = "Failed to join the game. Please check the join code.";
        }
    }

    /// <summary>
    /// Handles the Leave button click event.
    /// </summary>
    private async void OnLeaveClicked()
    {
        statusText.text = "Leaving the game...";
        RelayManager.Instance.LeaveGame();

        // Clear join code display and update status
        ownJoinCodeText.text = "";
        hostStatusText.text = "Not hosting.";
        statusText.text = "You have left the game.";
        joinButton.gameObject.SetActive(true);
        joinCodeInput.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(false);

        // Optionally, you can decide to automatically re-host or provide options to the user
        // For this demo, we'll offer the option to re-host
        // You can uncomment the following line to automatically re-host after leaving
        await Host();
    }

    /// <summary>
    /// Updates every frame to check for input.
    /// </summary>
    private void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tab key pressed.");
            ToggleLobbyOverlay();
        }
    }

    /// <summary>
    /// Toggles the visibility of the lobby overlay and manages mouse control.
    /// </summary>
    private void ToggleLobbyOverlay()
    {
        if (lobbyOverlay == null)
        {
            Debug.LogWarning("Lobby Overlay is not assigned in the Inspector.");
            return;
        }

        isOverlayActive = !isOverlayActive;
        lobbyOverlay.enabled = isOverlayActive;

        if (isOverlayActive)
        {
            // Focus the input field
            joinCodeInput.ActivateInputField();
            EnableCursor();
        }
        else
        {
            // Defocus the input field
            joinCodeInput.DeactivateInputField();
            DisableCursor();
        }
    }

    /// <summary>
    /// Enables the mouse cursor and unlocks it.
    /// </summary>
    private void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Hides the mouse cursor and locks it to the center of the screen.
    /// </summary>
    private void DisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}