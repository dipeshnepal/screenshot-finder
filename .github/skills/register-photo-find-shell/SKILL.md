---
name: register-photo-find-shell
description: "Use when registering or testing the Photo Find SharpShell Explorer context-menu extension after a Release build."
user-invocable: true
---

# Register Photo Find Shell Extension

Use this workflow only after building the Release configuration. The project is Windows-only, targets `.NET Framework 4.8`, and is compiled for AnyCPU. Use the x64 ServerManager option on 64-bit Windows.

## Release DLL

From the project root, run:

```powershell
dotnet build -c Release
```

The server DLL is:

```text
bin\Release\net48\PhotoFindExtensions.dll
```

## Install And Register SharpShell

The project restores the `SharpShell` NuGet package automatically. The separate SharpShell **ServerManager** tool is required to register the COM server with Explorer. Download it from the SharpShell GitHub releases page:

https://github.com/dwmkerr/sharpshell/releases

Start `ServerManager.exe` as administrator, choose the x64 option, add the Release DLL, and register or install the server. Use the exact DLL that was built above.

SharpShell ServerManager is the preferred registration tool for this project. Do not register a .NET 8 COM host; ServerManager expects the classic .NET Framework SharpShell assembly.

## Reload Explorer

After registration, restart Explorer:

```powershell
Stop-Process -Name explorer -Force
Start-Process explorer.exe
```

Test both locations:

1. Right-click an empty area inside a folder.
2. Right-click a directory.
3. Choose **Photo Find -> Find Screenshots**.

The command should report the number of matching screenshots in the selected folder.

Registration changes Windows Explorer and requires administrator approval. Do not run registration automatically without the user's confirmation.