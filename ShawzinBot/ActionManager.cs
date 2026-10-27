using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Melanchall.DryWetMidi.Core;
using Keyboard = InputManager.Keyboard;
using Timer = System.Threading.Timer;

namespace ShawzinBot {
    public class ActionManager {
        public static string[] WindowNames = { "Warframe", "星际战甲" };

        private static IntPtr _warframeWindow = IntPtr.Zero;

        // Dictionary of note IDs and a series of ints. In order: Scale, Fret, Key, Vibrato
        private static readonly Dictionary<int, int[]> ShawzinNotes = new Dictionary<int, int[]> {
            { 48, new[] { 0, 0, 1, 0 } }, // C3
            { 49, new[] { 0, 0, 2, 0 } }, // C#3
            { 50, new[] { 0, 0, 3, 0 } }, // D3
            { 51, new[] { 0, 1, 1, 0 } }, // D#3
            { 52, new[] { 0, 1, 2, 0 } }, // E3
            { 53, new[] { 0, 1, 3, 0 } }, // F3
            { 54, new[] { 0, 2, 1, 0 } }, // F#3
            { 55, new[] { 0, 2, 2, 0 } }, // G3
            { 56, new[] { 0, 2, 3, 0 } }, // G#3
            { 57, new[] { 0, 3, 1, 0 } }, // A3
            { 58, new[] { 0, 3, 2, 0 } }, // A#3
            { 59, new[] { 0, 3, 3, 0 } }, // B3
            { 60, new[] { 8, 1, 3, 0 } }, // C4
            { 61, new[] { 4, 2, 1, 0 } }, // C#4
            { 62, new[] { 8, 2, 1, 0 } }, // D4
            { 63, new[] { 1, 2, 2, 0 } }, // D#4
            { 64, new[] { 8, 2, 2, 0 } }, // E4
            { 65, new[] { 1, 2, 3, 0 } }, // F4
            { 66, new[] { 1, 3, 1, 0 } }, // F#4
            { 67, new[] { 8, 2, 3, 0 } }, // G4
            { 68, new[] { 6, 2, 3, 0 } }, // G#4
            { 69, new[] { 8, 3, 1, 0 } }, // A4
            { 70, new[] { 1, 3, 3, 0 } }, // A#4
            { 71, new[] { 4, 3, 2, 1 } }, // B4
            { 72, new[] { 4, 3, 2, 0 } }, // C5
            { 73, new[] { 4, 3, 3, 0 } }, // C#5
            { 74, new[] { 8, 3, 3, 0 } }, // D5
            { 75, new[] { 7, 3, 3, 0 } } // D#5
        };

        private static readonly Dictionary<int, Keys> ShawzinFrets = new Dictionary<int, Keys> {
            { 0, Keys.None }, // No Fret
            { 1, Keys.Left }, // Sky Fret
            { 2, Keys.Down }, // Earth Fret
            { 3, Keys.Right } // Water Fret
        };

        private static readonly Dictionary<int, Keys> ShawzinStrings = new Dictionary<int, Keys> {
            { 1, Keys.D1 }, // 1st String
            { 2, Keys.D2 }, // 2nd String
            { 3, Keys.D3 } // 3rd String
        };

        private static readonly Dictionary<int, Keys> ShawzinSpecial = new Dictionary<int, Keys> {
            { 0, Keys.Space }, // Vibrato
            { 1, Keys.Tab } // Scale change
        };

        private static readonly int ScaleSize = 9;

        public static int ActiveScale;

        private static Keys _fretKey;
        private static Keys _vibratoKey;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        public static IntPtr FindWindow(string lpWindowName) {
            return FindWindow(null, lpWindowName);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fUnknown);

        /// <summary>
        ///     Play a MIDI note inside Warframe.
        /// </summary>
        /// <param name="note"> The note to be played.</param>
        /// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        public static bool PlayNote(NoteOnEvent note, bool enableVibrato, bool transposeNotes) {
            if (WindowNames.Select(x => IsWindowFocused(x)).All(x => !x)) return false;
            var noteId = (int)note.NoteNumber;
            if (!ShawzinNotes.ContainsKey(noteId)) {
                if (transposeNotes) {
                    if (noteId < ShawzinNotes.Keys.First())
                        noteId = ShawzinNotes.Keys.First() + noteId % 12;
                    else if (noteId > ShawzinNotes.Keys.Last()) noteId = ShawzinNotes.Keys.Last() - 15 + noteId % 12;
                }
                else {
                    return false;
                }
            }

            PlayNote(noteId, enableVibrato, transposeNotes);
            return true;
        }

        /// <summary>
        ///     Play a MIDI note inside Warframe.
        /// </summary>
        /// <param name="noteId"> The MIDI ID of the note to be played.</param>
        /// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        public static void PlayNote(int noteId, bool enableVibrato, bool transposeNotes) {
            var shawzinNote = ShawzinNotes[noteId];
            SetScale(shawzinNote[0]);
            var stringKey = ShawzinStrings[shawzinNote[2]];

            Keyboard.KeyUp(_fretKey);
            _fretKey = ShawzinFrets[shawzinNote[1]];

            Keyboard.KeyUp(_vibratoKey);
            _vibratoKey = ShawzinSpecial[0];

            if (shawzinNote[3] == 1 && enableVibrato) {
                KeyHold(_vibratoKey, TimeSpan.FromMilliseconds(100));
                Keyboard.KeyDown(_vibratoKey);
            }

            Keyboard.KeyDown(_fretKey);
            KeyTap(stringKey);
        }

        public static void SetScale(int scaleIndex) {
            var scaleDifference = 0;

            if (scaleIndex < ActiveScale)
                scaleDifference = ScaleSize - (ActiveScale - scaleIndex);
            else if (scaleIndex > ActiveScale) scaleDifference = scaleIndex - ActiveScale;

            for (var i = 0; i < scaleDifference; i++) KeyTap(ShawzinSpecial[1]);

            ActiveScale = scaleIndex;
        }

        /// <summary>
        ///     Tap a key.
        /// </summary>
        /// <param name="key"> The key to be tapped.</param>
        public static void KeyTap(Keys key) {
            Keyboard.KeyDown(key);
            Keyboard.KeyUp(key);
        }

        /// <summary>
        ///     Hold key for certain amount of time and release.
        /// </summary>
        /// <param name="key"> The key to be held.</param>
        /// <param name="time"> The amount of time the key should be held for.</param>
        public static void KeyHold(Keys key, TimeSpan time) {
            Keyboard.KeyDown(key);
            new Timer(state => Keyboard.KeyUp(key), null, time, Timeout.InfiniteTimeSpan);
        }

        public static bool OnSongPlay() {
            if (WindowNames.Select(x => FindWindow(x))
                .All(x => (_warframeWindow = x).Equals(IntPtr.Zero))) return false;
            SwitchToThisWindow(_warframeWindow, true);
            return IsWindowFocused(_warframeWindow);
        }

        public static bool IsWindowFocused(IntPtr windowPtr) {
            return !windowPtr.Equals(IntPtr.Zero) && GetForegroundWindow().Equals(windowPtr);
        }

        public static bool IsWindowFocused(string windowName) {
            return IsWindowFocused(FindWindow(windowName));
        }
    }
}