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
using Microsoft.Win32;
using ShawzinBot.Models;
using ShawzinBot.Properties;
using InputDevice = Melanchall.DryWetMidi.Devices.InputDevice;

namespace ShawzinBot.ViewModels {
    public class MainViewModel : Screen {
        #region Constructor

        public MainViewModel() {
            EnableVibrato = Settings.Default.EnableVibrato;
            TransposeNotes = Settings.Default.TransposeNotes;
            HistoryMusicPath = Settings.Default.HistoryMusicPath;
            VersionString = _programVersion.ToString();
            if (HistoryMusicPath != "" && File.Exists(HistoryMusicPath)) SetUpMidi(HistoryMusicPath);

            MidiInputs.Add(new MidiInputModel("无"));

            foreach (InputDevice device in InputDevice.GetAll()) MidiInputs.Add(new MidiInputModel(device.Name));

            SelectedMidiInput = MidiInputs[0];
            foreach (string speed in new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2" }) {
                MidiSpeeds.Add(new MidiSpeedModel(speed, double.Parse(speed)));
            }

            SelectedMidiSpeed = MidiSpeeds[3];

            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromSeconds(1);
        }

        #endregion

        #region Private Variables

        private string _songName;
        private double _songSlider;
        private double _maximumTime = 1;
        private string _currentTime = "0:00";
        private string _totalTime = "0:00";
        private string _playPauseIcon = "Play";
        private string _scale = "请校准音阶至：半音";

        private BindableCollection<MidiInputModel> _midiInputs = new BindableCollection<MidiInputModel>();
        private BindableCollection<MidiTrackModel> _midiTracks = new BindableCollection<MidiTrackModel>();
        private BindableCollection<MidiSpeedModel> _midiSpeeds = new BindableCollection<MidiSpeedModel>();
        private MidiInputModel _selectedMidiInput;
        private MidiSpeedModel _selectedMidiSpeed;

        private bool _enableVibrato = true;
        private bool _transposeNotes = true;
        private string _historyMusicPath = "";
        private bool _ignoreSliderChange;

        private readonly string[] _scaleArray = {
            "半音",
            "六式音阶",
            "大调",
            "小调",
            "平调子",
            "弗里几亚属音",
            "阳调式",
            "五声小调",
            "五声大调"
        };

        private IEnumerable<TrackChunk> _midiTrackChunks;
        private TrackChunk _firstTrack;

        private Timer _playTimer;
        private ITimeSpan _playTime = new MidiTimeSpan();

        private readonly Version _programVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private string _versionString = "";

        #endregion

        #region Public Variables

        private MidiFile _midiFile;
        private TempoMap _tempoMap;
        private Playback _playback;
        private InputDevice _inputDevice;

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

