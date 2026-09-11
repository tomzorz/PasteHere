using System.Runtime.InteropServices;

namespace PasteHere;

/// <summary>
/// Puts the freshly written file into rename mode in the Explorer window the user
/// clicked in. SHOpenFolderAndSelectItems on its own opens a second window whenever
/// the open one shows the folder through OneDrive, a library, Quick Access or the
/// desktop, so we look for that window ourselves and only fall back to the API.
/// </summary>
internal static class ExplorerSelection
{
    private const int SelectFlags = Native.SvsiEdit | Native.SvsiDeselectOthers | Native.SvsiEnsureVisible | Native.SvsiFocused;
    private const int Attempts = 20;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(50);

    public static void SelectForRename(string file)
    {
        // Tell the shell about the file right away instead of waiting for Explorer's
        // own directory watcher to notice it.
        Native.SHChangeNotify(Native.ShcneCreate, Native.ShcnfPathW | Native.ShcnfFlush, file, IntPtr.Zero);

        if (TrySelectInOpenWindow(file)) return;
        OpenFolderAndSelect(file);
    }

    private static bool TrySelectInOpenWindow(string file)
    {
        var window = FindWindowShowing(Path.GetDirectoryName(file)!);
        if (window is null) return false;

        var name = Path.GetFileName(file);
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            dynamic view = window.Document;
            dynamic? item = view.Folder.ParseName(name);
            if (item is not null)
            {
                try
                {
                    view.SelectItem(item, SelectFlags);
                    if (IsSelected(view, file)) return true;
                }
                catch (COMException)
                {
                    // The view has not picked the new file up yet; try again below.
                }
            }

            Thread.Sleep(RetryDelay);
        }

        return false;
    }

    private static bool IsSelected(dynamic view, string file)
    {
        dynamic selected = view.SelectedItems();
        return selected.Count == 1 && PathsEqual(selected.Item(0).Path as string, file);
    }

    private static dynamic? FindWindowShowing(string folder)
    {
        var shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return null;

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic windows = shell.Windows();
        var foreground = Native.GetForegroundWindow();
        dynamic? match = null;
        for (var i = 0; i < (int)windows.Count; i++)
        {
            dynamic window = windows.Item(i);
            if (window is null || !PathsEqual(FolderPathOf(window), folder)) continue;

            // The context menu came from the foreground window; prefer it when the
            // same folder is open more than once.
            if (HandleOf(window) == foreground) return window;
            match ??= window;
        }

        return match;
    }

    private static IntPtr HandleOf(dynamic window) => (IntPtr)(long)window.HWND;

    private static string? FolderPathOf(dynamic window)
    {
        try
        {
            return window.Document?.Folder?.Self?.Path as string;
        }
        catch (COMException)
        {
            // Not a folder view (an Internet Explorer window, or one that is closing).
            return null;
        }
    }

    private static void OpenFolderAndSelect(string file)
    {
        var folder = Path.GetDirectoryName(file)!;
        var onDesktop = PathsEqual(folder, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        var flags = Native.OfasiEdit | (onDesktop ? Native.OfasiOpenDesktop : 0);

        var folderPidl = Native.ILCreateFromPathW(folder);
        var filePidl = Native.ILCreateFromPathW(file);
        try
        {
            if (folderPidl == IntPtr.Zero || filePidl == IntPtr.Zero) throw new FileNotFoundException("The shell cannot resolve the new file.", file);
            Marshal.ThrowExceptionForHR(Native.SHOpenFolderAndSelectItems(folderPidl, 1, in filePidl, flags));
        }
        finally
        {
            Native.ILFree(folderPidl);
            Native.ILFree(filePidl);
        }
    }

    private static bool PathsEqual(string? a, string b) =>
        a is not null
        && string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(a)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)),
            StringComparison.OrdinalIgnoreCase);
}
