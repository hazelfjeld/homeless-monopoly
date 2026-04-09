using UnityEngine;
using TMPro;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
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

    public async void HostLobby()
    {
        mainMenuPanel.SetActive(false);
        hostLobbyPanel.SetActive(true);

        lobbyCodeText.text = "Code: Creating...";
        playerListText.text = "Players:\nLoading...";

        await CreateHostLobby();
    }

    private async Task CreateHostLobby()
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 4
            }
            .WithRelayNetwork()
            .WithPlayerName(VisibilityPropertyOptions.Member);

            activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            SubscribeToSessionEvents();
            RefreshPlayerList();

            lobbyCodeText.text = "Code: " + activeSession.Code;
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
            lobbyCodeText.text = "Code: Failed";
            playerListText.text = "Players:\nError";
        }
    }

    public void OpenJoinPanel()
    {
        mainMenuPanel.SetActive(false);
        joinLobbyPanel.SetActive(true);
    }

    public async void JoinLobby()
    {
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

            SubscribeToSessionEvents();
            RefreshPlayerList();

            lobbyCodeText.text = "Code: " + activeSession.Code;
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
        }
    }

    private void SubscribeToSessionEvents()
    {
        if (activeSession == null)
            return;

        activeSession.PlayerJoined += OnPlayerJoined;
        activeSession.PlayerLeaving += OnPlayerLeaving;
        activeSession.PlayerHasLeft += OnPlayerHasLeft;
        activeSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
    }

    private void OnPlayerJoined(string playerId)
    {
        RefreshPlayerList();
    }

    private void OnPlayerLeaving(string playerId)
    {
        RefreshPlayerList();
    }

    private void OnPlayerHasLeft(string playerId)
    {
        RefreshPlayerList();
    }

    private void OnPlayerPropertiesChanged()
    {
        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        if (activeSession == null || playerListText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Players:");

        foreach (var player in activeSession.Players)
        {
            string playerName = player.GetPlayerName();

            if (string.IsNullOrEmpty(playerName))
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                    playerName = "You";
                else
                    playerName = "Player " + player.Id.Substring(0, 6);
            }

            sb.AppendLine("- " + playerName);
        }

        playerListText.text = sb.ToString();
    }
}