# PasteHere
A tool that helps you paste an image or text from your clipboard as a file

![demo gif](https://github.com/tomzorz/PasteHere/raw/master/demo.gif)

Built on .NET 10 with a few WinForms APIs, some shell COM and a sprinkle of p/invoke.

## Usage

1. Unpack the release somewhere it can stay. The context-menu entry points at that location, so a folder you will not move later.
2. Run `PasteHere.exe --register` once. This adds "Paste clipboard as file" to the menu you get when right-clicking the empty space inside a folder. It writes to your own user's registry, so no admin rights are needed.
3. Copy something, right-click inside a folder, pick the entry. The new file shows up selected and in rename mode, in the same Explorer window.

`PasteHere.exe --unregister` removes the entry again. If you move the exe, run `--register` from the new location.

Text becomes a .txt file, images become .png. When the clipboard holds both, the image wins.

The exe needs the .NET 10 Desktop Runtime (x64). Windows offers the download if it is missing.

### The .reg file

`sources/reg file/pastehere.reg` does the same thing as `--register`, by hand: edit the path in it (keep the doubled backslashes) and double-click it. Only useful if you cannot run the exe with `--register` for some reason.

### Upgrading from v1

The v1 .reg file wrote a machine-wide entry under `HKEY_CLASSES_ROOT`. `--register` tells you when that entry is still around; remove it in regedit (admin rights needed) or the menu shows the item twice.

## Building

```
cd sources/PasteHere
dotnet publish -c Release
```

The single-file exe lands in `PasteHere/bin/Release/net10.0-windows/win-x64/publish/`.
