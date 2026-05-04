using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Text;

public class EscapeHomelessnessGameManager : MonoBehaviour
{

    [Header("Win Screen UI")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TMP_Text winScreenTitleText;
    [SerializeField] private TMP_Text winScreenBodyText;
    [SerializeField] private string donationUrl = "https://endhomelessness.org/other-ways-to-give/";

    [Header("Player Status UI")]
    [SerializeField] private GameObject playerStatusPanel;
    [SerializeField] private TMP_Text playerStatusTitleText;
    [SerializeField] private TMP_Text playerStatusBodyText;
    [Header("Character Select UI")]
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private TMP_Text characterSelectNameText;
    [SerializeField] private TMP_Text characterSelectDescriptionText;
    [SerializeField] private TMP_Text characterSelectHintText;
    [SerializeField] private bool showCharacterSelectOnStart = true;

    private int selectedCharacterIndex = -1;
    [Header("Board")]
    [SerializeField] private List<BoardSpaceData> boardSpaces = new List<BoardSpaceData>();
    [SerializeField] private bool startPlayersOnFirstGoSpace = true;

    [Header("Character Templates")]
    [SerializeField] private List<Player> characterTemplates = new List<Player>();

    [Header("Decks")]
    [SerializeField] private CardDeck stopDeck = new CardDeck();
    [SerializeField] private CardDeck goDeck = new CardDeck();
    [SerializeField] private CardDeck communityDeck = new CardDeck();
    [SerializeField] private CardDeck questionDeck = new CardDeck();

    [Header("Token Visual")]
    [SerializeField] private Transform playerTokenTransform;
    [SerializeField] private Vector3 tokenOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Popup UI")]
    [SerializeField] private GameObject cardPopupPanel;
    [SerializeField] private TMP_Text cardPopupTitleText;
    [SerializeField] private TMP_Text cardPopupBodyText;
    [SerializeField] private TMP_Text statusText;

    [Header("Timing")]
    [SerializeField] private float jumpDelaySeconds = 0.5f;

    [Header("Startup")]
    [SerializeField] private bool autoStartGame = true;

    [Header("Runtime State")]
    [SerializeField] private List<Player> activePlayers = new List<Player>();
    [SerializeField] private int currentPlayerIndex;
    [SerializeField] private bool gameIsOver;
    [SerializeField] private string lastTurnSummary = "";

    [Header("Card Button Flow")]
    [SerializeField] private bool waitingForCardButtonPress;
    [SerializeField] private BoardSpaceType requiredCardButtonType = BoardSpaceType.Start;
    [SerializeField] private bool waitingForPopupClose;
    [SerializeField] private bool turnBusy;

    [Header("Multiplayer")]
    [SerializeField] private bool enableMultiplayerWhenNetworkConnected = true;
    [SerializeField] private float multiplayerStartDelaySeconds = 0.25f;

    private const string SnapshotMessageName = "EscapeHomelessness.Snapshot";
    private const string ActionMessageName = "EscapeHomelessness.Action";

    private readonly List<ulong> activePlayerOwnerClientIds = new List<ulong>();
    private readonly List<Transform> runtimeTokenTransforms = new List<Transform>();
    private bool multiplayerMode;
    private bool multiplayerHost;
    private bool networkMessagesRegistered;
    private bool applyingNetworkSnapshot;
    private string currentPopupTitle = "";
    private string currentPopupBody = "";

    public string LastTurnSummary => lastTurnSummary;
    public bool GameIsOver => gameIsOver;
    public bool WaitingForCardButtonPress => waitingForCardButtonPress;
    public BoardSpaceType RequiredCardButtonType => requiredCardButtonType;

    public Player CurrentPlayer
    {
        get
        {
            if (activePlayers.Count == 0)
            {
                return null;
            }

            if (currentPlayerIndex < 0 || currentPlayerIndex >= activePlayers.Count)
            {
                return null;
            }

            return activePlayers[currentPlayerIndex];
        }
    }

public void OpenPlayerStatusPopup()
{
    Player activePlayer = CurrentPlayer;

    if (activePlayer == null)
    {
        SetStatus("There is no active player right now.");
        return;
    }

    RefreshPlayerStatusPopup();

    if (playerStatusPanel != null)
    {
        playerStatusPanel.SetActive(true);
    }
}

public void ClosePlayerStatusPopup()
{
    HidePlayerStatusPopup();
}

private void HidePlayerStatusPopup()
{
    if (playerStatusPanel != null)
    {
        playerStatusPanel.SetActive(false);
    }
}

private void RefreshPlayerStatusPopup()
{
    Player activePlayer = CurrentPlayer;

    if (activePlayer == null)
    {
        if (playerStatusTitleText != null)
        {
            playerStatusTitleText.text = "No Active Player";
        }

        if (playerStatusBodyText != null)
        {
            playerStatusBodyText.text = "";
        }

        return;
    }

    if (playerStatusTitleText != null)
    {
        playerStatusTitleText.text = $"{activePlayer.CharacterName}, Age {activePlayer.Age}";
    }

    if (playerStatusBodyText != null)
    {
        playerStatusBodyText.text = BuildPlayerStatusText(activePlayer);
    }
}

private string BuildPlayerStatusText(Player player)
{
    StringBuilder statusBuilder = new StringBuilder();

    statusBuilder.AppendLine($"Current space: {GetCurrentSpaceName(player)}");
    statusBuilder.AppendLine($"Cash: ${player.CashAmount}");
    statusBuilder.AppendLine($"Wait turns: {player.TurnsToWait}");
    statusBuilder.AppendLine();

    statusBuilder.AppendLine("Resources:");
    bool hasAnyResources = false;

    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasPhone, "Phone", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasWorkingPhone, "Working phone", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasCar, "Car", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasDriversLicense, "Driver's license", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasId, "ID", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasBirthCertificate, "Birth certificate", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasSocialSecurityCard, "Social Security card", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasBackpack, "Backpack", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasJob, "Job", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasShelter, "Shelter", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.IsInShelter, "Currently in shelter", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasPlaceToSleep, "Place to sleep", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasSafePlaceDuringDay, "Safe place during the day", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasCleanClothes, "Clean clothes", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasHealthInsurance, "Health insurance", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasCaseworker, "Caseworker", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.HasGedOrDiploma, "GED / Diploma", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.IsLiterate, "Can read", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.IsHealthy, "Healthy", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.IsVeteran, "Veteran", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.IsStudent, "Student", statusBuilder);
    hasAnyResources |= AppendStatusLineIfTrueAndReturn(player.AttendsChurchFrequently, "Church attendance", statusBuilder);

    if (!hasAnyResources)
    {
        statusBuilder.AppendLine("- None listed");
    }

    statusBuilder.AppendLine();
    statusBuilder.AppendLine("Barriers:");
    bool hasAnyBarriers = false;

    hasAnyBarriers |= AppendStatusLineIfTrueAndReturn(player.HasAddiction, "Addiction", statusBuilder);
    hasAnyBarriers |= AppendStatusLineIfTrueAndReturn(player.HasCriminalRecord, "Criminal record", statusBuilder);
    hasAnyBarriers |= AppendStatusLineIfTrueAndReturn(player.HasBeenEvictedInThePast, "Previously evicted", statusBuilder);
    hasAnyBarriers |= AppendStatusLineIfTrueAndReturn(player.HasBeenToJail, "Been to jail", statusBuilder);
    hasAnyBarriers |= AppendStatusLineIfTrueAndReturn(player.NeedsMedication, "Needs medication", statusBuilder);

    if (!hasAnyBarriers)
    {
        statusBuilder.AppendLine("- None listed");
    }

    return statusBuilder.ToString();
}

private bool AppendStatusLineIfTrueAndReturn(bool condition, string label, StringBuilder statusBuilder)
{
    if (!condition)
    {
        return false;
    }

    statusBuilder.AppendLine($"- {label}");
    return true;
}
    private void AppendStatusLineIfTrue(StringBuilder statusBuilder, bool condition, string label)
    {
        if (condition)
        {
            statusBuilder.AppendLine($"- {label}");
        }
    }

