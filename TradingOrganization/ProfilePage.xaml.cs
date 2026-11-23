using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace TradingOrganization
{
    /// <summary>
    /// Страница профиля: личные данные, маскированные учётные данные,
    /// сообщения по заявкам, смена аватара и выход из аккаунта.
    /// </summary>
    public partial class ProfilePage : Page
    {
        private readonly UserSession _user;
        private bool _credentialsVisible;

        public ProfilePage()
        {
            InitializeComponent();

            _user = Manager.CurrentUser;
            if (_user == null)
            {
                Manager.ResetNavigation(new AuthPage());
                return;
            }

            EditDataBtn.Visibility = _user.Role == UserRole.Employee
                ? Visibility.Collapsed
                : Visibility.Visible;
            RequestChangeBtn.Visibility = _user.Role == UserRole.Employee
                ? Visibility.Visible
                : Visibility.Collapsed;

            RefreshAll();

            Loaded += (s, e) => RefreshAll();
        }

        private void RefreshAll()
        {
            var user = _user.User;

            FioText.Text = _user.ФИО;
            RoleText.Text = _user.РольНазвание;
            AvatarImage.Source = PhotoHelper.Load(user.Photo, _user.PhotoFolder);

            BuildInfo(user);
            UpdateCredentials();
            LoadLetters();
        }

        private void BuildInfo(User user)
        {
            var rows = new List<InfoRow>();

            rows.Add(new InfoRow { Название = "Фамилия", Значение = user.LastName });
            rows.Add(new InfoRow { Название = "Имя", Значение = user.FirstName });
            rows.Add(new InfoRow { Название = "Отчество", Значение = string.IsNullOrWhiteSpace(user.Patronymic) ? "—" : user.Patronymic });
            rows.Add(new InfoRow { Название = "Роль", Значение = _user.РольНазвание });
            rows.Add(new InfoRow { Название = "Дата рождения", Значение = user.ДатаРожденияТекст });
            rows.Add(new InfoRow { Название = "Возраст", Значение = user.ВозрастТекст });

            if (_user.Role == UserRole.Employee)
            {
                rows.Add(new InfoRow { Название = "Должность", Значение = user.ДолжностьНазвание });
                rows.Add(new InfoRow { Название = "Пол", Значение = user.ПолНазвание });
                rows.Add(new InfoRow { Название = "Город", Значение = TextOrDash(user.City) });
                rows.Add(new InfoRow { Название = "Адрес проживания", Значение = TextOrDash(user.Address) });
                rows.Add(new InfoRow { Название = "Торговая точка", Значение = _user.ТочкаОписание });
                rows.Add(new InfoRow { Название = "Организация", Значение = _user.ОрганизацияНазвание });
            }
            else if (_user.Role == UserRole.Manager)
            {
                rows.Add(new InfoRow { Название = "Торговая точка", Значение = _user.ТочкаОписание });
                rows.Add(new InfoRow { Название = "Организация", Значение = _user.ОрганизацияНазвание });
            }
            else
            {
                rows.Add(new InfoRow { Название = "Организация", Значение = _user.ОрганизацияНазвание });

                if (_user.Organization != null)
                    rows.Add(new InfoRow { Название = "Адрес организации", Значение = _user.Organization.Address });
            }

            InfoList.ItemsSource = rows;
        }

        private static string TextOrDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "—" : value;
        }

        private void UpdateCredentials()
        {
            if (_credentialsVisible)
            {
                LoginValue.Text = _user.Логин;
                PasswordValue.Text = _user.Пароль;
                ShowCredentialsBtn.Content = "Скрыть";
            }
            else
            {
                LoginValue.Text = Mask(_user.Логин);
                PasswordValue.Text = Mask(_user.Пароль);
                ShowCredentialsBtn.Content = "Показать";
            }
        }

        private static string Mask(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "—";

            return new string('\u2022', Math.Max(6, value.Length));
        }

        private void ShowCredentials_Click(object sender, RoutedEventArgs e)
        {
            if (_credentialsVisible)
            {
                _credentialsVisible = false;
                UpdateCredentials();
                return;
            }

            var result = MessageBox.Show(
                "Показать вам пароль?\n\nУбедитесь, что никто кроме вас его не увидит.",
                "Показать пароль",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _credentialsVisible = true;
            UpdateCredentials();
        }

        private void LoadLetters()
        {
            LettersStack.Children.Clear();

            var requests = Manager.Context.ChangeRequests
                .Where(r => r.UserID == _user.Id && r.Status != RequestStatus.NeedsFix)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            if (requests.Count == 0)
            {
                LettersCard.Visibility = Visibility.Collapsed;
                return;
            }

            LettersCard.Visibility = Visibility.Visible;

            foreach (var request in requests)
            {
                LettersStack.Children.Add(BuildLetter(request));
            }
        }

        private FrameworkElement BuildLetter(ChangeRequest request)
        {
            var container = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(new TextBlock
            {
                Text = "Заявка №" + request.RequestID + " · " + request.СозданТекст,
                FontSize = 12.5,
                Foreground = (Brush)FindResource("MutedBrush"),
                VerticalAlignment = VerticalAlignment.Center
            });
            header.Children.Add(new TextBlock
            {
                Text = request.СтатусНазвание,
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = request.Status == RequestStatus.Rejected
                    ? (Brush)FindResource("ErrorBrush")
                    : (Brush)FindResource("AccentBrush"),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            container.Children.Add(header);

            if (request.Status == RequestStatus.Applied)
            {
                container.Children.Add(new TextBlock
                {
                    Text = "Данные были изменены. Проверьте, правильно ли внесены корректировки.",
                    FontSize = 14.5,
                    Foreground = (Brush)FindResource("TextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                buttons.Children.Add(CreateButton("Всё верно", "NavButton",
                    (s, e) => ConfirmRequest(request)));
                buttons.Children.Add(CreateButton("Есть ошибки", "DangerButton",
                    (s, e) => Manager.MainFrame.Navigate(new RequestPage(request)),
                    new Thickness(6, 0, 0, 0)));
                container.Children.Add(buttons);
            }
            else if (request.Status == RequestStatus.Rejected)
            {
                container.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(request.ManagerComment)
                        ? "Заведующий отклонил заявку."
                        : request.ManagerComment,
                    FontSize = 14,
                    Foreground = (Brush)FindResource("TextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                buttons.Children.Add(CreateButton("Составить новый запрос", "NavButton",
                    (s, e) => Manager.MainFrame.Navigate(new RequestPage(request))));
                container.Children.Add(buttons);
            }
            else if (request.Status == RequestStatus.Confirmed)
            {
                container.Children.Add(new TextBlock
                {
                    Text = "Изменения подтверждены. Спасибо!",
                    FontSize = 14,
                    Foreground = (Brush)FindResource("MutedBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }
            else
            {
                container.Children.Add(new TextBlock
                {
                    Text = "Запрос отправлен заведующему. Ожидайте ответа.",
                    FontSize = 14.5,
                    Foreground = (Brush)FindResource("TextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                container.Children.Add(new TextBlock
                {
                    Text = "«" + request.Message + "»",
                    FontSize = 13,
                    FontStyle = FontStyles.Italic,
                    Foreground = (Brush)FindResource("MutedBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }

            var wrapper = new Border
            {
                BorderBrush = (Brush)FindResource("LineBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = container
            };

            return new Border { Child = wrapper, Padding = new Thickness(0, 2, 0, 10) };
        }

        private Button CreateButton(string text, string styleKey,
            RoutedEventHandler handler, Thickness? margin = null)
        {
            var button = new Button
            {
                Content = text,
                Style = (Style)FindResource(styleKey),
                FontSize = 12.5,
                Padding = new Thickness(14, 7, 14, 7),
                Margin = margin ?? new Thickness(0)
            };
            button.Click += handler;
            return button;
        }

        private void ConfirmRequest(ChangeRequest request)
        {
            try
            {
                request.Status = RequestStatus.Confirmed;
                request.ResolvedAt = DateTime.Now;
                Manager.Context.SaveChanges();

                MessageBox.Show(
                    "Спасибо! Изменения подтверждены.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadLetters();
                var window = Window.GetWindow(this) as MainWindow;
                if (window != null)
                    window.RefreshHeader();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подтвердить изменения.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new EditUserPage(_user.User, null));
        }

        private void RequestChange_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new RequestPage(null));
        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите фотографию",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var directory = PhotoHelper.AvatarsDirectory;
                Directory.CreateDirectory(directory);

                var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                var fileName = string.Format("{0}_{1}_{2:yyyyMMddHHmmss}{3}",
                    _user.РольКод, _user.Id, DateTime.Now, extension);

                File.Copy(dialog.FileName, Path.Combine(directory, fileName), true);

                var oldPhoto = _user.User.Photo;
                if (!string.IsNullOrWhiteSpace(oldPhoto) &&
                    !Path.IsPathRooted(oldPhoto))
                {
                    var oldPath = Path.Combine(directory, oldPhoto);
                    if (File.Exists(oldPath))
                    {
                        try { File.Delete(oldPath); }
                        catch { /* старый файл не критичен */ }
                    }
                }

                _user.SetPhoto(fileName);
                Manager.Context.SaveChanges();

                AvatarImage.Source = PhotoHelper.Load(fileName, _user.PhotoFolder);

                var window = Window.GetWindow(this) as MainWindow;
                if (window != null)
                    window.RefreshHeader();

                MessageBox.Show("Фотография обновлена.", "Профиль",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось изменить фотографию.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из аккаунта?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            Manager.Logout();
            Manager.ResetNavigation(new AuthPage());
        }
    }
}
