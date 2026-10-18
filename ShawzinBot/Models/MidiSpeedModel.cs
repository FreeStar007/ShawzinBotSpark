namespace ShawzinBot.Models {
    public class MidiSpeedModel {
        public MidiSpeedModel(string speedName, double speed) {
            SpeedName = speedName;
            Speed = speed;
        }

        public string SpeedName { get; private set; }
        public double Speed { get; private set; }
    }
}