    public void DonateButtonPressed()
{
    Application.OpenURL(donationUrl);
}

public void CloseWinScreen()
{
    HideWinScreen();
}

private void ShowWinScreen(Player winningPlayer)
{
    if (winScreenTitleText != null)
    {
        winScreenTitleText.text = "You Escaped Homelessness";
    }

    if (winScreenBodyText != null)
    {
        if (winningPlayer != null)
        {
            winScreenBodyText.text =
                $"{winningPlayer.CharacterName} won the game.\n\n" +
                "Thank you for playing.\n" +
                "If you'd like, you can donate to support homeless shelters.";
        }
        else
        {
            winScreenBodyText.text =
                "A player won the game.\n\n" +
                "If you'd like, you can donate to support homeless shelters.";
        }
    }

    if (winScreenPanel != null)
    {
        winScreenPanel.SetActive(true);
    }
}

private void HideWinScreen()
{
    if (winScreenPanel != null)
    {
        winScreenPanel.SetActive(false);
    }
}
    private void Awake()
    {
        LoadDeckData();
        HideCardPopup();
        HidePlayerStatusPopup();
        HideWinScreen();
    }

    private void Start()
    {
        if (TryStartNetworkedGame())
        {
            return;
        }

        if (showCharacterSelectOnStart)
        {
            ShowCharacterSelect();
            return;
        }

        if (autoStartGame)
        {
            StartGameWithAllTemplates();
        }
    }

    private bool TryStartNetworkedGame()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (!enableMultiplayerWhenNetworkConnected ||
            networkManager == null ||
            !networkManager.IsListening)
        {
            return false;
        }

        multiplayerMode = true;
        multiplayerHost = networkManager.IsHost || networkManager.IsServer;
        RegisterNetworkMessages();
        HideCharacterSelect();
        HideCardPopup();
        HideWinScreen();

        if (multiplayerHost)
        {
            StartCoroutine(StartHostMultiplayerGameAfterSceneReady());
        }
        else
        {
            SetStatus("Connected. Waiting for the host to start the game.");
            StartCoroutine(RequestSnapshotUntilReceived());
        }

