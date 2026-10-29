namespace ShawzinBot.Models {
    // MIDI输入设备列表项模型
    public class MidiInputModel {
        // 创建设备列表项
        // deviceName：设备显示名称；isNone：是否代表“无设备”占位项
        public MidiInputModel(string deviceName, bool isNone = false) {
            DeviceName = deviceName;
            IsNone = isNone;
        }

        // 设备显示名称
        public string DeviceName { get; private set; }

        // 是否代表不使用MIDI输入设备
        public bool IsNone { get; private set; }
    }
}