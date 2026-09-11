using System.Runtime.InteropServices;

namespace PasteHere;

/// <summary>
/// The shell32 and user32 surface this tool needs: item ID lists, selecting an
/// item in a folder window, and telling the shell a file just appeared.
/// </summary>
internal static partial class Native
{
    // SHOpenFolderAndSelectItems flags
    public const uint OfasiEdit = 0x0001;
    public const uint OfasiOpenDesktop = 0x0002;

    // IShellFolderViewDual.SelectItem flags
    public const int SvsiEdit = 0x0003;
    public const int SvsiDeselectOthers = 0x0004;
    public const int SvsiEnsureVisible = 0x0008;
    public const int SvsiFocused = 0x0010;

    // SHChangeNotify
    public const int ShcneCreate = 0x00000002;
    public const uint ShcnfPathW = 0x0005;
    public const uint ShcnfFlush = 0x1000;

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr ILCreateFromPathW(string path);

    [LibraryImport("shell32.dll")]
    public static partial void ILFree(IntPtr pidl);

    [LibraryImport("shell32.dll")]
    public static partial int SHOpenFolderAndSelectItems(IntPtr folder, uint count, in IntPtr items, uint flags);

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial void SHChangeNotify(int eventId, uint flags, string item1, IntPtr item2);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetForegroundWindow();
}