        return true;
    }

    private IEnumerator RequestSnapshotUntilReceived()
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!multiplayerMode || multiplayerHost || activePlayers.Count > 0)
            {
                yield break;
            }

            SendMultiplayerAction(MultiplayerGameAction.RequestSnapshot());
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator StartHostMultiplayerGameAfterSceneReady()
    {
        if (multiplayerStartDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(multiplayerStartDelaySeconds);
        }
        else
        {
            yield return null;
        }

        List<ulong> ownerClientIds = BuildPlayerOwnerList();
        List<Player> selectedCharacterTemplates = BuildMultiplayerCharacterList(ownerClientIds.Count);

        if (selectedCharacterTemplates.Count == 0)
        {
            SetStatus("No character templates are available for multiplayer.");
            yield break;
        }

        activePlayerOwnerClientIds.Clear();
        int playerCount = Mathf.Min(ownerClientIds.Count, selectedCharacterTemplates.Count);

        for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
        {
            activePlayerOwnerClientIds.Add(ownerClientIds[playerIndex]);
        }

        StartGame(selectedCharacterTemplates);
        BroadcastMultiplayerSnapshot();
    }

    private void OnDestroy()
    {
        if (!networkMessagesRegistered || NetworkManager.Singleton == null)
        {
            return;
        }

        CustomMessagingManager messagingManager = NetworkManager.Singleton.CustomMessagingManager;

        if (messagingManager == null)
        {
            return;
        }

        messagingManager.UnregisterNamedMessageHandler(SnapshotMessageName);
        messagingManager.UnregisterNamedMessageHandler(ActionMessageName);
        NetworkManager.Singleton.OnClientConnectedCallback -= OnNetworkClientConnected;
        networkMessagesRegistered = false;
    }

    private List<ulong> BuildPlayerOwnerList()
    {
        List<ulong> ownerClientIds = new List<ulong>();
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager != null)
        {
            ownerClientIds.AddRange(networkManager.ConnectedClientsIds);
            ownerClientIds.Sort();
        }

        if (ownerClientIds.Count == 0 && networkManager != null)
        {
            ownerClientIds.Add(networkManager.LocalClientId);
        }

        return ownerClientIds;
    }

    private List<Player> BuildMultiplayerCharacterList(int requestedPlayerCount)
    {
        List<Player> selectedCharacterTemplates = new List<Player>();

        if (characterTemplates == null)
        {
            return selectedCharacterTemplates;
        }

        int playerCount = Mathf.Min(requestedPlayerCount, characterTemplates.Count);

        for (int templateIndex = 0; templateIndex < playerCount; templateIndex++)
        {
            Player templatePlayer = characterTemplates[templateIndex];

            if (templatePlayer != null)
            {
                selectedCharacterTemplates.Add(templatePlayer);
            }
        }

        return selectedCharacterTemplates;
    }

    private void LoadDeckData()
    {
        stopDeck.DeckName = "Stop Deck";
        goDeck.DeckName = "Go Deck";
        communityDeck.DeckName = "Community Deck";
        questionDeck.DeckName = "Question Deck";

        stopDeck.StartingCards = EscapeHomelessnessCardLibrary.CreateStopCards();
        goDeck.StartingCards = EscapeHomelessnessCardLibrary.CreateGoCards();
        communityDeck.StartingCards = EscapeHomelessnessCardLibrary.CreateCommunityCards();
        questionDeck.StartingCards = EscapeHomelessnessCardLibrary.CreateQuestionCards();
    }

    public void StartGameWithAllTemplates()
    {
        StartGame(characterTemplates);
    }

    public void StartGame(List<Player> selectedCharacterTemplates)
    {
        if (boardSpaces == null || boardSpaces.Count == 0)
        {
            SetStatus("No board spaces have been set up yet.");
            Debug.LogWarning(lastTurnSummary);
            return;
        }

        if (selectedCharacterTemplates == null || selectedCharacterTemplates.Count == 0)
        {
            SetStatus("No character templates were provided.");
            Debug.LogWarning(lastTurnSummary);
            return;
        }

        activePlayers.Clear();

        for (int templateIndex = 0; templateIndex < selectedCharacterTemplates.Count; templateIndex++)
        {
            Player templatePlayer = selectedCharacterTemplates[templateIndex];

            if (templatePlayer == null)
            {
                continue;
            }

            Player runtimePlayer = templatePlayer.Clone();
            runtimePlayer.ResetForNewRun(GetStartingBoardIndex());
            activePlayers.Add(runtimePlayer);
        }

        if (activePlayers.Count == 0)
        {
            SetStatus("No playable characters were created.");
            Debug.LogWarning(lastTurnSummary);
            return;
        }

        TrimMultiplayerOwnerListToActivePlayers();
        currentPlayerIndex = 0;
        gameIsOver = false;
        waitingForCardButtonPress = false;
        waitingForPopupClose = false;
        requiredCardButtonType = BoardSpaceType.Start;
        turnBusy = false;

        stopDeck.ResetDeck();
        goDeck.ResetDeck();
        communityDeck.ResetDeck();
        questionDeck.ResetDeck();
        HideWinScreen();
        HideCardPopup();
        EnsureTokenVisuals(activePlayers.Count);
        UpdateAllTokenPositions();
        RefreshPlayerStatusPopup();
        SetStatus("Game started.");
        BeginCurrentPlayerTurn();
    }

    private void TrimMultiplayerOwnerListToActivePlayers()
    {
        if (!multiplayerMode)
        {
            return;
        }

        while (activePlayerOwnerClientIds.Count > activePlayers.Count)
        {
            activePlayerOwnerClientIds.RemoveAt(activePlayerOwnerClientIds.Count - 1);
        }

        NetworkManager networkManager = NetworkManager.Singleton;
        ulong fallbackOwner = networkManager != null ? networkManager.LocalClientId : 0;

        while (activePlayerOwnerClientIds.Count < activePlayers.Count)
        {
            activePlayerOwnerClientIds.Add(fallbackOwner);
        }
    }


    public void SelectCharacter(int characterIndex)
    {
        if (characterTemplates == null || characterTemplates.Count == 0)
        {
            SetStatus("No character templates exist yet.");
            return;
        }

        if (characterIndex < 0 || characterIndex >= characterTemplates.Count)
        {
            SetStatus("That character index is out of range.");
            return;
        }

        selectedCharacterIndex = characterIndex;

        Player selectedCharacter = characterTemplates[characterIndex];

        if (characterSelectNameText != null)
        {
            characterSelectNameText.text = $"{selectedCharacter.CharacterName}, Age {selectedCharacter.Age}";
        }

        if (characterSelectDescriptionText != null)
        {
            characterSelectDescriptionText.text = BuildCharacterPreviewText(selectedCharacter);
        }

        if (characterSelectHintText != null)
        {
            characterSelectHintText.text = "Press Start Game to use this character.";
        }
    }

    public void ConfirmCharacterSelection()
    {
        if (selectedCharacterIndex < 0)
        {
            SetStatus("Pick a character first.");
            return;
        }

        if (characterTemplates == null || selectedCharacterIndex >= characterTemplates.Count)
        {
            SetStatus("That character is missing.");
            return;
        }

        HideCharacterSelect();

        Player selectedCharacter = characterTemplates[selectedCharacterIndex];
        StartGame(new List<Player> { selectedCharacter });
    }

    public void StartGameWithCharacterIndex(int characterIndex)
    {
        if (characterTemplates == null || characterTemplates.Count == 0)
        {
            SetStatus("No character templates exist yet.");
            return;
        }

        if (characterIndex < 0 || characterIndex >= characterTemplates.Count)
        {
            SetStatus("That character index is out of range.");
            return;
        }

        HideCharacterSelect();
        StartGame(new List<Player> { characterTemplates[characterIndex] });
    }

    private void ShowCharacterSelect()
    {
        HideCardPopup();

        gameIsOver = false;
        waitingForCardButtonPress = false;
        waitingForPopupClose = false;
        turnBusy = false;

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(true);
        }

        if (characterTemplates != null && characterTemplates.Count > 0)
        {
            SelectCharacter(0);
        }
        else
        {
            if (characterSelectNameText != null)
            {
                characterSelectNameText.text = "No Characters Found";
            }

            if (characterSelectDescriptionText != null)
            {
                characterSelectDescriptionText.text = "Add preset characters to the Character Templates list in the GameManager inspector.";
            }

            if (characterSelectHintText != null)
            {
                characterSelectHintText.text = "";
            }
        }
    }

    private void HideCharacterSelect()
    {
        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }
    }

    private string BuildCharacterPreviewText(Player player)
    {
        if (player == null)
        {
            return "";
        }

        string previewText = "";

        if (!string.IsNullOrWhiteSpace(player.Backstory))
        {
            previewText += player.Backstory;
        }

        if (player.CashAmount > 0)
        {
            if (previewText.Length > 0)
            {
                previewText += "\n\n";
            }

            previewText += $"Starting cash: ${player.CashAmount}";
        }

        List<string> startingTraits = new List<string>();

        if (player.HasPhone) startingTraits.Add("Phone");
        if (player.HasWorkingPhone) startingTraits.Add("Working phone");
        if (player.HasCar) startingTraits.Add("Car");
        if (player.HasDriversLicense) startingTraits.Add("Driver's license");
        if (player.HasId) startingTraits.Add("ID");
        if (player.HasBirthCertificate) startingTraits.Add("Birth certificate");
        if (player.HasSocialSecurityCard) startingTraits.Add("Social Security card");
        if (player.HasShelter) startingTraits.Add("Shelter");
        if (player.IsInShelter) startingTraits.Add("In shelter");
        if (player.HasPlaceToSleep) startingTraits.Add("Place to sleep");
        if (player.HasCleanClothes) startingTraits.Add("Clean clothes");
        if (player.HasGedOrDiploma) startingTraits.Add("GED/Diploma");
        if (player.IsLiterate) startingTraits.Add("Can read");
        if (player.HasCaseworker) startingTraits.Add("Caseworker");
        if (player.HasJob) startingTraits.Add("Job");
        if (player.IsHealthy) startingTraits.Add("Healthy");
        if (player.HasAddiction) startingTraits.Add("Addiction");
        if (player.HasBeenEvictedInThePast) startingTraits.Add("Previously evicted");
        if (player.HasBeenToJail) startingTraits.Add("Been to jail");
        if (player.IsVeteran) startingTraits.Add("Veteran");
        if (player.IsStudent) startingTraits.Add("Student");
        if (player.AttendsChurchFrequently) startingTraits.Add("Frequent church attendance");

        if (startingTraits.Count > 0)
        {
            if (previewText.Length > 0)
            {
                previewText += "\n\n";
            }

            previewText += "Starting traits: " + string.Join(", ", startingTraits);
        }

        return previewText;
    }
    public void PlayCurrentPlayerTurn()
    {
        BeginCurrentPlayerTurn();
    }

    public void PressStopButton()
    {
        SubmitCardButtonAction(BoardSpaceType.Stop);
    }

    public void PressGoButton()
    {
        SubmitCardButtonAction(BoardSpaceType.Go);
    }

    public void PressCommunityButton()
    {
        SubmitCardButtonAction(BoardSpaceType.Community);
    }

    public void PressQuestionButton()
    {
        SubmitCardButtonAction(BoardSpaceType.Question);
    }

    private void SubmitCardButtonAction(BoardSpaceType boardSpaceType)
    {
        if (!CanLocalPlayerActOnCurrentTurn())
        {
            SetStatus(BuildNotYourTurnMessage());
            return;
        }

        if (multiplayerMode && !multiplayerHost)
        {
            SendMultiplayerAction(MultiplayerGameAction.CardButton(boardSpaceType));
            return;
        }

        TryResolveCardButton(boardSpaceType);
        BroadcastMultiplayerSnapshot();
    }

    public void CloseCardPopup()
    {
        if (!CanLocalPlayerActOnCurrentTurn())
        {
            SetStatus(BuildNotYourTurnMessage());
            return;
        }

        if (multiplayerMode && !multiplayerHost)
        {
            SendMultiplayerAction(MultiplayerGameAction.ClosePopup());
            return;
        }

        ResolveCloseCardPopup();
        BroadcastMultiplayerSnapshot();
    }

    private void ResolveCloseCardPopup()
    {
        if (!waitingForPopupClose)
        {
            HideCardPopup();
            return;
        }

        waitingForPopupClose = false;
        HideCardPopup();

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null || gameIsOver)
        {
            return;
        }

        StartCoroutine(ResolveAutomaticMovementAndEndTurn(activePlayer));
    }

    private bool CanLocalPlayerActOnCurrentTurn()
    {
        if (!multiplayerMode)
        {
            return true;
        }

        if (CurrentPlayer == null)
        {
            return false;
        }

        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            return true;
        }

        return GetCurrentTurnOwnerClientId() == networkManager.LocalClientId;
    }

    private string BuildNotYourTurnMessage()
    {
        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            return "Waiting for the game to start.";
        }

        return $"It is {activePlayer.CharacterName}'s turn.";
    }

    private ulong GetCurrentTurnOwnerClientId()
    {
        if (currentPlayerIndex < 0 || currentPlayerIndex >= activePlayerOwnerClientIds.Count)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            return networkManager != null ? networkManager.LocalClientId : 0;
        }

        return activePlayerOwnerClientIds[currentPlayerIndex];
    }

    private void BeginCurrentPlayerTurn()
    {
        if (gameIsOver || turnBusy || waitingForPopupClose)
        {
            return;
        }

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            SetStatus("No active player found.");
            return;
        }

        UpdateSingleTokenPosition(activePlayer);
        waitingForCardButtonPress = false;
        requiredCardButtonType = BoardSpaceType.Start;

        if (activePlayer.TurnsToWait > 0)
        {
            ResolveWaitingTurn(activePlayer);

            if (!gameIsOver)
            {
                StartCoroutine(ResolveAutomaticMovementAndEndTurn(activePlayer));
            }

            return;
        }

        BoardSpaceData currentSpace = GetCurrentSpace(activePlayer);

        if (currentSpace == null)
        {
            SetStatus($"{activePlayer.CharacterName} is not on a valid board space.");
            FinishTurn();
            return;
        }

        switch (currentSpace.SpaceType)
        {
            case BoardSpaceType.Start:
            case BoardSpaceType.Jump:
                StartCoroutine(ResolveStartOfTurnAutomaticSpaces(activePlayer));
                return;

            case BoardSpaceType.Home:
                DeclareWinner(activePlayer, $"{activePlayer.CharacterName} reached Home.");
                return;

            case BoardSpaceType.Stop:
            case BoardSpaceType.Go:
            case BoardSpaceType.Community:
            case BoardSpaceType.Question:
                waitingForCardButtonPress = true;
                requiredCardButtonType = currentSpace.SpaceType;
                SetStatus($"{activePlayer.CharacterName} is on a {currentSpace.SpaceType} space. Click the matching button.");
                return;

            default:
                SetStatus("Unsupported space type.");
                FinishTurn();
                return;
        }
    }

    private void TryResolveCardButton(BoardSpaceType pressedButtonType)
    {
        if (gameIsOver || turnBusy)
        {
            return;
        }

        if (waitingForPopupClose)
        {
            SetStatus("Close the current card before clicking another button.");
            return;
        }

        if (!waitingForCardButtonPress)
        {
            SetStatus("You are not waiting for a card button right now.");
            return;
        }

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            SetStatus("No active player found.");
            return;
        }

        if (pressedButtonType != requiredCardButtonType)
        {
            SetStatus($"Wrong button. This player is on a {requiredCardButtonType} space.");
            return;
        }

        waitingForCardButtonPress = false;

        BoardSpaceData currentSpace = GetCurrentSpace(activePlayer);

        if (currentSpace == null)
        {
            SetStatus("Current board space could not be found.");
            FinishTurn();
            return;
        }

        bool resolvedSuccessfully = TryResolveCardSpace(
            activePlayer,
            currentSpace,
            out string popupTitle,
            out string popupBody);

        if (!resolvedSuccessfully)
        {
            FinishTurn();
            return;
        }

        waitingForPopupClose = true;
        ShowCardPopup(popupTitle, popupBody);
    }

    private bool TryResolveCardSpace(
        Player player,
        BoardSpaceData currentSpace,
        out string popupTitle,
        out string popupBody)
    {
        popupTitle = "";
        popupBody = "";

        CardDeck correctDeck = GetDeckForSpaceType(currentSpace.SpaceType);

        if (correctDeck == null)
        {
            SetStatus($"No deck is assigned for {currentSpace.SpaceType} spaces.");
            return false;
        }

        Card drawnCard = correctDeck.Draw();

        if (drawnCard == null)
        {
            SetStatus($"The {currentSpace.SpaceType} deck is empty.");
            return false;
        }

        CardResolution cardResolution = player.ApplyCard(drawnCard);

        bool shouldMoveImmediately =
            cardResolution.Movement != null &&
            cardResolution.Movement.MovementMode != MovementMode.None &&
            cardResolution.Movement.MovementTiming == MovementTiming.Immediate;

        if (shouldMoveImmediately)
        {
            ApplyMovementInstruction(player, cardResolution.Movement);
        }

        string resolutionText = cardResolution.ResolutionText;

        if (string.IsNullOrWhiteSpace(resolutionText))
        {
            resolutionText = drawnCard.RulesText;
        }

        popupTitle = drawnCard.Title;
        popupBody = drawnCard.RulesText;

        if (resolutionText != drawnCard.RulesText)
        {
            popupBody += "\n\nResult: " + resolutionText;
        }

        if (cardResolution.WaitTurnsAdded > 0)
        {
            popupBody += $"\n\nWait turns added: {cardResolution.WaitTurnsAdded}";
        }

        if (cardResolution.CashChangeAmount != 0)
        {
            popupBody += $"\nCash change: {cardResolution.CashChangeAmount}";
        }

        if (shouldMoveImmediately)
        {
            popupBody += $"\n\nNew space: {GetCurrentSpaceName(player)}";
        }

        UpdateSingleTokenPosition(player);
        SetStatus($"{player.CharacterName} drew '{drawnCard.Title}'.");

        return true;
    }

    private IEnumerator ResolveStartOfTurnAutomaticSpaces(Player player)
    {
        turnBusy = true;

        while (player != null && !gameIsOver)
        {
            BoardSpaceData currentSpace = GetCurrentSpace(player);

            if (currentSpace == null)
            {
                break;
            }

            if (currentSpace.SpaceType == BoardSpaceType.Start)
            {
                ResolveStartSpace(player);
                yield return null;
            }
            else if (currentSpace.SpaceType == BoardSpaceType.Jump)
            {
                yield return new WaitForSeconds(jumpDelaySeconds);
                ResolveJumpSpace(player, currentSpace);
            }
            else
            {
                break;
            }

            CheckForWinAfterMovement(player);

            if (gameIsOver)
            {
                turnBusy = false;
                yield break;
            }
        }

        turnBusy = false;

        if (gameIsOver || player == null)
        {
            yield break;
        }

        BoardSpaceData finalSpace = GetCurrentSpace(player);

        if (finalSpace == null)
        {
            FinishTurn();
            yield break;
        }

        switch (finalSpace.SpaceType)
        {
            case BoardSpaceType.Stop:
            case BoardSpaceType.Go:
            case BoardSpaceType.Community:
            case BoardSpaceType.Question:
                waitingForCardButtonPress = true;
                requiredCardButtonType = finalSpace.SpaceType;
                SetStatus($"{player.CharacterName} is on a {finalSpace.SpaceType} space. Click the matching button.");
                break;

            case BoardSpaceType.Home:
                DeclareWinner(player, $"{player.CharacterName} reached Home.");
                break;

            default:
                FinishTurn();
                break;
        }
    }

    private IEnumerator ResolveAutomaticMovementAndEndTurn(Player player)
    {
        turnBusy = true;

        while (player != null && !gameIsOver)
        {
            BoardSpaceData currentSpace = GetCurrentSpace(player);

            if (currentSpace == null)
            {
                break;
            }

            if (currentSpace.SpaceType == BoardSpaceType.Jump)
            {
                yield return new WaitForSeconds(jumpDelaySeconds);
                ResolveJumpSpace(player, currentSpace);
                CheckForWinAfterMovement(player);

                if (gameIsOver)
                {
                    turnBusy = false;
                    yield break;
                }

                continue;
            }

            break;
        }

        turnBusy = false;

        if (gameIsOver)
        {
            yield break;
        }

        FinishTurn();
    }

    private void ResolveWaitingTurn(Player player)
    {
        player.TurnsToWait--;

        if (player.TurnsToWait > 0)
        {
            SetStatus($"{player.CharacterName} must wait {player.TurnsToWait} more turn(s).");
            return;
        }

        SetStatus($"{player.CharacterName} finished waiting.");

        bool hasPendingMovement =
            player.PendingMovement != null &&
            player.PendingMovement.MovementMode != MovementMode.None;

        if (hasPendingMovement)
        {
            ApplyMovementInstruction(player, player.PendingMovement);
            player.ClearDelayedMovement();
            SetStatus($"{player.CharacterName} finished waiting and moved to {GetCurrentSpaceName(player)}.");
        }

        UpdateSingleTokenPosition(player);
    }

    private void ResolveStartSpace(Player player)
    {
        int firstGoSpaceIndex = FindFirstSpaceOfType(BoardSpaceType.Go);

        if (firstGoSpaceIndex == -1)
        {
            SetStatus("No GO space was found on the board.");
            return;
        }

        MovePlayerToIndex(player, firstGoSpaceIndex);
        SetStatus($"{player.CharacterName} moved from Start to the first GO space.");
    }

    private void ResolveJumpSpace(Player player, BoardSpaceData jumpSpace)
    {
        MovePlayerToIndex(player, player.BoardIndex + jumpSpace.JumpAmount);

        string directionWord = jumpSpace.JumpAmount >= 0 ? "forward" : "back";
        int absoluteJumpAmount = Mathf.Abs(jumpSpace.JumpAmount);

        SetStatus($"{player.CharacterName} hit an explosion space and moved {directionWord} {absoluteJumpAmount} space(s).");
    }

    private void ApplyMovementInstruction(Player player, MovementInstruction movementInstruction)
    {
        if (player == null || movementInstruction == null)
        {
            return;
        }

        int occurrenceCount = Mathf.Max(1, Mathf.Abs(movementInstruction.SpaceCount));

        switch (movementInstruction.MovementMode)
        {
            case MovementMode.None:
                return;

            case MovementMode.RelativeSpaces:
                MovePlayerToIndex(player, player.BoardIndex + movementInstruction.SpaceCount);
                return;

            case MovementMode.NextSpaceOfType:
                MovePlayerToIndex(
                    player,
                    FindNextSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType, occurrenceCount));
                return;

            case MovementMode.PreviousSpaceOfType:
                MovePlayerToIndex(
                    player,
                    FindPreviousSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType, occurrenceCount));
                return;

            case MovementMode.NearestSpaceOfType:
                MovePlayerToIndex(
                    player,
                    FindNearestSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType));
                return;
        }
    }

    private void MovePlayerToIndex(Player player, int targetIndex)
    {
        if (player == null || boardSpaces == null || boardSpaces.Count == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(targetIndex, 0, boardSpaces.Count - 1);
        player.BoardIndex = clampedIndex;
        UpdateSingleTokenPosition(player);
        RefreshPlayerStatusPopup();
    }

    private void UpdateSingleTokenPosition(Player player)
    {
        if (player == null)
        {
            return;
        }

        int playerIndex = activePlayers.IndexOf(player);

        if (playerIndex < 0)
        {
            playerIndex = 0;
        }

        EnsureTokenVisuals(activePlayers.Count);

        if (playerIndex >= runtimeTokenTransforms.Count)
        {
            return;
        }

        if (player.BoardIndex < 0 || player.BoardIndex >= boardSpaces.Count)
        {
            return;
        }

        BoardSpaceData currentSpace = boardSpaces[player.BoardIndex];

        if (currentSpace == null || currentSpace.WorldPosition == null)
        {
            return;
        }

        runtimeTokenTransforms[playerIndex].position =
            currentSpace.WorldPosition.position + GetTokenOffsetForPlayer(playerIndex);
    }

    private void UpdateAllTokenPositions()
    {
        EnsureTokenVisuals(activePlayers.Count);

        for (int playerIndex = 0; playerIndex < activePlayers.Count; playerIndex++)
        {
            UpdateSingleTokenPosition(activePlayers[playerIndex]);
        }
    }

    private void EnsureTokenVisuals(int playerCount)
    {
        if (playerTokenTransform == null)
        {
            return;
        }

        if (runtimeTokenTransforms.Count == 0)
        {
            runtimeTokenTransforms.Add(playerTokenTransform);
            ApplyTokenColor(playerTokenTransform, 0);
        }

        while (runtimeTokenTransforms.Count < playerCount)
        {
            Transform tokenTransform = Instantiate(playerTokenTransform, playerTokenTransform.parent);
            tokenTransform.name = $"PlayerToken_{runtimeTokenTransforms.Count + 1}";
            runtimeTokenTransforms.Add(tokenTransform);
            ApplyTokenColor(tokenTransform, runtimeTokenTransforms.Count - 1);
        }

        for (int tokenIndex = 0; tokenIndex < runtimeTokenTransforms.Count; tokenIndex++)
        {
            if (runtimeTokenTransforms[tokenIndex] != null)
            {
                runtimeTokenTransforms[tokenIndex].gameObject.SetActive(tokenIndex < playerCount);
            }
        }
    }

    private void ApplyTokenColor(Transform tokenTransform, int playerIndex)
    {
        SpriteRenderer spriteRenderer = tokenTransform.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            return;
        }

        Color[] tokenColors =
        {
            new Color(0.16f, 0.46f, 0.95f),
            new Color(0.9f, 0.22f, 0.18f),
            new Color(0.14f, 0.68f, 0.32f),
            new Color(0.95f, 0.72f, 0.18f)
        };

        spriteRenderer.color = tokenColors[playerIndex % tokenColors.Length];
    }

    private Vector3 GetTokenOffsetForPlayer(int playerIndex)
    {
        float horizontalOffset = ((playerIndex % 2) - 0.5f) * 0.2f;
        float verticalOffset = (playerIndex / 2) * 0.16f;

        return tokenOffset + new Vector3(horizontalOffset, verticalOffset, 0f);
    }

    private void ShowCardPopup(string title, string body)
    {
        currentPopupTitle = title;
        currentPopupBody = body;

        if (cardPopupTitleText != null)
        {
            cardPopupTitleText.text = title;
        }

        if (cardPopupBodyText != null)
        {
            cardPopupBodyText.text = body;
        }

        if (cardPopupPanel != null)
        {
            cardPopupPanel.SetActive(true);
        }
    }

    private void HideCardPopup()
    {
        if (!waitingForPopupClose)
        {
            currentPopupTitle = "";
            currentPopupBody = "";
        }

        if (cardPopupPanel != null)
        {
            cardPopupPanel.SetActive(false);
        }
    }

    private void SetStatus(string message)
    {
        lastTurnSummary = message;

        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
        BroadcastMultiplayerSnapshot();
    }

    private int FindFirstSpaceOfType(BoardSpaceType targetSpaceType)
    {
        for (int boardIndex = 0; boardIndex < boardSpaces.Count; boardIndex++)
        {
            if (boardSpaces[boardIndex].SpaceType == targetSpaceType)
            {
                return boardIndex;
            }
        }

        return -1;
    }

    private int FindNextSpaceOfType(int startingIndex, BoardSpaceType targetSpaceType, int occurrences = 1)
    {
        int foundCount = 0;

        for (int boardIndex = startingIndex + 1; boardIndex < boardSpaces.Count; boardIndex++)
        {
            if (boardSpaces[boardIndex].SpaceType != targetSpaceType)
            {
                continue;
            }

            foundCount++;

            if (foundCount >= occurrences)
            {
                return boardIndex;
            }
        }

        return startingIndex;
    }

    private int FindPreviousSpaceOfType(int startingIndex, BoardSpaceType targetSpaceType, int occurrences = 1)
    {
        int foundCount = 0;

        for (int boardIndex = startingIndex - 1; boardIndex >= 0; boardIndex--)
        {
            if (boardSpaces[boardIndex].SpaceType != targetSpaceType)
            {
                continue;
            }

            foundCount++;

            if (foundCount >= occurrences)
            {
                return boardIndex;
            }
        }

        return startingIndex;
    }

    private int FindNearestSpaceOfType(int startingIndex, BoardSpaceType targetSpaceType)
    {
        int nearestIndex = startingIndex;
        int shortestDistance = int.MaxValue;

        for (int boardIndex = 0; boardIndex < boardSpaces.Count; boardIndex++)
        {
            if (boardSpaces[boardIndex].SpaceType != targetSpaceType)
            {
                continue;
            }

            int currentDistance = Mathf.Abs(boardIndex - startingIndex);

            if (currentDistance < shortestDistance)
            {
                shortestDistance = currentDistance;
                nearestIndex = boardIndex;
            }
        }

        return nearestIndex;
    }

    private void CheckForWinAfterMovement(Player player)
    {
        BoardSpaceData currentSpace = GetCurrentSpace(player);

        if (currentSpace == null)
        {
            return;
        }

        if (currentSpace.SpaceType == BoardSpaceType.Home)
        {
            DeclareWinner(player, $"{player.CharacterName} reached Home.");
            return;
        }

        if (player.BoardIndex >= boardSpaces.Count - 1)
        {
            DeclareWinner(player, $"{player.CharacterName} reached the end of the board.");
        }
    }

    private void DeclareWinner(Player player, string summaryText)
    {
        gameIsOver = true;
        waitingForCardButtonPress = false;
        waitingForPopupClose = false;
        requiredCardButtonType = BoardSpaceType.Start;
        turnBusy = false;

        HideCardPopup();
        SetStatus(summaryText);
        ShowWinScreen(player);
    }

    private void FinishTurn()
    {
        if (activePlayers.Count == 0)
        {
            return;
        }

        currentPlayerIndex++;

        if (currentPlayerIndex >= activePlayers.Count)
        {
            currentPlayerIndex = 0;
        }

        BeginCurrentPlayerTurn();
    }

    private int GetStartingBoardIndex()
    {
        if (!startPlayersOnFirstGoSpace)
        {
            return 0;
        }

        int firstGoSpaceIndex = FindFirstSpaceOfType(BoardSpaceType.Go);

        if (firstGoSpaceIndex == -1)
        {
            return 0;
        }

        return firstGoSpaceIndex;
    }

    public BoardSpaceData GetCurrentSpace(Player player)
    {
        if (player == null)
        {
            return null;
        }

        if (player.BoardIndex < 0 || player.BoardIndex >= boardSpaces.Count)
        {
            return null;
        }

        return boardSpaces[player.BoardIndex];
    }

    private string GetCurrentSpaceName(Player player)
    {
        BoardSpaceData currentSpace = GetCurrentSpace(player);

        if (currentSpace == null)
        {
            return "Unknown";
        }

        if (!string.IsNullOrWhiteSpace(currentSpace.SpaceName))
        {
            return currentSpace.SpaceName;
        }

        return currentSpace.SpaceType.ToString();
    }

    private void RegisterNetworkMessages()
    {
        if (networkMessagesRegistered || NetworkManager.Singleton == null)
        {
            return;
        }

        CustomMessagingManager messagingManager = NetworkManager.Singleton.CustomMessagingManager;

        if (messagingManager == null)
        {
            return;
        }

        messagingManager.RegisterNamedMessageHandler(SnapshotMessageName, OnSnapshotMessageReceived);
        NetworkManager.Singleton.OnClientConnectedCallback -= OnNetworkClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnNetworkClientConnected;

        if (multiplayerHost)
        {
            messagingManager.RegisterNamedMessageHandler(ActionMessageName, OnActionMessageReceived);
        }

        networkMessagesRegistered = true;
    }

    private void OnNetworkClientConnected(ulong clientId)
    {
        if (multiplayerHost && activePlayers.Count > 0)
        {
            SendSnapshotToClient(clientId);
        }
    }

    private void OnSnapshotMessageReceived(ulong senderClientId, FastBufferReader messagePayload)
    {
        if (multiplayerHost)
        {
            return;
        }

        string json = ReadJsonMessage(messagePayload);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        MultiplayerGameSnapshot snapshot = JsonUtility.FromJson<MultiplayerGameSnapshot>(json);
        ApplyMultiplayerSnapshot(snapshot);
    }

    private void OnActionMessageReceived(ulong senderClientId, FastBufferReader messagePayload)
    {
        if (!multiplayerHost)
        {
            return;
        }

        string json = ReadJsonMessage(messagePayload);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        MultiplayerGameAction action = JsonUtility.FromJson<MultiplayerGameAction>(json);

        if (action.ActionType == MultiplayerGameAction.RequestSnapshotAction)
        {
            SendSnapshotToClient(senderClientId);
            return;
        }

        if (!SenderOwnsCurrentTurn(senderClientId))
        {
            SendSnapshotToClient(senderClientId);
            return;
        }

        if (action.ActionType == MultiplayerGameAction.CardButtonAction)
        {
            TryResolveCardButton((BoardSpaceType)action.BoardSpaceType);
        }
        else if (action.ActionType == MultiplayerGameAction.ClosePopupAction)
        {
            ResolveCloseCardPopup();
        }

        BroadcastMultiplayerSnapshot();
    }

    private bool SenderOwnsCurrentTurn(ulong senderClientId)
    {
        if (CurrentPlayer == null)
        {
            return false;
        }

        return GetCurrentTurnOwnerClientId() == senderClientId;
    }

    private void SendMultiplayerAction(MultiplayerGameAction action)
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("Network is not ready yet.");
            return;
        }

        string json = JsonUtility.ToJson(action);
        SendJsonMessage(ActionMessageName, NetworkManager.ServerClientId, json);
    }

    private void BroadcastMultiplayerSnapshot()
    {
        if (!multiplayerMode || !multiplayerHost || applyingNetworkSnapshot)
        {
            return;
        }

        if (activePlayers == null || activePlayers.Count == 0)
        {
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsListening)
        {
            return;
        }

        MultiplayerGameSnapshot snapshot = CreateMultiplayerSnapshot();
        string json = JsonUtility.ToJson(snapshot);
        List<ulong> recipientClientIds = new List<ulong>();

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            if (clientId != networkManager.LocalClientId)
            {
                recipientClientIds.Add(clientId);
            }
        }

        if (recipientClientIds.Count == 0)
        {
            return;
        }

        SendJsonMessage(SnapshotMessageName, recipientClientIds, json);
    }

    private void SendSnapshotToClient(ulong clientId)
    {
        if (!multiplayerMode || !multiplayerHost || activePlayers.Count == 0)
        {
            return;
        }

        string json = JsonUtility.ToJson(CreateMultiplayerSnapshot());
        SendJsonMessage(SnapshotMessageName, clientId, json);
    }

    private MultiplayerGameSnapshot CreateMultiplayerSnapshot()
    {
        MultiplayerGameSnapshot snapshot = new MultiplayerGameSnapshot
        {
            CurrentPlayerIndex = currentPlayerIndex,
            GameIsOver = gameIsOver,
            LastTurnSummary = lastTurnSummary,
            WaitingForCardButtonPress = waitingForCardButtonPress,
            RequiredCardButtonType = (int)requiredCardButtonType,
            WaitingForPopupClose = waitingForPopupClose,
            TurnBusy = turnBusy,
            PopupTitle = currentPopupTitle,
            PopupBody = currentPopupBody,
            SessionCode = MultiplayerSessionContext.SessionCode
        };

        for (int playerIndex = 0; playerIndex < activePlayers.Count; playerIndex++)
        {
            snapshot.Players.Add(activePlayers[playerIndex].Clone());

            ulong ownerClientId = playerIndex < activePlayerOwnerClientIds.Count
                ? activePlayerOwnerClientIds[playerIndex]
                : GetCurrentTurnOwnerClientId();

            snapshot.OwnerClientIds.Add(ownerClientId.ToString());
        }

        return snapshot;
    }

    private void ApplyMultiplayerSnapshot(MultiplayerGameSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        applyingNetworkSnapshot = true;

        activePlayers.Clear();

        for (int playerIndex = 0; playerIndex < snapshot.Players.Count; playerIndex++)
        {
            Player snapshotPlayer = snapshot.Players[playerIndex];

            if (snapshotPlayer != null)
            {
                activePlayers.Add(snapshotPlayer.Clone());
            }
        }

        activePlayerOwnerClientIds.Clear();

        for (int ownerIndex = 0; ownerIndex < snapshot.OwnerClientIds.Count; ownerIndex++)
        {
            if (ulong.TryParse(snapshot.OwnerClientIds[ownerIndex], out ulong ownerClientId))
            {
                activePlayerOwnerClientIds.Add(ownerClientId);
            }
        }

        TrimMultiplayerOwnerListToActivePlayers();
        currentPlayerIndex = Mathf.Clamp(snapshot.CurrentPlayerIndex, 0, Mathf.Max(activePlayers.Count - 1, 0));
        gameIsOver = snapshot.GameIsOver;
        lastTurnSummary = snapshot.LastTurnSummary;
        waitingForCardButtonPress = snapshot.WaitingForCardButtonPress;
        requiredCardButtonType = (BoardSpaceType)snapshot.RequiredCardButtonType;
        waitingForPopupClose = snapshot.WaitingForPopupClose;
        turnBusy = snapshot.TurnBusy;
        currentPopupTitle = snapshot.PopupTitle;
        currentPopupBody = snapshot.PopupBody;

        HideCharacterSelect();
        EnsureTokenVisuals(activePlayers.Count);
        UpdateAllTokenPositions();
        RefreshPlayerStatusPopup();

        if (statusText != null)
        {
            statusText.text = lastTurnSummary;
        }

        if (waitingForPopupClose && !string.IsNullOrWhiteSpace(currentPopupTitle))
        {
            ShowCardPopup(currentPopupTitle, currentPopupBody);
        }
        else
        {
            HideCardPopup();
        }

        if (gameIsOver)
        {
            ShowWinScreen(CurrentPlayer);
        }
        else
        {
            HideWinScreen();
        }

        applyingNetworkSnapshot = false;
    }

    private string ReadJsonMessage(FastBufferReader messagePayload)
    {
        messagePayload.ReadValueSafe(out string json);
        return json;
    }

    private void SendJsonMessage(string messageName, ulong clientId, string json)
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.CustomMessagingManager == null)
        {
            return;
        }

        int writeSize = FastBufferWriter.GetWriteSize(json) + 128;
        using (FastBufferWriter writer = new FastBufferWriter(writeSize, Allocator.Temp, writeSize * 2))
        {
            writer.WriteValueSafe(json);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                messageName,
                clientId,
                writer,
                NetworkDelivery.ReliableFragmentedSequenced);
        }
    }

    private void SendJsonMessage(string messageName, IReadOnlyList<ulong> clientIds, string json)
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.CustomMessagingManager == null ||
            clientIds == null ||
            clientIds.Count == 0)
        {
            return;
        }

        int writeSize = FastBufferWriter.GetWriteSize(json) + 128;
        using (FastBufferWriter writer = new FastBufferWriter(writeSize, Allocator.Temp, writeSize * 2))
        {
            writer.WriteValueSafe(json);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                messageName,
                clientIds,
                writer,
                NetworkDelivery.ReliableFragmentedSequenced);
        }
    }

    private CardDeck GetDeckForSpaceType(BoardSpaceType boardSpaceType)
    {
        switch (boardSpaceType)
        {
            case BoardSpaceType.Stop:
                return stopDeck;

            case BoardSpaceType.Go:
                return goDeck;

            case BoardSpaceType.Community:
                return communityDeck;

            case BoardSpaceType.Question:
                return questionDeck;

            default:
                return null;
        }
    }
}

