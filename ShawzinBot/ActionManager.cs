using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Melanchall.DryWetMidi.Core;
using ShawzinBot.Languages;
using Keyboard = InputManager.Keyboard;
using Timer = System.Threading.Timer;

namespace ShawzinBot {
    // 按键动作管理器：把MIDI音符换算成三线琴的游戏按键并发送
    public static class ActionManager {
        // 需要操作的游戏窗口标题集合，取自多语言资源，覆盖所有语言下的标题
        public static string[] WindowNames = LanguageManager.GetStrings("GameWindowTitle");

        // 当前缓存的游戏窗口句柄，避免每次判断焦点都重复查找
        private static IntPtr _warframeWindow = IntPtr.Zero;

        // 音符ID到演奏方式的映射字典，数组四项依次为：音阶、品位、弦号、颤音标记
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

        // 品位编号到方向键的映射
        private static readonly Dictionary<int, Keys> ShawzinFrets = new Dictionary<int, Keys> {
            { 0, Keys.None }, // 空品，不按任何品位键
            { 1, Keys.Left }, // 天品
            { 2, Keys.Down }, // 地品
            { 3, Keys.Right } // 水品
        };

        // 弦号到数字键的映射
        private static readonly Dictionary<int, Keys> ShawzinStrings = new Dictionary<int, Keys> {
            { 1, Keys.D1 }, // 第一弦
            { 2, Keys.D2 }, // 第二弦
            { 3, Keys.D3 } // 第三弦
        };

        // 功能键映射，供颤音与音阶切换复用
        private static readonly Dictionary<int, Keys> ShawzinSpecial = new Dictionary<int, Keys> {
            { 0, Keys.Space }, // 颤音键
            { 1, Keys.Tab } // 音阶切换键
        };

        // 音阶总格数，音阶为循环切换，用于计算跨越零点的切换次数
        private static readonly int ScaleSize = 9;

        // 当前游戏内生效的音阶编号，由外部读取用于界面显示
        public static int ActiveScale;

        // 上一个音使用的品位键，弹新音之前先松开，避免按键粘滞
        private static Keys _fretKey;

        // 上一个音使用的颤音键，同样需要在换音时松开
        private static Keys _vibratoKey;

        // 查找窗口句柄的系统接口，按窗口类名与标题定位窗口
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        // 按窗口标题查找窗口句柄，不匹配窗口类名
        // lpWindowName：窗口标题
        // 返回：找到的窗口句柄，未找到时为空句柄
        public static IntPtr FindWindow(string lpWindowName) {
            return FindWindow(null, lpWindowName);
        }

        // 获取当前前台窗口句柄的系统接口
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        // 把指定窗口切换到前台的系统接口
        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fUnknown);

        // 播放一个MIDI音符事件，音域换算与移调判断都在这一层完成
        // note：待播放的音符事件；enableVibrato：是否启用颤音；transposeNotes：是否启用移调
        // 返回：false表示游戏窗口未获得焦点，或音符超出音域且未开启移调
        public static bool PlayNote(NoteOnEvent note, bool enableVibrato, bool transposeNotes) {
            // 游戏窗口不在前台时直接放弃，防止把按键发到别的程序里
            if (WindowNames.Select(x => IsWindowFocused(x)).All(x => !x)) return false;
            var noteId = (int)note.NoteNumber;
            if (!ShawzinNotes.ContainsKey(noteId)) {
                // 超出可演奏音域时，把音符折算到音域内的同音名位置
                if (transposeNotes) {
                    if (noteId < ShawzinNotes.Keys.First())
                        noteId = ShawzinNotes.Keys.First() + noteId % 12;
                    else if (noteId > ShawzinNotes.Keys.Last()) noteId = ShawzinNotes.Keys.Last() - 15 + noteId % 12;
                } else {
                    return false;
                }
            }

            PlayNote(noteId, enableVibrato);
            return true;
        }

        // 按音符ID演奏，依次完成切音阶、换品位、弹响琴弦
        // noteId：待演奏的MIDI音符ID，调用方保证其已在可演奏音域内；enableVibrato：是否启用颤音
        private static void PlayNote(int noteId, bool enableVibrato) {
            var shawzinNote = ShawzinNotes[noteId];
            SetScale(shawzinNote[0]);
            var stringKey = ShawzinStrings[shawzinNote[2]];

            // 先松开旧品位键，再记住本音使用的新品位键
            Keyboard.KeyUp(_fretKey);
            _fretKey = ShawzinFrets[shawzinNote[1]];

            // 颤音键同理，本音不用颤音时保持松开状态
            Keyboard.KeyUp(_vibratoKey);
            _vibratoKey = ShawzinSpecial[0];

            // 该音标记了需要颤音且用户开启了颤音，先短按一次并持续按住颤音键
            if (shawzinNote[3] == 1 && enableVibrato) {
                KeyHold(_vibratoKey, TimeSpan.FromMilliseconds(100));
                Keyboard.KeyDown(_vibratoKey);
            }

            Keyboard.KeyDown(_fretKey);
            KeyTap(stringKey);
        }

        // 切换游戏内音阶到指定编号
        // scaleIndex：目标音阶编号，音阶只能逐格前进，跨越零点时按循环总格数折算切换次数
        public static void SetScale(int scaleIndex) {
            var scaleDifference = 0;

            if (scaleIndex < ActiveScale)
                scaleDifference = ScaleSize - (ActiveScale - scaleIndex);
            else if (scaleIndex > ActiveScale) scaleDifference = scaleIndex - ActiveScale;

            // 相同音阶时次数为零，不触发任何按键
            for (var i = 0; i < scaleDifference; i++) KeyTap(ShawzinSpecial[1]);

            ActiveScale = scaleIndex;
        }

        // 点按一次指定按键，按下后立即松开
        // key：要点按的按键
        public static void KeyTap(Keys key) {
            Keyboard.KeyDown(key);
            Keyboard.KeyUp(key);
        }

        // 按住指定按键到设定时长后自动松开，松开动作由定时器完成，不阻塞当前线程
        // key：要按住的按键；time：按住时长
        public static void KeyHold(Keys key, TimeSpan time) {
            Keyboard.KeyDown(key);
            new Timer(state => Keyboard.KeyUp(key), null, time, Timeout.InfiniteTimeSpan);
        }

        // 曲目开始播放时把游戏窗口切到前台
        // 返回：窗口切换完成后是否已处于前台焦点，未找到游戏窗口时同样返回false
        public static bool OnSongPlay() {
            if (WindowNames.Select(x => FindWindow(x))
                .All(x => (_warframeWindow = x).Equals(IntPtr.Zero))) return false;
            SwitchToThisWindow(_warframeWindow, true);
            return IsWindowFocused(_warframeWindow);
        }

        // 判断指定窗口是否为当前前台窗口
        // windowPtr：待判断的窗口句柄
        public static bool IsWindowFocused(IntPtr windowPtr) {
            return !windowPtr.Equals(IntPtr.Zero) && GetForegroundWindow().Equals(windowPtr);
        }

        // 按窗口标题判断该窗口是否为当前前台窗口
        // windowName：窗口标题
        public static bool IsWindowFocused(string windowName) {
            return IsWindowFocused(FindWindow(windowName));
        }
    }
}