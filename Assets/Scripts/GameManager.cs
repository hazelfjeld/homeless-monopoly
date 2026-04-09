using System.Collections.Generic;
using UnityEngine;

public class EscapeHomelessnessGameManager : MonoBehaviour
{
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

    private void Awake()
    {
        LoadDeckData();
    }

    private void Start()
    {
        if (autoStartGame)
        {
            StartGameWithAllTemplates();
        }
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
            lastTurnSummary = "No board spaces have been set up yet.";
            Debug.LogWarning(lastTurnSummary);
            return;
        }

        if (selectedCharacterTemplates == null || selectedCharacterTemplates.Count == 0)
        {
            lastTurnSummary = "No character templates were provided.";
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
            lastTurnSummary = "No playable characters were created.";
            Debug.LogWarning(lastTurnSummary);
            return;
        }

        currentPlayerIndex = 0;
        gameIsOver = false;
        waitingForCardButtonPress = false;
        requiredCardButtonType = BoardSpaceType.Start;
        lastTurnSummary = "Game started.";

        stopDeck.ResetDeck();
        goDeck.ResetDeck();
        communityDeck.ResetDeck();
        questionDeck.ResetDeck();

        UpdateSingleTokenPosition(CurrentPlayer);
        BeginCurrentPlayerTurn();
    }

    public void PlayCurrentPlayerTurn()
    {
        BeginCurrentPlayerTurn();
    }

    public void PressStopButton()
    {
        TryResolveCardButton(BoardSpaceType.Stop);
    }

    public void PressGoButton()
    {
        TryResolveCardButton(BoardSpaceType.Go);
    }

    public void PressCommunityButton()
    {
        TryResolveCardButton(BoardSpaceType.Community);
    }

    public void PressQuestionButton()
    {
        TryResolveCardButton(BoardSpaceType.Question);
    }

    private void BeginCurrentPlayerTurn()
    {
        if (gameIsOver)
        {
            return;
        }

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            lastTurnSummary = "No active player found.";
            return;
        }

        UpdateSingleTokenPosition(activePlayer);
        waitingForCardButtonPress = false;
        requiredCardButtonType = BoardSpaceType.Start;

        if (activePlayer.TurnsToWait > 0)
        {
            ResolveWaitingTurn(activePlayer);
            CheckForWinAfterMovement(activePlayer);

            if (!gameIsOver)
            {
                FinishTurn();
            }

            return;
        }

        BoardSpaceData currentSpace = GetCurrentSpace(activePlayer);

        if (currentSpace == null)
        {
            lastTurnSummary = $"{activePlayer.CharacterName} is not on a valid board space.";
            FinishTurn();
            return;
        }

        switch (currentSpace.SpaceType)
        {
            case BoardSpaceType.Start:
                ResolveStartSpace(activePlayer);
                CheckForWinAfterMovement(activePlayer);

                if (!gameIsOver)
                {
                    FinishTurn();
                }
                return;

            case BoardSpaceType.Jump:
                ResolveJumpSpace(activePlayer, currentSpace);
                CheckForWinAfterMovement(activePlayer);

                if (!gameIsOver)
                {
                    FinishTurn();
                }
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
                lastTurnSummary = $"{activePlayer.CharacterName} is on a {currentSpace.SpaceType} space. Click the matching button.";
                return;

            default:
                lastTurnSummary = "Unsupported space type.";
                FinishTurn();
                return;
        }
    }

    private void TryResolveCardButton(BoardSpaceType pressedButtonType)
    {
        if (gameIsOver)
        {
            return;
        }

        if (!waitingForCardButtonPress)
        {
            lastTurnSummary = "You are not waiting for a card button right now.";
            return;
        }

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            lastTurnSummary = "No active player found.";
            return;
        }

        if (pressedButtonType != requiredCardButtonType)
        {
            lastTurnSummary = $"Wrong button. This player is on a {requiredCardButtonType} space.";
            return;
        }

        waitingForCardButtonPress = false;

        BoardSpaceData currentSpace = GetCurrentSpace(activePlayer);

        if (currentSpace == null)
        {
            lastTurnSummary = "Current board space could not be found.";
            FinishTurn();
            return;
        }

        ResolveCardSpace(activePlayer, currentSpace);
        CheckForWinAfterMovement(activePlayer);

        if (!gameIsOver)
        {
            FinishTurn();
        }
    }

    private void ResolveCardSpace(Player player, BoardSpaceData currentSpace)
    {
        CardDeck correctDeck = GetDeckForSpaceType(currentSpace.SpaceType);

        if (correctDeck == null)
        {
            lastTurnSummary = $"No deck is assigned for {currentSpace.SpaceType} spaces.";
            return;
        }

        Card drawnCard = correctDeck.Draw();

        if (drawnCard == null)
        {
            lastTurnSummary = $"The {currentSpace.SpaceType} deck is empty.";
            return;
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

        lastTurnSummary = $"{player.CharacterName} drew '{drawnCard.Title}'. {resolutionText}";

        if (cardResolution.WaitTurnsAdded > 0)
        {
            lastTurnSummary += $" Wait turns added: {cardResolution.WaitTurnsAdded}.";
        }

        if (cardResolution.CashChangeAmount != 0)
        {
            lastTurnSummary += $" Cash change: {cardResolution.CashChangeAmount}.";
        }

        if (shouldMoveImmediately)
        {
            lastTurnSummary += $" New space: {GetCurrentSpaceName(player)}.";
        }

        UpdateSingleTokenPosition(player);
    }

    private void ResolveWaitingTurn(Player player)
    {
        player.TurnsToWait--;

        if (player.TurnsToWait > 0)
        {
            lastTurnSummary = $"{player.CharacterName} must wait {player.TurnsToWait} more turn(s).";
            return;
        }

        lastTurnSummary = $"{player.CharacterName} finished waiting.";

        bool hasPendingMovement =
            player.PendingMovement != null &&
            player.PendingMovement.MovementMode != MovementMode.None;

        if (hasPendingMovement)
        {
            ApplyMovementInstruction(player, player.PendingMovement);
            player.ClearDelayedMovement();
            lastTurnSummary += $" Then moved to {GetCurrentSpaceName(player)}.";
        }

        UpdateSingleTokenPosition(player);
    }

    private void ResolveStartSpace(Player player)
    {
        int firstGoSpaceIndex = FindFirstSpaceOfType(BoardSpaceType.Go);

        if (firstGoSpaceIndex == -1)
        {
            lastTurnSummary = "No GO space was found on the board.";
            return;
        }

        MovePlayerToIndex(player, firstGoSpaceIndex);
        lastTurnSummary = $"{player.CharacterName} moved from Start to the first GO space.";
    }

    private void ResolveJumpSpace(Player player, BoardSpaceData jumpSpace)
    {
        MovePlayerToIndex(player, player.BoardIndex + jumpSpace.JumpAmount);

        string directionWord = jumpSpace.JumpAmount >= 0 ? "forward" : "back";
        int absoluteJumpAmount = Mathf.Abs(jumpSpace.JumpAmount);

        lastTurnSummary = $"{player.CharacterName} hit a jump space and moved {directionWord} {absoluteJumpAmount} space(s).";
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
    }

    private void UpdateSingleTokenPosition(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (playerTokenTransform == null)
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

        playerTokenTransform.position = currentSpace.WorldPosition.position + tokenOffset;
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
        requiredCardButtonType = BoardSpaceType.Start;
        lastTurnSummary = summaryText;
        Debug.Log(summaryText);
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