using System.Windows;
using System.Windows.Input;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    // 与应用主题一致的消息框，用于替代系统原生弹窗
    public partial class ThemedMessageBox {
        // message：正文内容；title：标题
        public ThemedMessageBox(string message, string title) {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            OkButton.Content = LanguageManager.GetString("DialogOk");

            // 主窗口可见时以其为Owner并居中于Owner，否则居中于屏幕
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsVisible) {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        // 点击确定按钮关闭对话框
        private void OnOk(object sender, RoutedEventArgs e) {
            Close();
        }

        // 按住标题栏区域时拖动对话框
        private void OnDragMove(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        // 以模态方式显示消息框，title为空时标题留白
        public static void Show(string message, string title = "") {
            new ThemedMessageBox(message, title).ShowDialog();
        }
    }
}