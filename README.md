# Photo Find Explorer Extension

This project builds a Windows Explorer context-menu shell extension that scans a folder for likely screenshots.

## Build

```powershell
dotnet restore
dotnet build -c Release
```

## Install SharpShell

SharpShell has two parts:

1. The `SharpShell` NuGet package is referenced by this project and is restored automatically by `dotnet restore`.
2. The SharpShell **ServerManager** desktop tool is used to register the compiled DLL with Windows Explorer. Download the matching ServerManager release from the [SharpShell GitHub releases](https://github.com/dwmkerr/sharpshell/releases) page.

The current project targets `.NET Framework 4.8` and builds for `AnyCPU`, matching the SharpShell server model. Register it with the x64 ServerManager option on 64-bit Windows.

## Register And Test

1. Build the Release configuration:

	```powershell
	dotnet build -c Release
	```

2. Start `ServerManager.exe` as administrator.
3. Add or register this file:

	```text
	bin\Release\net48\PhotoFindExtensions.dll
	```

4. Restart Explorer so it reloads the COM shell extension:

	```powershell
	Stop-Process -Name explorer -Force
	Start-Process explorer.exe
	```

5. Right-click an empty area inside a folder, or right-click a directory. Select **Photo Find -> Find Screenshots**.

After scanning, the extension opens an Explorer search-results view containing only the matching screenshot files. Explorer displays them using its normal view, so enable thumbnail view in Explorer to see image previews.

When a directory itself is selected, that directory is used as the scan root. When the command is run from empty space inside a folder, the current folder is used. Scanning includes subdirectories.

The registration and unregistration workflows are also available as workspace skills in `.github/skills/`.

## Unregister

Use SharpShell ServerManager as administrator to unregister the same DLL before moving or deleting it. Restart Explorer again with the commands above. The [unregister-photo-find-shell skill](.github/skills/unregister-photo-find-shell/SKILL.md) contains the full cleanup workflow.

SharpShell ServerManager is the preferred registration tool for this project. `regasm.exe` is not the normal SharpShell registration workflow.