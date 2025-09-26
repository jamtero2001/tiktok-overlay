# League of Legends TikTok Overlay

A Windows overlay application that displays TikTok videos on top of League of Legends gameplay.

## Features

- **Automatic League Detection**: Automatically appears when League of Legends is running
- **TikTok Integration**: Embed and display TikTok videos directly in the overlay
- **Customizable Position**: Move and resize the overlay window
- **Hotkey Controls**: Quick keyboard shortcuts for control
- **Transparent Background**: Semi-transparent overlay that doesn't block gameplay
- **Always On Top**: Stays visible over the game window

## Installation

### Prerequisites
- Windows 10/11
- .NET 6.0 Runtime
- Microsoft Edge WebView2 Runtime (usually pre-installed)

### Build from Source
```bash
git clone <repository-url>
cd overlay-tiktok-lol
dotnet build --configuration Release
dotnet run
```

### Download Release
Download the latest release from the releases page and run `LeagueTikTokOverlay.exe`.

## Usage

### Starting the Overlay
1. Run `LeagueTikTokOverlay.exe`
2. Start League of Legends
3. The overlay will automatically appear in-game

### Controls

#### Hotkeys
- `Ctrl + H`: Hide/Show overlay
- `Ctrl + R`: Toggle between small and large size
- `Ctrl + M`: Enable drag mode to move the overlay

#### On-Screen Controls
- **Load Video**: Enter a TikTok URL and click to load
- **Hide/Show**: Toggle overlay visibility
- **Move**: Enable dragging to reposition
- **Resize**: Toggle between overlay sizes

### Loading TikTok Videos

1. **Direct URL**: Paste any TikTok video URL in the input field
   - Example: `https://www.tiktok.com/@username/video/1234567890`
2. **Auto-rotation**: The overlay automatically cycles through popular content
3. **Manual Control**: Use the interface to load specific videos

## Technical Details

### Architecture
- **Language**: C# with WinForms
- **Web Engine**: Microsoft Edge WebView2 for TikTok content
- **Overlay Method**: Windows API layered windows
- **League Detection**: Process and window name detection

### TikTok Integration
- Uses TikTok's official embed API
- Supports all public TikTok video content
- Respects TikTok's terms of service
- Maintains proper attribution and creator credits

### Performance
- Minimal impact on game performance
- Efficient window management
- Low memory footprint
- Hardware-accelerated video playback

## Customization

### Overlay Appearance
Edit the HTML/CSS in `LeagueOverlay.cs` to customize:
- Background transparency
- Border styling
- Button appearance
- Size and positioning

### TikTok Content
Modify the `popularVideos` array to set default content or implement custom feed logic.

## Safety and Security

### Game Integrity
- **No Game Files Modified**: Does not inject into or modify League of Legends
- **External Overlay**: Runs as separate process using Windows API
- **Read-Only**: Cannot interact with or modify game state
- **Riot Compliant**: Uses approved overlay techniques

### Privacy
- No data collection or transmission
- Local execution only
- Respects TikTok's privacy policies
- No account access required

## Troubleshooting

### Common Issues

**Overlay doesn't appear**
- Ensure League of Legends is running in windowed or borderless mode
- Check that WebView2 runtime is installed
- Run as administrator if needed

**TikTok videos won't load**
- Check internet connection
- Verify TikTok URL format
- Some videos may be region-restricted

**Performance issues**
- Reduce overlay size
- Close other overlays/streaming software
- Update graphics drivers

### System Requirements
- Windows 10 version 1903+ or Windows 11
- 4GB RAM minimum
- DirectX 11 compatible graphics
- Active internet connection for TikTok content

## Legal and Compliance

This software:
- Complies with Riot Games' Third-Party Application policies
- Uses only public TikTok APIs and embed methods
- Does not violate League of Legends Terms of Service
- Maintains proper attribution for all content

## Support

For issues, feature requests, or contributions, please visit the project repository.

## License

[Specify your license here]

---

**Disclaimer**: This is an unofficial third-party application. Not affiliated with Riot Games or TikTok.