# WebGL Build Notes

1. Open **File > Build Settings** and select **WebGL**.
2. Ensure these scenes are included and enabled:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/MainScene.unity`
3. In **Player Settings > Resolution and Presentation**, choose a fixed canvas size appropriate for classroom demos.
4. In **Publishing Settings**, use Compression Format supported by your host (Gzip/Brotli).
5. Build and host via HTTPS (required for most browser networking features).
6. For multiplayer demos, validate Relay/Services environment keys before publishing.
7. Test host + client in separate browser profiles to avoid session crossover.
