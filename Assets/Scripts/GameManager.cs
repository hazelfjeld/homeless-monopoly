using System.Collections.Generic;
using UnityEngine;

public class EscapeHomelessnessGameManager : MonoBehaviour
{
    [Header("Token Visual")]
    [SerializeField] private Transform playerTokenTransform;
    [SerializeField] private Vector3 tokenOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Board")]
    [SerializeField] private List<BoardSpaceData> boardSpaces = new List<BoardSpaceData>();
    [SerializeField] private bool startPlayersOnFirstGoSpace = true;

    [Header("Character Templates")]
    [SerializeField] private List<Player> characterTemplates = new List<Player>();

    [Header("Decks")]
    [SerializeField] private CardDeck goDeck = new CardDeck();
    [SerializeField] private CardDeck stopDeck = new CardDeck();
    [SerializeField] private CardDeck questionDeck = new CardDeck();
    [SerializeField] private CardDeck communityDeck = new CardDeck();

    [Header("Runtime State")]
    [SerializeField] private List<Player> activePlayers = new List<Player>();
    [SerializeField] private int currentPlayerIndex;
    [SerializeField] private bool gameIsOver;
    [SerializeField] private string lastTurnSummary = "";

    public IReadOnlyList<Player> ActivePlayers => activePlayers;
    public int CurrentPlayerIndex => currentPlayerIndex;
    public bool GameIsOver => gameIsOver;
    public string LastTurnSummary => lastTurnSummary;

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

    public void StartGameWithAllTemplates()
    {
        StartGame(characterTemplates);
    }

    public void StartGame(List<Player> selectedCharacterTemplates)
    {
        if (boardSpaces.Count == 0)
        {
            Debug.LogWarning("Cannot start game. No board spaces were assigned.");
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
            Debug.LogWarning("Cannot start game. No players were created.");
            return;
        }

        currentPlayerIndex = 0;
        gameIsOver = false;
        lastTurnSummary = "Game started.";

        goDeck.ResetDeck();
        stopDeck.ResetDeck();
        questionDeck.ResetDeck();
        communityDeck.ResetDeck();
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
    public void PlayCurrentPlayerTurn()
    {
        if (gameIsOver)
        {
            lastTurnSummary = "The game is already over.";
            return;
        }

        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            lastTurnSummary = "No active player found.";
            return;
        }

        if (activePlayer.TurnsToWait > 0)
        {
            ResolveWaitingTurn(activePlayer);
            FinishTurn();
            return;
        }

        BoardSpaceData currentSpace = GetCurrentSpace(activePlayer);

        if (currentSpace == null)
        {
            lastTurnSummary = $"{activePlayer.CharacterName} is not on a valid space.";
            FinishTurn();
            return;
        }

        switch (currentSpace.SpaceType)
        {
            case BoardSpaceType.Start:
                ResolveStartSpace(activePlayer);
                break;

            case BoardSpaceType.Jump:
                ResolveJumpSpace(activePlayer, currentSpace);
                break;

            case BoardSpaceType.Home:
                DeclareWinner(activePlayer, $"{activePlayer.CharacterName} reached home.");
                return;

            case BoardSpaceType.Go:
            case BoardSpaceType.Stop:
            case BoardSpaceType.Question:
            case BoardSpaceType.Community:
                ResolveCardSpace(activePlayer, currentSpace);
                break;

            default:
                lastTurnSummary = $"{activePlayer.CharacterName} landed on an unsupported space type.";
                break;
        }

        if (!gameIsOver)
        {
            CheckForWinAfterMovement(activePlayer);
        }

        if (!gameIsOver)
        {
            FinishTurn();
        }
    }

    public void ForceMoveCurrentPlayer(int spaceAmount)
    {
        Player activePlayer = CurrentPlayer;

        if (activePlayer == null)
        {
            return;
        }

        MovePlayerToIndex(activePlayer, activePlayer.BoardIndex + spaceAmount);
        lastTurnSummary = $"{activePlayer.CharacterName} moved {spaceAmount} spaces.";
        CheckForWinAfterMovement(activePlayer);
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
    }

    private void ResolveStartSpace(Player player)
    {
        int startingPlayableIndex = FindFirstSpaceOfType(BoardSpaceType.Go);

        if (startingPlayableIndex == -1)
        {
            lastTurnSummary = "No GO space was found on the board.";
            return;
        }

        MovePlayerToIndex(player, startingPlayableIndex);
        lastTurnSummary = $"{player.CharacterName} moved from Start to the first GO space.";
    }

