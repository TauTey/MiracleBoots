using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Составление письма заведующему с просьбой изменить данные сотрудника.
    /// Используется как для новой заявки, так и для повторной (после ответа заведующего).
    /// </summary>
    public partial class RequestPage : Page
    {
        private readonly ChangeRequest _previous;
        private readonly UserSession _user;

        public RequestPage(ChangeRequest previous)
        {
            InitializeComponent();

            _previous = previous;
            _user = Manager.CurrentUser;

            if (_user == null)
            {
                Manager.ResetNavigation(new AuthPage());
                return;
            }

            if (_user.Role != UserRole.Employee)
            {
                MessageBox.Show("Заявки на изменение данных доступны только сотрудникам.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            Configure();
        }

        private void Configure()
        {
            var managerId = _previous != null && _previous.ManagerID.HasValue
                ? _previous.ManagerID
                : (_user.Point != null ? (int?)_user.Point.ManagerID : null);

            if (managerId.HasValue)
            {
                var manager = Manager.Context.Users
                    .FirstOrDefault(u => u.UserID == managerId.Value);

                RecipientText.Text = manager != null
                    ? "Получатель: " + manager.ФИО + ", заведующий торговой точкой"
                    : "Получатель: заведующий торговой точкой";
            }
            else
            {
                RecipientText.Text = "Получатель: заведующий торговой точкой";
            }

            if (_previous == null)
            {
                SubtitleText.Text = "Опишите, какие данные нужно исправить";
                return;
            }

            PreviousCard.Visibility = Visibility.Visible;
            PreviousMessage.Text = _previous.Message;

            if (!string.IsNullOrWhiteSpace(_previous.ManagerComment))
            {
                PreviousComment.Text = _previous.ManagerComment;
                PreviousComment.Visibility = Visibility.Visible;
            }

            if (_previous.Status == RequestStatus.Applied)
            {
                SubtitleText.Text = "Что именно было внесено неверно";
                PreviousTitle.Text = "Данные были изменены. Опишите ошибки повторно";
            }
            else
            {
                SubtitleText.Text = "Составьте новый запрос";
                PreviousTitle.Text = "Предыдущая заявка отклонена";
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var message = (MessageTextBox.Text ?? string.Empty).Trim();

            if (message.Length < 10)
            {
                ShowError("Опишите просьбу подробнее (не менее 10 символов).");
                return;
            }

            var managerId = _previous != null && _previous.ManagerID.HasValue
                ? _previous.ManagerID
                : (_user.Point != null ? (int?)_user.Point.ManagerID : null);

            if (!managerId.HasValue)
            {
                ShowError("Не удалось определить заведующего вашей торговой точки.");
                return;
            }

            try
            {
                var context = Manager.Context;

                var hasPending = context.ChangeRequests.Any(r =>
                    r.UserID == _user.Id && r.Status == RequestStatus.Pending);

                if (hasPending)
                {
                    ShowError("У вас уже есть заявка, ожидающая решения заведующего.");
                    return;
                }

                // Если сотрудник нашёл ошибки в уже изменённых данных,
                // предыдущая заявка помечается как требующая повторного запроса.
                if (_previous != null && _previous.Status == RequestStatus.Applied)
                {
                    _previous.Status = RequestStatus.NeedsFix;
                    _previous.ResolvedAt = DateTime.Now;
                }

                var request = new ChangeRequest
                {
                    UserID = _user.Id,
                    Message = message,
                    Status = RequestStatus.Pending,
                    CreatedAt = DateTime.Now,
                    ManagerID = managerId.Value
                };

                context.ChangeRequests.Add(request);
                context.SaveChanges();

                MessageBox.Show(
                    "Запрос отправлен заведующему. Ответ появится в вашем профиле.",
                    "Заявка на изменение данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                GoBack();
            }
            catch (Exception ex)
            {
                ShowError("Не удалось отправить запрос.\n" + ex.Message);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (Manager.MainFrame != null && Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
            else if (Manager.MainFrame != null)
                Manager.MainFrame.Navigate(new MainPage());
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
