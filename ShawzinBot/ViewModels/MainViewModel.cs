using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using Caliburn.Micro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using ShawzinBot.Languages;
using ShawzinBot.Models;
using ShawzinBot.Properties;
using ShawzinBot.Views;
using InputDevice = Melanchall.DryWetMidi.Devices.InputDevice;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;
using Timer = System.Timers.Timer;

namespace ShawzinBot.ViewModels {
    // 主界面视图模型：负责MIDI文件的加载、轨道管理、播放控制以及按键输出调度
    public class MainViewModel : Screen {
        #region Constructor

        public MainViewModel() {
            // 从用户配置恢复上次的开关与历史曲目
            EnableVibrato = Settings.Default.EnableVibrato;
            TransposeNotes = Settings.Default.TransposeNotes;
            HistoryMusicPath = Settings.Default.HistoryMusicPath;
            VersionString = _programVersion.ToString();
            if (HistoryMusicPath != "" && File.Exists(HistoryMusicPath)) SetUpMidi(HistoryMusicPath);

            // 第一项为“无设备”，其后枚举系统内全部MIDI输入设备
            MidiInputs.Add(new MidiInputModel(LanguageManager.GetString("DeviceNone"), true));

            foreach (var device in InputDevice.GetAll()) MidiInputs.Add(new MidiInputModel(device.Name));

            SelectedMidiInput = MidiInputs[0];
            // 可选的播放倍速列表，默认选中1倍速
            foreach (var speed in new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2" })
                MidiSpeeds.Add(new MidiSpeedModel(speed, double.Parse(speed)));

            SelectedMidiSpeed = MidiSpeeds[3];

            UpdateScale(0);

            LanguageManager.LanguageChanged += OnLanguageChanged;

            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromSeconds(1);
        }

        // 语言切换后刷新设备名称与音阶显示（这两项文案来自多语言资源）
        private void OnLanguageChanged(object sender, EventArgs e) {
            RefreshDevices();
            UpdateScale(ActionManager.ActiveScale);
        }

        #endregion

        #region Private Variables

        // 歌曲名、进度条值、进度条最大值
        private string _songName;
        private double _songSlider;

        private double _maximumTime = 1;

        // 当前时间与总时长的显示文本
        private string _currentTime = "0:00";

        private string _totalTime = "0:00";

        // 播放/暂停按钮显示的图标名称
        private string _playPauseIcon = "Play";

        // 当前音阶的显示文本
        private string _scale;

        private BindableCollection<MidiInputModel> _midiInputs = new BindableCollection<MidiInputModel>();
        private BindableCollection<MidiTrackModel> _midiTracks = new BindableCollection<MidiTrackModel>();
        private BindableCollection<MidiSpeedModel> _midiSpeeds = new BindableCollection<MidiSpeedModel>();
        private MidiInputModel _selectedMidiInput;
        private MidiSpeedModel _selectedMidiSpeed;

        private bool _enableVibrato = true;
        private bool _transposeNotes = true;

        private string _historyMusicPath = "";

        // 为true时忽略进度条变更，避免程序回写进度条时触发跳转
        private bool _ignoreSliderChange;

        // 九个音阶名称对应的多语言资源键
        private static readonly string[] ScaleNameKeys = {
            "ScaleName0",
            "ScaleName1",
            "ScaleName2",
            "ScaleName3",
            "ScaleName4",
            "ScaleName5",
            "ScaleName6",
            "ScaleName7",
            "ScaleName8"
        };

        private IEnumerable<TrackChunk> _midiTrackChunks;

        // 首轨道（通常是速度/调号信息轨），播放时单独保留
        private TrackChunk _firstTrack;

        // 等待游戏窗口获得焦点的轮询定时器
        private Timer _playTimer;

        // 暂停时记录的播放位置，恢复播放时从该位置继续
        private ITimeSpan _playTime = new MidiTimeSpan();

        // 程序版本号，取自程序集
        private readonly Version _programVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private string _versionString = "";

        #endregion

        #region Public Variables

        private MidiFile _midiFile;
        private Playback _playback;
        private InputDevice _inputDevice;

        // 轨道勾选状态变化后需要重建播放器的标记
        public static bool ReloadPlayback;

        #endregion

        #region Properties

        public string VersionString {
            get => _versionString;
            set {
                _versionString = "v" + value;
                NotifyOfPropertyChange(() => VersionString);
            }
        }

        public string SongName {
            get => _songName;
            set {
                _songName = value;
                NotifyOfPropertyChange(() => SongName);
            }
        }

        public string CurrentTime {
            get => _currentTime;
            set {
                _currentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        public string TotalTime {
            get => _totalTime;
            set {
                _totalTime = value;
                NotifyOfPropertyChange(() => TotalTime);
            }
        }

        public double SongSlider {
            get => _songSlider;
            set {
                _songSlider = value;
                NotifyOfPropertyChange(() => SongSlider);
                // 用户手动拖动进度条时，先停止播放再跳转到目标位置
                if (!_ignoreSliderChange && _playback != null) {
                    if (_playback.IsRunning) {
                        _playback.Stop();
                        PlayPauseIcon = "Play";
                    }

                    var time = TimeSpan.FromSeconds(_songSlider);

                    CurrentTime = time.ToString("m\\:ss");
                    _playback.MoveToTime((MetricTimeSpan)time);
                }

                _ignoreSliderChange = false;
            }
        }

        public double MaximumTime {
            get => _maximumTime;
            set {
                _maximumTime = value;
                NotifyOfPropertyChange(() => MaximumTime);
            }
        }

        public string PlayPauseIcon {
            get => _playPauseIcon;
            set {
                _playPauseIcon = value;
                NotifyOfPropertyChange(() => PlayPauseIcon);
            }
        }

        public BindableCollection<MidiInputModel> MidiInputs {
            get => _midiInputs;
            set {
                _midiInputs = value;
                NotifyOfPropertyChange(() => MidiInputs);
            }
        }

        public MidiInputModel SelectedMidiInput {
            get => _selectedMidiInput;
            set {
                _selectedMidiInput = value;
                _inputDevice?.Dispose();

                // 选中真实设备时打开监听，把外部MIDI输入实时转成游戏按键
                if (value != null && !value.IsNone) {
                    _inputDevice = InputDevice.GetByName(value.DeviceName);
                    _inputDevice.EventReceived += OnNoteEvent;
                    _inputDevice.StartEventsListening();
                    ActionManager.OnSongPlay();
                }

                NotifyOfPropertyChange(() => SelectedMidiInput);
            }
        }

        public BindableCollection<MidiSpeedModel> MidiSpeeds {
            get => _midiSpeeds;
            set {
                _midiSpeeds = value;
                NotifyOfPropertyChange(() => MidiSpeeds);
            }
        }

        public MidiSpeedModel SelectedMidiSpeed {
            get => _selectedMidiSpeed;
            set {
                _selectedMidiSpeed = value;
                NotifyOfPropertyChange(() => SelectedMidiSpeed);

                // 倍速变化时同步到当前播放器
                if (value?.Speed != null && _playback != null) _playback.Speed = value.Speed;
            }
        }

        public BindableCollection<MidiTrackModel> MidiTracks {
            get => _midiTracks;
            set {
                _midiTracks = value;
                NotifyOfPropertyChange(() => MidiTracks);
            }
        }

        public bool EnableVibrato {
            get => _enableVibrato;
            set {
                _enableVibrato = value;
                SetUpProperties("EnableVibrato", value);
            }
        }

        public bool TransposeNotes {
            get => _transposeNotes;
            set {
                _transposeNotes = value;
                SetUpProperties("TransposeNotes", value);
            }
        }


        public string Scale {
            get => _scale;
            set {
                _scale = value;
                NotifyOfPropertyChange(() => Scale);
            }
        }

        private string HistoryMusicPath {
            get => _historyMusicPath;
            set {
                _historyMusicPath = value;
                SetUpProperties("HistoryMusicPath", value);
            }
        }

        #endregion

        #region Methods

        // 把字符串型配置写入用户设置并持久化
        private void SetUpProperties(string property, string target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
            NotifyOfPropertyChange(() => property);
        }

        // 把布尔型配置写入用户设置并持久化
        private void SetUpProperties(string property, bool target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
            NotifyOfPropertyChange(() => property);
        }

        // 读取MIDI文件并初始化歌曲信息、轨道列表与进度条
        private void SetUpMidi(string filePath) {
            ClearMidi();
            try {
                // 宽松解析策略：忽略非法参数与残缺数据，避免因个别异常事件导致整个文件读取失败
                _midiFile = MidiFile.Read(filePath, new ReadingSettings {
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                    InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits,
                    SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOff,
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte,
                    UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore,
                    MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore
                });
            } catch (Exception e) {
                ThemedMessageBox.Show(string.Format(LanguageManager.GetString("ErrorMessageFormat"), e),
                    LanguageManager.GetString("ErrorTitle"));
                ClearMidi();
                HistoryMusicPath = "";
                return;
            }

            SongName = Path.GetFileNameWithoutExtension(filePath);
            TimeSpan midiFileDuration = _midiFile.GetDuration<MetricTimeSpan>();
            TotalTime = midiFileDuration.ToString("m\\:ss");
            MaximumTime = midiFileDuration.TotalSeconds;
            UpdateSlider(0);
            CurrentTime = "0:00";
            _midiTrackChunks = _midiFile.GetTrackChunks();
            // 多轨道时首轨作为信息轨固定播放，其余轨道全部列出供勾选
            if (_midiTrackChunks.Count() > 1) {
                _firstTrack = _midiTrackChunks.FirstOrDefault();
                _midiFile.Chunks.Remove(_firstTrack);
                MidiTracks.Add(new MidiTrackModel(_firstTrack, true));
                foreach (var track in _midiFile.GetTrackChunks()) MidiTracks.Add(new MidiTrackModel(track, true));
            } else {
                _firstTrack = _midiTrackChunks.FirstOrDefault();
                _midiFile.Chunks.Remove(_firstTrack);
                MidiTracks.Add(new MidiTrackModel(_firstTrack, true));
            }
        }

        // 弹出文件选择框打开MIDI文件，仅允许 .mid/.midi 格式
        public void OpenFile() {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MIDI file|*.mid;*.midi;";
            if (openFileDialog.ShowDialog() != true) return;
            HistoryMusicPath = openFileDialog.FileName;
            SetUpMidi(openFileDialog.FileName);
        }

        // 释放播放器与文件对象，并把界面状态复位到初始值
        private void ClearMidi() {
            if (_playback != null) {
                _playback.Stop();
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(_playback);
                _playback.Dispose();
                _playback = null;
            }

            _midiFile = null;
            MidiTracks.Clear();

            PlayPauseIcon = "Play";
            SongName = "";
            TotalTime = "0:00";
            CurrentTime = "0:00";
            MaximumTime = 1;
        }

        // 播放/暂停切换：首次播放时按勾选的轨道重建播放器，并开始向游戏输出按键
        public void PlayPause() {
            if (_midiFile == null || MaximumTime == 0d) return;
            if (_playback == null || ReloadPlayback) {
                if (_playback != null) {
                    _playback.Stop();
                    _playTime = _playback.GetCurrentTime(TimeSpanType.Midi);
                    _playback.Dispose();
                    _playback = null;
                    PlayPauseIcon = "Play";
                }

                _midiFile.Chunks.Clear();
                _midiFile.Chunks.Add(_firstTrack);

                // 只把勾选中的轨道加入播放集合
                foreach (var trackModel in MidiTracks)
                    if (trackModel.IsChecked)
                        _midiFile.Chunks.Add(trackModel.Track);

                _playback = _midiFile.GetPlayback();
                _playback.Speed = SelectedMidiSpeed.Speed;
                _playback.MoveToTime(_playTime);
                _playback.Finished += (s, e) => { ResetMidi(); };

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(_playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnTick;
                PlaybackCurrentTimeWatcher.Instance.Start();

                _playback.EventPlayed += OnNoteEvent;
                ReloadPlayback = false;
            }

            if (_playback.IsRunning) {
                PlayPauseIcon = "Play";
                _playback.Stop();
            } else if (PlayPauseIcon == "Pause") {
                // 处于暂停等待状态时再次点击，取消定时器并回到停止状态
                PlayPauseIcon = "Play";
                _playTimer.Dispose();
            } else {
                PlayPauseIcon = "Pause";

                // 先把游戏窗口切到前台，未获得焦点时由定时器继续轮询等待
                ActionManager.OnSongPlay();
                _playTimer = new Timer();
                _playTimer.Interval = 100;
                _playTimer.Elapsed += PlayTimerElapsed;
                _playTimer.Start();
            }
        }

        // 轮询游戏窗口焦点，一旦获得焦点就真正开始播放
        private void PlayTimerElapsed(object sender, ElapsedEventArgs e) {
            if (ActionManager.WindowNames.Any(ActionManager.IsWindowFocused)) {
                _playback.Start();
                _playTimer.Dispose();
            }
        }

        // 把播放位置复位到开头并同步界面
        private void ResetMidi() {
            if (_playback != null) {
                _playback.MoveToStart();
                UpdateSlider(0);
                _playTime = new MidiTimeSpan();
                PlayPauseIcon = "Play";
                CurrentTime = "0:00";
            }
        }

        // 重新播放按钮命令
        public void Restart() {
            ResetMidi();
        }

        // 清除按钮命令：卸载曲目并清空历史路径
        public void Clear() {
            ClearMidi();
            HistoryMusicPath = "";
        }

        // 重新枚举MIDI输入设备，并把选中项复位为“无设备”
        public void RefreshDevices() {
            MidiInputs.Clear();
            MidiInputs.Add(new MidiInputModel(LanguageManager.GetString("DeviceNone"), true));

            foreach (var device in InputDevice.GetAll()) MidiInputs.Add(new MidiInputModel(device.Name));

            SelectedMidiInput = MidiInputs[0];
        }

        // 按音阶编号刷新界面显示的音阶名称
        private void UpdateScale(int scaleIndex) {
            if (scaleIndex < 0 || scaleIndex >= ScaleNameKeys.Length) return;
            var format = LanguageManager.GetString("ScaleFormat");
            var name = LanguageManager.GetString(ScaleNameKeys[scaleIndex]);
            Scale = string.Format(format, name);
        }

        #endregion

        #region EventHandlers

        // 播放进度定时器回调：同步进度条与当前时间显示
        private void OnTick(object sender, PlaybackCurrentTimeChangedEventArgs e) {
            foreach (var playbackTime in e.Times) {
                TimeSpan time = (MetricTimeSpan)playbackTime.Time;

                UpdateSlider(time.TotalSeconds);
                CurrentTime = time.ToString("m\\:ss");
            }
        }

        // 播放器音符事件回调：把音符转换为游戏按键；若游戏窗口已失焦则停止播放，避免音阶错乱
        private void OnNoteEvent(object sender, MidiEventPlayedEventArgs e) {
            switch (e.Event.EventType) {
                case MidiEventType.NoteOn:
                    var note = e.Event as NoteOnEvent;
                    if (note != null && note.Velocity <= 0) return;

                    if (!ActionManager.PlayNote(note, EnableVibrato, TransposeNotes))
                        PlayPause();
                    UpdateScale(ActionManager.ActiveScale);
                    return;
                default:
                    return;
            }
        }

        // 外部MIDI输入设备事件回调：实时把弹奏的音符转发到游戏
        private void OnNoteEvent(object sender, MidiEventReceivedEventArgs e) {
            if (e.Event.EventType != MidiEventType.NoteOn) return;

            var note = e.Event as NoteOnEvent;
            if (note != null && note.Velocity <= 0) return;

            ActionManager.PlayNote(note, EnableVibrato, TransposeNotes);
        }

        // 视图注销时解绑语言切换事件，避免泄漏
        protected override void OnDeactivate(bool close) {
            if (close) LanguageManager.LanguageChanged -= OnLanguageChanged;
            base.OnDeactivate(close);
        }

        // 由程序回写进度条，标记忽略本次变更以防止触发播放跳转
        private void UpdateSlider(double value) {
            _ignoreSliderChange = true;
            SongSlider = value;
        }

        #endregion
    }
}