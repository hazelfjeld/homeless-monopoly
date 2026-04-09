using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;

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
            playerListText.text = "Players:\n- Host";
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
            playerListText.text = "Players:\n- Joined Player";
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
        }
    }
}