namespace ShawzinBot.Models {
    // MIDI播放倍速列表项模型
    public class MidiSpeedModel {
        // speedName：倍速显示文本；speed：实际倍速数值
        public MidiSpeedModel(string speedName, double speed) {
            SpeedName = speedName;
            Speed = speed;
        }

        // 倍速显示名称
        public string SpeedName { get; private set; }

        // 倍速数值
        public double Speed { get; private set; }
    }
}