[Serializable]
public class MultiplayerGameSnapshot
{
    public List<Player> Players = new List<Player>();
    public List<string> OwnerClientIds = new List<string>();
    public int CurrentPlayerIndex;
    public bool GameIsOver;
    public string LastTurnSummary = "";
    public bool WaitingForCardButtonPress;
    public int RequiredCardButtonType;
    public bool WaitingForPopupClose;
    public bool TurnBusy;
    public string PopupTitle = "";
    public string PopupBody = "";
    public string SessionCode = "";
}

[Serializable]
public class MultiplayerGameAction
{
    public const string CardButtonAction = "CardButton";
    public const string ClosePopupAction = "ClosePopup";
    public const string RequestSnapshotAction = "RequestSnapshot";

    public string ActionType = "";
    public int BoardSpaceType;

    public static MultiplayerGameAction CardButton(BoardSpaceType boardSpaceType)
    {
        return new MultiplayerGameAction
        {
            ActionType = CardButtonAction,
            BoardSpaceType = (int)boardSpaceType
        };
    }

    public static MultiplayerGameAction ClosePopup()
    {
        return new MultiplayerGameAction
        {
            ActionType = ClosePopupAction
        };
    }

    public static MultiplayerGameAction RequestSnapshot()
    {
        return new MultiplayerGameAction
        {
            ActionType = RequestSnapshotAction
        };
    }
}
