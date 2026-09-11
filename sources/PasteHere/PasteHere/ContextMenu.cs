using Microsoft.Win32;

namespace PasteHere;

/// <summary>
/// The Explorer folder-background context-menu entry. Lives under HKCU so no admin
/// rights are needed, and the command always points at the exe that registered it,
/// which ends the era of people hand-editing backslashes in a .reg file.
/// </summary>
internal static class ContextMenu
{
    public const string Label = "Paste clipboard as file";

    private const string KeyPath = @"Software\Classes\Directory\Background\shell\PasteHere";

    public static void Register()
    {
        var exe = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot determine the path of the running executable.");
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.SetValue("", Label);
        using var command = key.CreateSubKey("command");
        command.SetValue("", $"\"{exe}\" \"%V\"");
    }

    public static void Unregister() => Registry.CurrentUser.DeleteSubKeyTree(KeyPath, throwOnMissingSubKey: false);

    /// <summary>
    /// True when the old .reg file's entry is present too. HKEY_CLASSES_ROOT writes
    /// land in HKLM, which needs admin rights to clean up, so we can only point at it.
    /// </summary>
    public static bool MachineWideEntryExists
    {
        get
        {
            using var key = Registry.LocalMachine.OpenSubKey(KeyPath);
            return key is not null;
        }
    }
}
