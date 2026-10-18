using System.Linq;
using Melanchall.DryWetMidi.Core;
using ShawzinBot.ViewModels;

namespace ShawzinBot.Models {
    public class MidiTrackModel {
        private bool _isChecked;

        public MidiTrackModel(TrackChunk track) {
            Track = track;
            var trackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            TrackName = string.IsNullOrEmpty(trackName) || trackName.Replace(" ", "").Replace("?", "") == ""
                ? "未知段落"
                : trackName;
        }

        public MidiTrackModel(TrackChunk track, bool isChecked) {
            Track = track;
            var trackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            TrackName = string.IsNullOrEmpty(trackName) || trackName.Replace(" ", "") == ""
                ? "未知段落"
                : trackName;
            IsChecked = isChecked;
        }

        public string TrackName { get; private set; }
        public TrackChunk Track { get; private set; }

        public bool IsChecked {
            get => _isChecked;
            set {
                _isChecked = value;
                MainViewModel.reloadPlayback = true;
            }
        }
    }
}