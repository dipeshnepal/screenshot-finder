using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace PhotoFindExtensions;

[ComVisible(true)]
[Guid("7E4F6A8B-5D4A-4F58-9F54-9D0F35B78A21")]
[COMServerAssociation(AssociationType.Directory)]
[COMServerAssociation(AssociationType.DirectoryBackground)]
public class PhotoFindMenu : SharpContextMenu
{
    private static readonly object LogLock = new();
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PhotoFind",
        $"LOG_{DateTime.Now:MM_dd_yy_HH_mm_ss}.log");

    protected override bool CanShowMenu() => true;

    protected override ContextMenuStrip CreateMenu()
    {
        var mainMenu = new ContextMenuStrip();
        var photoFindItem = new ToolStripMenuItem("Photo Find");
        var screenshotItem = new ToolStripMenuItem("Find Screenshots");

        screenshotItem.Click += (_, _) => FindScreenshotsInFolder();
        photoFindItem.DropDownItems.Add(screenshotItem);
        mainMenu.Items.Add(photoFindItem);

        return mainMenu;
    }

    private void FindScreenshotsInFolder()
    {
        WriteLog($"DLL: {typeof(PhotoFindMenu).Assembly.Location}");

        var targetFolder = SelectedItemPaths
            .FirstOrDefault(path => Directory.Exists(path));

        if (string.IsNullOrEmpty(targetFolder))
        {
            targetFolder = FolderPath;
        }

        if (string.IsNullOrEmpty(targetFolder) || !Directory.Exists(targetFolder))
        {
            WriteLog("No valid scan folder was found.");
            return;
        }

        WriteLog($"Scan folder: {targetFolder}");
        var results = new ImageAnalyzer().ProcessDirectory(targetFolder);
        WriteLog($"Matching screenshots: {results.Count}");

        foreach (var result in results)
        {
            WriteLog($"Match: {result}");
        }

        ShowMatchesInExplorer(results);

        MessageBox.Show(
            $"Analysis complete! Found {results.Count} matching screenshot{(results.Count == 1 ? "" : "s")}.",
            "Photo Find",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static void WriteLog(string message)
    {
        try
        {
            lock (LogLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
                File.AppendAllText(
                    LogFilePath,
                    $"{DateTime.Now:O} {message}{Environment.NewLine}");
            }
        }
        catch
        {
        }
    }

    private static void ShowMatchesInExplorer(IReadOnlyList<string> results)
    {
        if (results.Count == 0)
        {
            return;
        }

        var query = string.Join(" OR ", results.Select(path =>
            $"System.ItemPathDisplay:\"{path.Replace("\"", "\\\"")}\""));
        var searchUri = $"search-ms:displayname=Photo%20Find%20Matches&query={Uri.EscapeDataString(query)}";

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{searchUri}\"",
            UseShellExecute = true
        });
    }
}