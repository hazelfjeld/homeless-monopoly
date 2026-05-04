using UnityEngine;
using TMPro;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject hostLobbyPanel;
    public GameObject joinLobbyPanel;
    public GameObject startGameButton;

    public TMP_Text lobbyCodeText;
    public TMP_Text playerListText;
    public TMP_Text joinStatusText;
    public TMP_InputField joinCodeInput;

    private ISession activeSession;
    private bool isHostSession;
    private Button resolvedStartGameButton;
    private TMP_Text resolvedStartGameButtonText;
    private TMP_Text resolvedJoinStatusText;

    private void Awake()
    {
        ResolveOrCreateStartGameButton();
        ResolveOrCreateJoinStatusText();
        UpdateStartGameButtonVisibility();
    }

    public async void HostLobby()
    {
        mainMenuPanel.SetActive(false);
        hostLobbyPanel.SetActive(true);
        joinLobbyPanel.SetActive(false);

        lobbyCodeText.text = "Code: Creating...";
        playerListText.text = "Players:\nLoading...";
        isHostSession = true;
        UpdateStartGameButtonVisibility();

        await CreateHostLobby();
    }

    private async Task CreateHostLobby()
    {
        try
        {
            await UnityServicesInit.InitializeServices();
            ConfigureTransportForRelayWebSockets();

            var options = new SessionOptions
            {
                MaxPlayers = 4
            }
            .WithRelayNetwork()
            .WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol.WSS })
            .WithPlayerName(VisibilityPropertyOptions.Member);

            activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            MultiplayerSessionContext.SetSession(activeSession, true);

            SubscribeToSessionEvents();
            RefreshPlayerList();
            UpdateStartGameButtonVisibility();

            lobbyCodeText.text = "Code: " + activeSession.Code;
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
            lobbyCodeText.text = "Code: Failed";
            playerListText.text = "Players:\nError creating lobby.\n" + e.Message;
            UpdateStartGameButtonVisibility();
        }
    }

    public void OpenJoinPanel()
    {
        mainMenuPanel.SetActive(false);
        joinLobbyPanel.SetActive(true);
        hostLobbyPanel.SetActive(false);
        isHostSession = false;
        SetJoinStatus("Enter the host's lobby code.");
        UpdateStartGameButtonVisibility();
    }

    public async void JoinLobby()
    {
        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            Debug.Log("No code entered.");
            SetJoinStatus("Enter a lobby code first.");
            return;
        }

        SetJoinStatus("Joining lobby...");
        await JoinLobbyByCode(code);
    }

    private async Task JoinLobbyByCode(string code)
    {
        try
        {
            await UnityServicesInit.InitializeServices();
            ConfigureTransportForRelayWebSockets();

            var options = new JoinSessionOptions()
                .WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol.WSS })
                .WithPlayerName(VisibilityPropertyOptions.Member);

            activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options);
            isHostSession = false;
            MultiplayerSessionContext.SetSession(activeSession, false);

            joinLobbyPanel.SetActive(false);
            hostLobbyPanel.SetActive(true);
            SetJoinStatus("");

            SubscribeToSessionEvents();
            RefreshPlayerList();
            UpdateStartGameButtonVisibility();

            lobbyCodeText.text = "Code: " + activeSession.Code;
        }
        catch (SessionException e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
            SetJoinStatus("Could not join lobby. Check the code and try again.\n" + e.Message);
        }
    }

    public void StartHostedGame()
    {
        if (!isHostSession)
        {
            SetLobbyStatus("Only the host can start the game.");
            return;
        }

        if (activeSession == null)
        {
            SetLobbyStatus("Create a lobby before starting the game.");
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsHost)
        {
            SetLobbyStatus("Network is still connecting. Wait a moment, then press Start Game again.");
            return;
        }

        MultiplayerSessionContext.MarkGameStarting();

        if (networkManager.SceneManager != null)
        {
            networkManager.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("MainScene");
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

        sb.AppendLine();

        if (isHostSession)
        {
            sb.AppendLine("Press Start Game when everyone has joined.");
        }
        else
        {
            sb.AppendLine("Waiting for the host to start the game.");
        }

        playerListText.text = sb.ToString();
    }

    private void ConfigureTransportForRelayWebSockets()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            return;
        }

        DontDestroyOnLoad(networkManager.gameObject);

        UnityTransport unityTransport = networkManager.GetComponent<UnityTransport>();

        if (unityTransport != null)
        {
            unityTransport.UseWebSockets = true;
        }
    }

    private void ResolveOrCreateStartGameButton()
    {
        if (startGameButton != null)
        {
            resolvedStartGameButton = startGameButton.GetComponent<Button>();
            resolvedStartGameButtonText = startGameButton.GetComponentInChildren<TMP_Text>();
        }

        if (resolvedStartGameButton == null && hostLobbyPanel != null)
        {
            GameObject buttonObject = new GameObject("StartGameButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(hostLobbyPanel.transform, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 32f);
            rectTransform.sizeDelta = new Vector2(220f, 48f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.17f, 0.45f, 0.92f, 1f);

            resolvedStartGameButton = buttonObject.GetComponent<Button>();

            GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelTransform = labelObject.GetComponent<RectTransform>();
            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = Vector2.zero;
            labelTransform.offsetMax = Vector2.zero;

            resolvedStartGameButtonText = labelObject.GetComponent<TextMeshProUGUI>();
            resolvedStartGameButtonText.text = "Start Game";
            resolvedStartGameButtonText.color = Color.white;
            resolvedStartGameButtonText.fontSize = 24f;
            resolvedStartGameButtonText.alignment = TextAlignmentOptions.Center;

            startGameButton = buttonObject;
        }

        if (resolvedStartGameButton != null)
        {
            resolvedStartGameButton.onClick.RemoveListener(StartHostedGame);
            resolvedStartGameButton.onClick.AddListener(StartHostedGame);
        }

        if (resolvedStartGameButtonText != null)
        {
            resolvedStartGameButtonText.text = "Start Game";
        }
    }

    private void ResolveOrCreateJoinStatusText()
    {
        resolvedJoinStatusText = joinStatusText;

        if (resolvedJoinStatusText != null || joinLobbyPanel == null)
        {
            return;
        }

        GameObject labelObject = new GameObject("JoinStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(joinLobbyPanel.transform, false);

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 24f);
        rectTransform.sizeDelta = new Vector2(520f, 72f);

        resolvedJoinStatusText = labelObject.GetComponent<TextMeshProUGUI>();
        resolvedJoinStatusText.text = "";
        resolvedJoinStatusText.color = Color.white;
        resolvedJoinStatusText.fontSize = 20f;
        resolvedJoinStatusText.alignment = TextAlignmentOptions.Center;
        joinStatusText = resolvedJoinStatusText;
    }

    private void UpdateStartGameButtonVisibility()
    {
        if (startGameButton != null)
        {
            startGameButton.SetActive(isHostSession && activeSession != null);
        }
    }

    private void SetLobbyStatus(string message)
    {
        Debug.Log(message);

        if (playerListText != null)
        {
            playerListText.text = message;
        }
    }

    private void SetJoinStatus(string message)
    {
        Debug.Log(message);

        if (resolvedJoinStatusText != null)
        {
            resolvedJoinStatusText.text = message;
        }
    }
}

public static class MultiplayerSessionContext
{
    public static ISession ActiveSession { get; private set; }
    public static bool HasSession { get; private set; }
    public static bool LocalPlayerIsHost { get; private set; }
    public static bool GameIsStarting { get; private set; }
    public static string SessionCode { get; private set; } = "";

    public static void SetSession(ISession session, bool localPlayerIsHost)
    {
        ActiveSession = session;
        HasSession = session != null;
        LocalPlayerIsHost = localPlayerIsHost;
        GameIsStarting = false;
        SessionCode = session != null ? session.Code : "";
    }

    public static void MarkGameStarting()
    {
        GameIsStarting = true;
    }

    public static void Clear()
    {
        ActiveSession = null;
        HasSession = false;
        LocalPlayerIsHost = false;
        GameIsStarting = false;
        SessionCode = "";
    }
}
