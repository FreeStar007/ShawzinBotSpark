namespace ShawzinBot.Properties {
    public static class PropertyManager {
        // 把配置写入用户设置并持久化
        // property：设置项名称；target：要写入的值
        public static void SetUpProperties(string property, object target) {
            Settings.Default[property] = target;
            Settings.Default.Save();
        }
    }
}