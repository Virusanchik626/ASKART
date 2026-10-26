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

            foreach (string emoji in emojis)
            {
                Button button = new Button();
                button.Content = emoji;
                button.FontSize = 24;
                button.Width = 50;
                button.Height = 50;
                button.Margin = new Thickness(5);
                button.Background = Brushes.Transparent;
                button.BorderThickness = new Thickness(0);
                button.Cursor = Cursors.Hand;
                button.Click += EmojiButton_Click;

                EmojiPanel.Children.Add(button);
            }
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
