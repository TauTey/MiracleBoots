using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TradingOrganization
{
    // Дополнительные (вычисляемые) свойства сущностей для привязки в XAML.

    public partial class User
    {
        public string ФИО
        {
            get { return NameHelper.FullName(LastName, FirstName, Patronymic); }
        }

        public string ДолжностьНазвание
        {
            get { return Position != null ? Position.Name : "—"; }
        }

        public string ДатаРожденияТекст
        {
            get { return BirthDate.HasValue ? NameHelper.DateText(BirthDate.Value) : "—"; }
        }

        public int Возраст
        {
            get { return BirthDate.HasValue ? NameHelper.Age(BirthDate.Value) : 0; }
        }

        public string ВозрастТекст
        {
            get { return BirthDate.HasValue ? Возраст + " лет" : "—"; }
        }

        public string ПолНазвание
        {
            get
            {
                if (Gender == "М") return "Мужской";
                if (Gender == "Ж") return "Женский";
                return "—";
            }
        }

        public string ТочкаОписание
        {
            get
            {
                return Store == null
                    ? "—"
                    : NameHelper.Capitalize(Store.Type) + ", " + Store.Address;
            }
        }

        public string ОрганизацияНазвание
        {
            get
            {
                var organization = Store != null ? Store.Organization : null;
                return organization != null ? organization.Name : "—";
            }
        }

        public BitmapImage Фотография
        {
            get { return PhotoHelper.Load(Photo, PhotoFolder); }
        }

        public string PhotoFolder
        {
            get
            {
                switch (Role)
                {
                    case Roles.Employee: return "Images_st";
                    case Roles.Manager: return "Images_zv";
                    default: return "Images_dr";
                }
            }
        }

        public string РольНазвание
        {
            get
            {
                switch (RoleValue)
                {
                    case UserRole.Employee: return "Сотрудник";
                    case UserRole.Manager: return "Заведующий точкой";
                    default: return "Директор";
                }
            }
        }

        public string ЛогинПодпись
        {
            get { return RoleValue == UserRole.Director ? "Налоговый номер" : "Логин"; }
        }

        public string Логин
        {
            get { return Login; }
        }
    }

    public partial class Store
    {
        public string ТипСЗаглавной
        {
            get { return NameHelper.Capitalize(Type); }
        }

        public string ЗаведующийФИО
        {
            get { return Manager != null ? Manager.ФИО : "Не назначен"; }
        }

        public BitmapImage ЗаведующийФотография
        {
            get { return Manager != null ? Manager.Фотография : PhotoHelper.Placeholder; }
        }

        public int КоличествоСотрудников
        {
            get { return Employees != null ? Employees.Count : 0; }
        }

        public int КоличествоЗаказов
        {
            get { return Orders != null ? Orders.Count : 0; }
        }

        public string ОрганизацияНазвание
        {
            get { return Organization != null ? Organization.Name : "—"; }
        }

        public string Подпись
        {
            get
            {
                var organization = Organization != null ? Organization.Name : "—";
                return ТипСЗаглавной + ", " + Address + " (" + organization + ")";
            }
        }
    }

    public partial class Order
    {
        public string ПоставщикНазвание
        {
            get { return Supplier != null ? Supplier.Name : "—"; }
        }

        public string ДатаТекст
        {
            get { return OrderDate.ToString("dd.MM.yyyy"); }
        }

        public string СуммаТекст
        {
            get { return Price.ToString("N2") + " ₽"; }
        }
    }

    public partial class ChangeRequest
    {
        public string СтатусНазвание
        {
            get
            {
                switch (Status)
                {
                    case RequestStatus.Pending: return "Ожидает решения";
                    case RequestStatus.Applied: return "Данные изменены";
                    case RequestStatus.Rejected: return "Отклонена";
                    case RequestStatus.Confirmed: return "Подтверждена";
                    case RequestStatus.NeedsFix: return "Требует повторного запроса";
                    default: return Status;
                }
            }
        }

        public string СозданТекст
        {
            get { return CreatedAt.ToString("dd.MM.yyyy HH:mm"); }
        }

        public Visibility ДействияВидимость
        {
            get { return Status == RequestStatus.Pending ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility ОтветВидимость
        {
            get
            {
                return Status == RequestStatus.Applied || Status == RequestStatus.Rejected
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }

    public partial class OrderRequest
    {
        public string ПоставщикНазвание
        {
            get { return Supplier != null ? Supplier.Name : "—"; }
        }

        public string ПоставщикОписание
        {
            get
            {
                return Supplier == null
                    ? "—"
                    : Supplier.ActivityType + ", " + Supplier.City + ", " + Supplier.Address;
            }
        }

        public string ЗаведующийФИО
        {
            get { return Manager != null ? Manager.ФИО : "—"; }
        }

        public string ТочкаОписание
        {
            get
            {
                return Store == null
                    ? "—"
                    : NameHelper.Capitalize(Store.Type) + ", " + Store.Address;
            }
        }

        public string КоличествоТекст
        {
            get { return Quantity.ToString("0.##") + " " + Unit; }
        }

        public string ЦенаТекст
        {
            get { return Price.ToString("N2") + " ₽"; }
        }

        public string ДатаТекст
        {
            get { return CreatedAt.ToString("dd.MM.yyyy"); }
        }

        public string СозданТекст
        {
            get { return CreatedAt.ToString("dd.MM.yyyy HH:mm"); }
        }

        public string СтатусНазвание
        {
            get
            {
                switch (Status)
                {
                    case OrderRequestStatus.Pending: return "Ожидает решения директора";
                    case OrderRequestStatus.Approved: return "Подтверждена";
                    case OrderRequestStatus.Rejected: return "Отклонена";
                    case OrderRequestStatus.Cancelled: return "Отменена директором";
                    default: return Status;
                }
            }
        }

        public Brush СтатусЦвет
        {
            get
            {
                switch (Status)
                {
                    case OrderRequestStatus.Approved: return new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57));
                    case OrderRequestStatus.Rejected: return new SolidColorBrush(Color.FromRgb(0xD6, 0x45, 0x45));
                    case OrderRequestStatus.Cancelled: return new SolidColorBrush(Color.FromRgb(0x6C, 0x7C, 0x99));
                    default: return new SolidColorBrush(Color.FromRgb(0x3E, 0x51, 0xC7));
                }
            }
        }

        /// <summary>
        /// Кнопки «Подтвердить»/«Отклонить» — только для заявок, ожидающих решения.
        /// </summary>
        public Visibility ДействияВидимость
        {
            get { return Status == OrderRequestStatus.Pending ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Кнопка «Отменить заявку» — для уже подтверждённых заявок.
        /// </summary>
        public Visibility ОтменаВидимость
        {
            get { return Status == OrderRequestStatus.Approved ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Кнопка «Редактировать» — заведующий правит заявку, пока её не рассмотрели.
        /// </summary>
        public Visibility РедактироватьВидимость
        {
            get { return Status == OrderRequestStatus.Pending ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility КомментарийВидимость
        {
            get
            {
                return string.IsNullOrWhiteSpace(DirectorComment)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
    }
}
