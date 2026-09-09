using System.Windows;
using System.Windows.Controls;

namespace Askart.Windows
{
    public partial class EmojiPickerWindow : Window
    {
        public string SelectedEmoji { get; private set; }

        public EmojiPickerWindow()
        {
            InitializeComponent();
            LoadEmojis();
        }

        private void LoadEmojis()
        {
            string[] emojis = { "😀", "😃", "", "😁", "😆", "", "🤣", "😂", "🙂", "🙃",
                               "😉", "😊", "😇", "🥰", "😍", "🤩", "😘", "😗", "😚", "😙",
                               "🥲", "😋", "😛", "😜", "🤪", "😝", "🤑", "", "🤭", "🤫",
                               "🤔", "🤐", "🤨", "😐", "😑", "😶", "😏", "😒", "🙄", "😬",
                               "👍", "👎", "👊", "✊", "", "🤜", "👏", "🙌", "👐", "",
                               "❤️", "", "💛", "💚", "", "💜", "🖤", "🤍", "💯", "💔" };

            foreach (var emoji in emojis)
            {
                var button = new Button
                {
                    Content = emoji,
                    FontSize = 24,
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5, 5, 5, 5),
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                button.Click += (s, e) =>
                {
                    SelectedEmoji = emoji;
                    DialogResult = true;
                };

                EmojiPanel.Children.Add(button);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}