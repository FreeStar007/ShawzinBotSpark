using System.Windows;
using System.Windows.Input;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    // 与应用主题一致的消息框，用于替代系统原生弹窗
    public partial class ThemedMessageBox {
        // 创建消息框，按当前语言填充确定按钮文案并决定弹窗位置
        // message：正文内容；title：标题文本
        public ThemedMessageBox(string message, string title) {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            OkButton.Content = LanguageManager.GetString("DialogOk");

            // 主窗口可见时以其为父窗口并居中于父窗口，否则居中于屏幕
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsVisible) {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        // 点击确定按钮关闭对话框
        // sender：确定按钮，本方法未使用；e：路由事件参数，本方法未使用
        private void OnOk(object sender, RoutedEventArgs e) {
            Close();
        }

        // 按住标题栏区域时拖动对话框
        // sender：标题栏元素，本方法未使用；e：鼠标按键事件参数，用于判断是否处于按下状态
        private void OnDragMove(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        // 以模态方式显示消息框
        // message：正文内容；title：标题文本，留空时标题区域为空白
        public static void Show(string message, string title = "") {
            new ThemedMessageBox(message, title).ShowDialog();
        }
    }
}