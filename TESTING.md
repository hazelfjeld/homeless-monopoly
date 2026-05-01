# Escape Homelessness Multiplayer Demo Testing

## Prerequisites
1. Unity Services configured and Anonymous Auth working.
2. MainMenu and MainScene included in Build Settings.
3. Two build instances (Editor + standalone, or two standalone clients).

## Exact Test Steps
1. Launch Host instance and open Main Menu.
2. Click **Host** to create lobby.
3. Verify a lobby code appears.
4. Launch Client instance.
5. Click **Join**, enter host lobby code, click **Join Confirm**.
6. Verify client reaches host lobby panel and waits.
7. On host, click **Start Button**.
8. Verify both instances load `MainScene`.
9. Verify only current turn player can effectively advance turn/card resolution.
10. Trigger each card button type during relevant board spaces and verify state text/turn progresses.
11. Play until a winner is declared.
12. Close one client instance and verify host remains running without crash.

## Pass Criteria
- Scene transition works from host start.
- Turn and summary state replicate to client.
- No hard crash on client close.
