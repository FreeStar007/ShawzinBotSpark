using System.Windows;
using System.Windows.Input;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    public partial class ThemedMessageBox {
        public ThemedMessageBox(string message, string title) {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            OkButton.Content = LanguageManager.GetString("DialogOk");

            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsVisible) {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void OnOk(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnDragMove(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        public static void Show(string message, string title = "") {
            new ThemedMessageBox(message, title).ShowDialog();
        }
    }
}