    private void ResolveJumpSpace(Player player, BoardSpaceData jumpSpace)
    {
        MovePlayerToIndex(player, player.BoardIndex + jumpSpace.JumpAmount);

        string directionWord = jumpSpace.JumpAmount >= 0 ? "forward" : "back";
        int absoluteAmount = Mathf.Abs(jumpSpace.JumpAmount);

        lastTurnSummary =
            $"{player.CharacterName} hit a jump space and moved {directionWord} {absoluteAmount} space(s).";
    }

    private void ResolveCardSpace(Player player, BoardSpaceData currentSpace)
    {
        CardDeck deckForSpace = GetDeckForBoardSpace(currentSpace.SpaceType);

        if (deckForSpace == null)
        {
            lastTurnSummary = $"No deck was assigned for {currentSpace.SpaceType} spaces.";
            return;
        }

        Card drawnCard = deckForSpace.Draw();

        if (drawnCard == null)
        {
            lastTurnSummary = $"The {currentSpace.SpaceType} deck is empty.";
            return;
        }

        CardResolution resolution = player.ApplyCard(drawnCard);

        bool shouldMoveImmediately =
            resolution.Movement != null &&
            resolution.Movement.MovementMode != MovementMode.None &&
            resolution.Movement.MovementTiming == MovementTiming.Immediate;

        if (shouldMoveImmediately)
        {
            ApplyMovementInstruction(player, resolution.Movement);
        }

        string cardText = string.IsNullOrWhiteSpace(resolution.ResolutionText)
            ? drawnCard.RulesText
            : resolution.ResolutionText;

        lastTurnSummary =
            $"{player.CharacterName} drew '{drawnCard.Title}'. {cardText}";

        if (resolution.WaitTurnsAdded > 0)
        {
            lastTurnSummary += $" Wait turns: {resolution.WaitTurnsAdded}.";
        }

        if (resolution.CashChangeAmount != 0)
        {
            lastTurnSummary += $" Cash change: {resolution.CashChangeAmount}.";
        }

        if (shouldMoveImmediately)
        {
            lastTurnSummary += $" New space: {GetCurrentSpaceName(player)}.";
        }
    }

    private void ApplyMovementInstruction(Player player, MovementInstruction movementInstruction)
    {
        if (player == null || movementInstruction == null)
        {
            return;
        }

        switch (movementInstruction.MovementMode)
        {
            case MovementMode.None:
                return;

            case MovementMode.RelativeSpaces:
                MovePlayerToIndex(player, player.BoardIndex + movementInstruction.SpaceCount);
                return;

            case MovementMode.NextSpaceOfType:
                MovePlayerToIndex(player, FindNextSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType));
                return;

            case MovementMode.PreviousSpaceOfType:
                MovePlayerToIndex(player, FindPreviousSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType));
                return;

            case MovementMode.NearestSpaceOfType:
                MovePlayerToIndex(player, FindNearestSpaceOfType(player.BoardIndex, movementInstruction.TargetSpaceType));
                return;
        }
    }

    private void MovePlayerToIndex(Player player, int targetIndex)
    {
        if (player == null || boardSpaces.Count == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(targetIndex, 0, boardSpaces.Count - 1);
        player.BoardIndex = clampedIndex;

        UpdateSingleTokenPosition(player);
    }

    private int FindNextSpaceOfType(int startingIndex, BoardSpaceType targetSpaceType)
    {
        for (int boardIndex = startingIndex + 1; boardIndex < boardSpaces.Count; boardIndex++)
        {
            if (boardSpaces[boardIndex].SpaceType == targetSpaceType)
            {
                return boardIndex;
            }
        }

        return startingIndex;
    }

    private int FindPreviousSpaceOfType(int startingIndex, BoardSpaceType targetSpaceType)
    {
        for (int boardIndex = startingIndex - 1; boardIndex >= 0; boardIndex--)
        {
            if (boardSpaces[boardIndex].SpaceType == targetSpaceType)
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

    private CardDeck GetDeckForBoardSpace(BoardSpaceType boardSpaceType)
    {
        switch (boardSpaceType)
        {
            case BoardSpaceType.Go:
                return goDeck;

            case BoardSpaceType.Stop:
                return stopDeck;

            case BoardSpaceType.Question:
                return questionDeck;

            case BoardSpaceType.Community:
                return communityDeck;

            default:
                return null;
        }
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
            DeclareWinner(player, $"{player.CharacterName} reached home.");
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
}