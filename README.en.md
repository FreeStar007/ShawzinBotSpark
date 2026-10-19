[中文](README.md) | [English](README.en.md) | [日本語](README.ja.md)

![**ShawzinBot** Logo](https://github.com/FreeStar007/ShawzinBotSpark/blob/main/ShawzinBot/Resources/Shawzin.png)

**ShawzinBot** is a small tool that turns **MIDI** input or files into a series of key presses to play the **Shawzin**. Any **MIDI** input works (**MIDI** keyboards, virtual **MIDI** channels, and so on), and standard **MIDI** files are supported too.

# Download
[Download page for the latest version of **ShawzinBot**](https://github.com/FreeStar007/ShawzinBotSpark/releases/latest)

# About

### What is ShawzinBot?
**ShawzinBot** is a third-party application that reads **MIDI** files (.mid) and plays their contents in *Warframe*. It works by simulating key presses to play the **Shawzin**.

### Is it safe?
**ShawzinBot** never interferes with the game — it injects no new code, modifies no existing code, and never touches memory. So it doesn't really qualify as a cheat at all. Feel free to use it.

### How does it work?
**ShawzinBot** is built on the **C#** [DryWetMIDI **MIDI** library (by melanchall)](https://www.midi.org/specifications/category/smf-specifications). This library handles loading and playing **MIDI** files; every note event is intercepted by **ShawzinBot**, which presses different key combinations based on the note ID to produce sound. It also supports external **MIDI** devices (keyboards, synthesizers, and the like).

# Getting Started
It's quite simple to use. As long as you keep the components together, it can sit anywhere on your PC — no installation is needed. Just double-click **ShawzinBot.exe** to run it (note that the **ShawzinBot.exe.config** file must not be deleted).

## Usage
Once the program is running, you'll see a window like this.

![**ShawzinBot** Overview](https://github.com/FreeStar007/ShawzinBotSpark/blob/main/ShawzinBot/Resources/app.png)

There's a button in the top-left corner; click it to open and load a **MIDI** file. After the file is loaded, click play and switch back to the game. **ShawzinBot** only touches your keyboard when you've switched back to *Warframe* — it won't press keys on its own otherwise.

To use an external **MIDI** device, just pick it from the dropdown menu and start playing. If you don't see your device, click the refresh button to the right of the dropdown to reload the list.

By default, the program selects every available **MIDI** track. To switch to other tracks or pick multiple ones, just tick the boxes under **MIDI Tracks (Track)**. It's very simple.

There are currently **2** adjustable settings:
* **Vibrato** - a tremolo-like effect when playing in-game, off by default
* **Transpose** - automatically transposes notes that can't be played, on by default

# Special Thanks
* [@lilggamegenius](https://github.com/lilggamegenius) - for help working out the in-game key mappings
* [/u/T2k5](https://www.reddit.com/user/T2k5/) - for writing the original complete keyboard script

# Notes
* Pressing multiple keys at once causes problems. This is a limitation of the **Shawzin** itself; slightly offsetting the notes solves it.

# News
This is a new project I started based on the MIT license of the original repository, and it keeps the original project's license.
Original project: https://github.com/ianespana/ShawzinBot
Thanks to [@ianespana](https://github.com/ianespana) for the original project.