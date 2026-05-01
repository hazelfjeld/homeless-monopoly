using System.Collections.Generic;

public static class LobbyGameBootstrap
{
    private static readonly List<string> lobbyPlayerNames = new List<string>();

    public static IReadOnlyList<string> LobbyPlayerNames => lobbyPlayerNames;

    public static void SetLobbyPlayers(IEnumerable<string> playerNames)
    {
        lobbyPlayerNames.Clear();

        if (playerNames == null)
        {
            return;
        }

        foreach (string playerName in playerNames)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                continue;
            }

            lobbyPlayerNames.Add(playerName.Trim());
        }
    }
}
