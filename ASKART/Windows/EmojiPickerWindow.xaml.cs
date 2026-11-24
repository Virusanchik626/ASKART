using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Askart.Windows
{
    public partial class EmojiPickerWindow : Window
    {
        public string SelectedEmoji { get; private set; }

        public EmojiPickerWindow()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            LoadEmojis();
        }

        private void LoadEmojis()
        {
            string[] emojis = { "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "🙃",
                               "😉", "😊", "😇", "🥰", "😍", "🤩", "😘", "😗", "😚", "😙",
                               "🥲", "😋", "😛", "😜", "🤪", "😝", "🤑", "🤗", "🤭", "🤫",
                               "🤔", "🤐", "🤨", "😐", "😑", "😶", "😏", "😒", "🙄", "😬",
                               "👍", "👎", "👊", "✊", "🤛", "🤜", "👏", "🙌", "👐", "🤲",
                               "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "💯", "💔" };

            EmojiPanel.ItemsSource = emojis;
        }

        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SelectedEmoji = button.Content.ToString();
            DialogResult = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
