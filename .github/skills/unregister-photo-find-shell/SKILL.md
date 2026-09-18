---
name: unregister-photo-find-shell
description: "Use when removing the Photo Find SharpShell Explorer context-menu extension and cleaning up its registration."
user-invocable: true
---

# Unregister Photo Find Shell Extension

Use the same Release DLL that was registered:

```text
bin\Release\net48\PhotoFindExtensions.dll
```

## Remove Registration

1. Start the SharpShell `ServerManager.exe` as administrator.
2. Select the registered `PhotoFindExtensions.dll` server.
3. Choose **Unregister**, **Remove**, or the equivalent ServerManager action.
4. Confirm that the x64 server and the exact DLL path are selected.

Do not delete or replace the DLL before unregistering it. If the DLL has already moved, restore it to the original path or use the ServerManager entry that contains its registered CLSID.

## Reload Explorer

Restart Explorer after unregistering:

```powershell
Stop-Process -Name explorer -Force
Start-Process explorer.exe
```

Verify that **Photo Find -> Find Screenshots** no longer appears in a directory or folder background context menu.

SharpShell ServerManager is the preferred cleanup tool. If ServerManager cannot find the old .NET 8 registration, remove that stale registration before registering the new `.NET Framework 4.8` assembly.

Unregistration changes Windows Explorer and requires administrator approval. Do not run it automatically without the user's confirmation.