        private double MaximumTime {
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

                if (value?.DeviceName != null && value.DeviceName != "无") {
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

        private MidiSpeedModel SelectedMidiSpeed {
            get => _selectedMidiSpeed;
            set {
                _selectedMidiSpeed = value;
                NotifyOfPropertyChange(() => SelectedMidiSpeed);

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

        private string HistoryMusicPath {
            get => _historyMusicPath;
            set {
                _historyMusicPath = value;
                SetUpProperties("HistoryMusicPath", value);
            }
        }


        public string Scale {
            get => _scale;
            set {
                _scale = value;
                NotifyOfPropertyChange(() => Scale);
            }
        }

        #endregion

        #region Methods

        private void SetUpProperties(string property, string target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
            NotifyOfPropertyChange(() => property);
        }

        private void SetUpProperties(string property, bool target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
            NotifyOfPropertyChange(() => property);
        }

        private void SetUpMidi(string filePath) {
            MidiTracks.Clear();
            _midiFile = MidiFile.Read(filePath, new ReadingSettings {
                InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits,
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOff,
                UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte,
                UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore,
                MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore
            });
            SongName = Path.GetFileNameWithoutExtension(filePath);
            _tempoMap = _midiFile.GetTempoMap();
            TimeSpan midiFileDuration = _midiFile.GetDuration<MetricTimeSpan>();
            TotalTime = midiFileDuration.ToString("m\\:ss");
            MaximumTime = midiFileDuration.TotalSeconds;
            UpdateSlider(0);
            CurrentTime = "0:00";
            _midiTrackChunks = _midiFile.GetTrackChunks();
            if (_midiTrackChunks.Count() > 1) {
                _firstTrack = _midiTrackChunks.FirstOrDefault();
                _midiFile.Chunks.Remove(_firstTrack);
                MidiTracks.Add(new MidiTrackModel(_firstTrack, true));
                foreach (var track in _midiFile.GetTrackChunks()) MidiTracks.Add(new MidiTrackModel(track, true));
            }
            else {
                _firstTrack = _midiTrackChunks.FirstOrDefault();
                _midiFile.Chunks.Remove(_firstTrack);
                MidiTracks.Add(new MidiTrackModel(_firstTrack, true));
            }
        }

        public void OpenFile() {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MIDI file|*.mid;*.midi;"; // Filter to only midi files
            if (openFileDialog.ShowDialog() != true) return;
            ClearMidi();
            HistoryMusicPath = openFileDialog.FileName;
            SetUpMidi(openFileDialog.FileName);
        }

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
            }
            else if (PlayPauseIcon == "Pause") {
                PlayPauseIcon = "Play";
                _playTimer.Dispose();
            }
            else {
                PlayPauseIcon = "Pause";

                ActionManager.OnSongPlay();
                _playTimer = new Timer();
                _playTimer.Interval = 100;
                _playTimer.Elapsed += PlayTimerElapsed;
                _playTimer.Start();
            }
        }

        private void PlayTimerElapsed(object sender, ElapsedEventArgs e) {
            if (ActionManager.IsWindowFocused("Warframe")) {
                _playback.Start();
                _playTimer.Dispose();
            }
        }

        private void ResetMidi() {
            if (_playback != null) {
                _playback.MoveToStart();
                UpdateSlider(0);
                _playTime = new MidiTimeSpan();
                PlayPauseIcon = "Play";
                CurrentTime = "0:00";
            }
        }

        public void Restart() {
            ResetMidi();
        }

        public void Clear() {
            ClearMidi();
            HistoryMusicPath = "";
        }

        public void RefreshDevices() {
            MidiInputs.Clear();
            MidiInputs.Add(new MidiInputModel("无"));

            foreach (var device in InputDevice.GetAll()) MidiInputs.Add(new MidiInputModel(device.Name));

            SelectedMidiInput = MidiInputs[0];
        }

        private void UpdateScale(int scaleIndex) {
            Scale = $"请校准音阶至：{_scaleArray[scaleIndex]}";
        }

        #endregion

        #region EventHandlers

        private void OnTick(object sender, PlaybackCurrentTimeChangedEventArgs e) {
            foreach (var playbackTime in e.Times) {
                TimeSpan time = (MetricTimeSpan)playbackTime.Time;

                UpdateSlider(time.TotalSeconds);
                CurrentTime = time.ToString("m\\:ss");
            }
        }

        private void OnNoteEvent(object sender, MidiEventPlayedEventArgs e) {
            switch (e.Event.EventType) {
                case MidiEventType.SetTempo:
                    var tempo = e.Event as SetTempoEvent;
                    //playback.Speed = tempo.MicrosecondsPerQuarterNote;
                    return;
                case MidiEventType.NoteOn:
                    var note = e.Event as NoteOnEvent;
                    if (note != null && note.Velocity <= 0) return;

                    //Check if the user has tabbed out of warframe, and stop playback to avoid Scale issues
                    if (!ActionManager.PlayNote(note, EnableVibrato, TransposeNotes))
                        PlayPause();
                    UpdateScale(ActionManager.activeScale);
                    return;
                default:
                    return;
            }
        }

        private void OnNoteEvent(object sender, MidiEventReceivedEventArgs e) {
            if (e.Event.EventType != MidiEventType.NoteOn) return;

            var note = e.Event as NoteOnEvent;
            if (note != null && note.Velocity <= 0) return;

            ActionManager.PlayNote(note, EnableVibrato, TransposeNotes);
        }

        private void UpdateSlider(double value) {
            _ignoreSliderChange = true;
            SongSlider = value;
        }

        #endregion
    }
}