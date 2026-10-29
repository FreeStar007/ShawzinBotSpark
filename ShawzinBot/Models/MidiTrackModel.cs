using System.Linq;
using Melanchall.DryWetMidi.Core;
using ShawzinBot.ViewModels;

namespace ShawzinBot.Models {
    // MIDI轨道列表项模型，承载轨道勾选状态
    public class MidiTrackModel {
        private bool _isChecked;

        public MidiTrackModel(TrackChunk track) {
            Track = track;
            // 轨道名取自带名称事件的文本，缺失时为空
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        public MidiTrackModel(TrackChunk track, bool isChecked) {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            IsChecked = isChecked;
        }

        // 轨道显示名称
        public string TrackName { get; private set; }

        // 轨道数据块
        public TrackChunk Track { get; private set; }

        // 勾选状态；变更时标记播放器需要重建
        public bool IsChecked {
            get => _isChecked;
            set {
                _isChecked = value;
                MainViewModel.ReloadPlayback = true;
            }
        }
    }
}