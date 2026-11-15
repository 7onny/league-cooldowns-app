# League Cooldowns

A compact C# / WPF Windows app that downloads champion ability cooldowns from League Wiki.

## Run

Open dist/LeagueCooldowns.exe. Keep its data folder beside it.
Windows 10/11 with .NET Framework 4.7.2 or later is required.

Type a champion name, then click a suggestion or press Enter. Arrow keys navigate suggestions and Escape dismisses them. Q, W, E, and R show base cooldowns in seconds by ability rank. Use each column's dropdown for alternate abilities and forms. Long names and cooldown timing notes appear on hover.

Refresh fetches the current wiki page. Wiki opens the champion source page. Keep on top pins the window above other windows.

Successful pages are saved under %LOCALAPPDATA%/LeagueCooldowns. If a request fails, the app shows the saved page and its timestamp. The champion roster ships with the app and refreshes on launch.

## Data interpretation

The app reads the main ability sections of the champion page. Recharge times and conditional or champion-level scaling values are displayed as text below the rank rows. A dash means a rank value is not available. "No listed cooldown" means no cooldown field was found for that variant, including passive abilities or variants whose cooldown is shared with another ability.

Values are before ability haste. The wiki's markup and special champion mechanics may change. Use the source link for detailed conditions. The full roster has not been individually validated.

Data source and attribution https://wiki.leagueoflegends.com/en-us/
League Wiki contributors provide the ability data. Refer to the source site's licensing terms for reuse.

## Build and checks

Run ./build.ps1 in PowerShell. It uses the .NET Framework C# compiler installed with Windows and produces dist/LeagueCooldowns.exe, with no NuGet dependencies.

Run ./test.ps1 for the downloaded sample pages, Ahri value assertions, and a WPF render check. The fixtures were fetched on 15 November 2026 and are retained locally for reproducible checks.
