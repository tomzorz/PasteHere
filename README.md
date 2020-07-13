# PasteHere
A tool that helps you paste an image or text from your clipboard as a file

[demo gif](https://github.com/tomzorz/PasteHere/raw/master/demo.gif)

Built using .net core, a few WinForms APIs and an extra sprinkle of p/invoke shell commands. 

## Usage

1. Unpack the release somewhere
2. Edit the .reg file using your editor of choice, and change the binary path to match wherever you extracted it
3. Double-click the .reg file to add it to your registry
4. If you did everything well, right clicking an empty space inside the folder should have the new option

Right now the tool outputs .txt and .png files for text and image content respectively.