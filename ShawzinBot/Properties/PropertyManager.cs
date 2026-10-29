namespace ShawzinBot.Properties {
    public class PropertyManager {
        // 把字符串型配置写入用户设置并持久化
        // property：设置项名称；target：要写入的值
        public static void SetUpProperties(string property, string target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
        }

        // 把布尔型配置写入用户设置并持久化
        // property：设置项名称；value：要写入的值
        public static void SetUpProperties(string property, bool target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
        }

        // 把整型配置写入用户设置并持久化
        // property：设置项名称；value：要写入的值
        public static void SetUpProperties(string property, int target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
        }
    }
}