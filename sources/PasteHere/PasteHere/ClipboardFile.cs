using System.Drawing.Imaging;

namespace PasteHere;

/// <summary>
/// Turns whatever is on the clipboard into a file. Images win over text when both
/// are present, which is what you want when copying out of a browser or Word.
/// </summary>
internal static class ClipboardFile
{
    /// <summary>
    /// Writes the clipboard content into <paramref name="folder"/> and returns the new
    /// file's path, or null when the clipboard holds nothing this tool can save.
    /// </summary>
    public static string? Write(string folder)
    {
        // ContainsImage can say yes and GetImage still hand back null for formats GDI+
        // cannot decode, so the text branch stays reachable in that case.
        if (Clipboard.ContainsImage() && Clipboard.GetImage() is { } image)
        {
            using (image)
            {
                var path = FreshPath(folder, ".png");
                image.Save(path, ImageFormat.Png);
                return path;
            }
        }

        if (Clipboard.ContainsText())
        {
            var path = FreshPath(folder, ".txt");
            File.WriteAllText(path, Clipboard.GetText());
            return path;
        }

        return null;
    }

    /// <summary>
    /// A timestamped name that still reads fine if the user backs out of the rename.
    /// </summary>
    private static string FreshPath(string folder, string extension)
    {
        var stem = $"Pasted {DateTime.Now:yyyy-MM-dd HH-mm-ss}";
        var path = Path.Combine(folder, stem + extension);
        for (var n = 2; File.Exists(path); n++) path = Path.Combine(folder, $"{stem} ({n}){extension}");
        return path;
    }
}
