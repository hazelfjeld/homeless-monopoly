using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject hostLobbyPanel;
    public GameObject joinLobbyPanel;

    public TMP_Text lobbyCodeText;
    public TMP_Text playerListText;
    public TMP_InputField joinCodeInput;

    private ISession activeSession;
    private bool isHost;
    private const string GameplaySceneName = "MainScene";

    private string GetLocalDisplayName()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            return "Player";
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        if (string.IsNullOrWhiteSpace(playerId) || playerId.Length <= 6)
        {
            return "Player";
        }

        return "Player-" + playerId.Substring(0, 6);
    }

    public async void HostLobby()
    {
        isHost = true;
        mainMenuPanel.SetActive(false);
        hostLobbyPanel.SetActive(true);

        lobbyCodeText.text = "Code: Creating...";
        playerListText.text = "Players:\n- Host";

        await CreateHostLobby();
    }

    private async Task CreateHostLobby()
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 4
            }.WithRelayNetwork();

            activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            lobbyCodeText.text = "Code: " + activeSession.Code;
            string hostName = GetLocalDisplayName();
            playerListText.text = "Players:\n- " + hostName;
            LobbyGameBootstrap.SetLobbyPlayers(new[] { hostName });
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
            lobbyCodeText.text = "Code: Failed";
        }
    }

    public void OpenJoinPanel()
    {
        mainMenuPanel.SetActive(false);
        joinLobbyPanel.SetActive(true);
    }

    public async void JoinLobby()
    {
        isHost = false;
        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            Debug.Log("No code entered.");
            return;
        }

        await JoinLobbyByCode(code);
    }

    private async Task JoinLobbyByCode(string code)
    {
        try
        {
            activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            joinLobbyPanel.SetActive(false);
            hostLobbyPanel.SetActive(true);

            lobbyCodeText.text = "Code: " + activeSession.Code;
            string joinedName = GetLocalDisplayName();
            playerListText.text = "Players:\n- " + joinedName;
            LobbyGameBootstrap.SetLobbyPlayers(new[] { joinedName });
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
        }
    }

    public void StartGameAsHost()
    {
        if (!isHost)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        if (activeSession == null)
        {
            Debug.LogWarning("Cannot start game: no active session.");
            return;
        }

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
            return;
        }

        Debug.LogWarning("NetworkManager server is not active; loading scene locally as fallback.");
        SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
    }
}
