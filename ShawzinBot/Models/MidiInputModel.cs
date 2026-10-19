namespace ShawzinBot.Models {
    public class MidiInputModel {
        public MidiInputModel(string deviceName, bool isNone = false) {
            DeviceName = deviceName;
            IsNone = isNone;
        }

        public string DeviceName { get; private set; }

        public bool IsNone { get; private set; }
    }
}