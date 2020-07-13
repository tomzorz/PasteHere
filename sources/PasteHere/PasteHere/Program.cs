using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace PasteHere
{
    class Program
    {
        // thanks to https://superuser.com/questions/445925/how-to-add-item-to-right-click-menu-when-not-selecting-a-folder-or-file for the regedit

        [STAThread()]
        static void Main(string[] args)
        {
            var fp = Path.Combine(Environment.CurrentDirectory, $"_PH_{Guid.NewGuid()}");

            if (Clipboard.ContainsImage())
            {
                fp += ".png";
                var image = Clipboard.GetImage();
                using var fs = File.OpenWrite(fp);
                Debug.Assert(image != null, nameof(image) + " != null");
                image.Save(fs, ImageFormat.Png);

            } else if (Clipboard.ContainsText())
            {
                fp += ".txt";
                var text = Clipboard.GetText();
                File.WriteAllText(fp, text);
            }

            SelectItemInExplorer(fp, true);
        }

        // thanks to https://stackoverflow.com/questions/8647447/send-folder-rename-command-to-windows-explorer
        // and https://stackoverflow.com/questions/3010305/programmatically-selecting-file-in-explorer

        public static void SelectItemInExplorer(string itemPath, bool edit)
        {
            if (itemPath == null)
                throw new ArgumentNullException("itemPath");

            var pidl = ILCreateFromPathW(itemPath);
            IntPtr folder = PathToAbsolutePIDL(pidl, Path.GetDirectoryName(itemPath));
            IntPtr file = PathToAbsolutePIDL(pidl, itemPath);
            try
            {
                SHOpenFolderAndSelectItems(folder, 1, new[] { file }, edit ? 1 : 0);
            }
            finally
            {
                ILFree(folder);
                ILFree(file);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr[] apidl, int dwFlags);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll")]
        private static extern int SHGetDesktopFolder(out IShellFolder ppshf);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [ComImport, Guid("000214E6-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellFolder
        {
            void ParseDisplayName(IntPtr hwnd, IBindCtx pbc, [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
            // NOTE: we declared only what we needed...
        }

        private static IntPtr GetShellFolderChildrenRelativePIDL(IntPtr hwnd, IShellFolder parentFolder, string displayName)
        {
            IBindCtx bindCtx;
            CreateBindCtx(0, out bindCtx);
            uint pchEaten;
            uint pdwAttributes = 0;
            IntPtr ppidl;
            parentFolder.ParseDisplayName(hwnd, bindCtx, displayName, out pchEaten, out ppidl, ref pdwAttributes);
            return ppidl;
        }

        private static IntPtr PathToAbsolutePIDL(IntPtr hwnd, string path)
        {
            IShellFolder desktopFolder;
            SHGetDesktopFolder(out desktopFolder);
            return GetShellFolderChildrenRelativePIDL(hwnd, desktopFolder, path);
        }
    }
}
