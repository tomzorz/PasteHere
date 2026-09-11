namespace PasteHere;

internal static class Program
{
    private const string Title = "PasteHere";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            return args switch
            {
                ["--register"] => Register(),
                ["--unregister"] => Unregister(),
                [] => Paste(Environment.CurrentDirectory),
                [var folder] when Directory.Exists(folder) => Paste(folder),
                _ => Usage(),
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static int Paste(string folder)
    {
        var file = ClipboardFile.Write(folder);
        if (file is null)
        {
            MessageBox.Show("The clipboard holds neither text nor an image.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 1;
        }

        ExplorerSelection.SelectForRename(file);
        return 0;
    }

    private static int Register()
    {
        ContextMenu.Register();
        var message = $"\"{ContextMenu.Label}\" now shows up when you right-click the empty space inside a folder.";
        if (ContextMenu.MachineWideEntryExists)
        {
            message += "\n\nA machine-wide entry from the old .reg file exists as well, so the menu will show the item twice. "
                     + "Remove HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\PasteHere in regedit (needs admin rights) to get rid of the duplicate.";
        }

        MessageBox.Show(message, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    private static int Unregister()
    {
        ContextMenu.Unregister();
        MessageBox.Show("The context-menu entry is gone.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    private static int Usage()
    {
        MessageBox.Show(
            "PasteHere [folder]      write the clipboard into the folder (default: the current one)\n"
            + "PasteHere --register    add the Explorer context-menu entry for the current user\n"
            + "PasteHere --unregister  remove it again",
            Title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return 2;
    }
}
