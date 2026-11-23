using System.Windows;

namespace TradingOrganization
{
    /// <summary>
    /// Универсальное окно ввода причины: отклонение или отмена заявки.
    /// </summary>
    public partial class CommentDialog : Window
    {
        private readonly string _emptyMessage;

        /// <summary>
        /// Диалог причины отклонения заявки (используется по умолчанию).
        /// </summary>
        public CommentDialog(string subject)
            : this(subject,
                   "Причина отклонения",
                   "Отклонить заявку",
                   "Например: «Указана неверная единица количества, цена слишком высокая».",
                   "Опишите причину отклонения (не менее 5 символов).")
        {
        }

        /// <summary>
        /// Диалог причины с настраиваемым заголовком и подписями.
        /// </summary>
        public CommentDialog(string subject, string title, string actionText,
                             string hint, string emptyMessage)
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(title))
            {
                Title = title;
                HeadingText.Text = title;
            }

            if (!string.IsNullOrWhiteSpace(actionText))
                ActionBtn.Content = actionText;

            HintText.Text = hint ?? string.Empty;
            HintText.Visibility = string.IsNullOrWhiteSpace(hint)
                ? Visibility.Collapsed
                : Visibility.Visible;

            _emptyMessage = string.IsNullOrWhiteSpace(emptyMessage)
                ? "Опишите причину (не менее 5 символов)."
                : emptyMessage;

            SubjectText.Text = subject ?? string.Empty;
            Loaded += (s, e) => CommentBox.Focus();
        }

        /// <summary>
        /// Введённая причина.
        /// </summary>
        public string Comment { get; private set; }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var comment = (CommentBox.Text ?? string.Empty).Trim();

            if (comment.Length < 5)
            {
                ShowError(_emptyMessage);
                return;
            }

            Comment = comment;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message ?? string.Empty;
            ErrorText.Visibility = string.IsNullOrEmpty(message)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
