using System.Linq;
using Melanchall.DryWetMidi.Core;
using ShawzinBot.ViewModels;

namespace ShawzinBot.Models {
    // MIDI音轨列表项模型，承载单条音轨的显示名与勾选状态
    public class MidiTrackModel {
        // 勾选状态的后备字段
        private bool _isChecked;

        // 创建音轨列表项，默认未勾选
        // track：音轨数据块
        public MidiTrackModel(TrackChunk track) {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        // 创建音轨列表项并指定初始勾选状态
        // track：音轨数据块；isChecked：初始是否勾选
        public MidiTrackModel(TrackChunk track, bool isChecked) {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            IsChecked = isChecked;
        }

        // 音轨显示名称，取自带名称事件的文本，事件缺失时为空
        public string TrackName { get; private set; }

        // 音轨数据块
        public TrackChunk Track { get; private set; }

        // 音轨是否参与播放，赋值会连带把视图模型置为需要重建播放器
        public bool IsChecked {
            get => _isChecked;
            set {
                _isChecked = value;
                MainViewModel.ReloadPlayback = true;
            }
        }
    }
}