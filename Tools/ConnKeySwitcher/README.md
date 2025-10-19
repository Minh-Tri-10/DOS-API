# Connection Key Switcher (WinForms)

- Purpose: change every GetConnectionString("...") in all Program.cs files to a selected key.
- Default keys:
  - SqlServer, DefaultConnection, LocConnection, HuyConnection, TriConnection, WeiConnection

How to build
- dotnet build Tools/ConnKeySwitcher/ConnKeySwitcher.csproj -c Release

Run
- Double-click the generated ConnKeySwitcher.exe under Tools/ConnKeySwitcher/bin/Release/net8.0-windows/.
- Choose repo root (defaults to detected solution root) and the desired key.
- Click Scan to preview, Apply to replace. A .bak backup is created next to each modified Program.cs.

Notes
- Skips in/ and obj/ directories.
- Replacement uses regex: GetConnectionString("...") ? GetConnectionString("<chosen>").
- Encoding is UTF-8 without BOM.
