using System;
using System.Linq;

namespace TradingOrganization
{
    public enum UserRole
    {
        Employee,
        Manager,
        Director
    }

    /// <summary>
    /// Данные авторизованного пользователя (все роли хранятся в одной таблице Users).
    /// </summary>
    public class UserSession
    {
        public User User { get; private set; }

        public UserRole Role
        {
            get { return User != null ? User.RoleValue : UserRole.Employee; }
        }

        public Store Point { get; private set; }

        public Organization Organization { get; private set; }

        public int Id
        {
            get { return User != null ? User.UserID : 0; }
        }

        public string Фамилия
        {
            get { return User != null ? User.LastName : string.Empty; }
        }

        public string Имя
        {
            get { return User != null ? User.FirstName : string.Empty; }
        }

        public string Отчество
        {
            get { return User != null ? User.Patronymic : string.Empty; }
        }

        public string ФИО
        {
            get { return NameHelper.FullName(Фамилия, Имя, Отчество); }
        }

        public DateTime ДатаРождения
        {
            get { return User != null && User.BirthDate.HasValue ? User.BirthDate.Value : DateTime.MinValue; }
        }

        public int Возраст
        {
            get { return NameHelper.Age(ДатаРождения); }
        }

        public string Логин
        {
            get { return User != null ? User.Login : string.Empty; }
        }

        public string Пароль
        {
            get { return User != null ? User.Password : string.Empty; }
        }

        public string Фото
        {
            get { return User != null ? User.Photo : null; }
        }

        public string РольНазвание
        {
            get
            {
                switch (Role)
                {
                    case UserRole.Employee: return "Сотрудник";
                    case UserRole.Manager: return "Заведующий точкой";
                    default: return "Директор";
                }
            }
        }

        public string РольКод
        {
            get
            {
                switch (Role)
                {
                    case UserRole.Employee: return "staff";
                    case UserRole.Manager: return "manager";
                    default: return "director";
                }
            }
        }

        public string PhotoFolder
        {
            get
            {
                switch (Role)
                {
                    case UserRole.Employee: return "Images_st";
                    case UserRole.Manager: return "Images_zv";
                    default: return "Images_dr";
                }
            }
        }

        public string ЛогинПодпись
        {
            get { return Role == UserRole.Director ? "Налоговый номер" : "Логин"; }
        }

        public string ТочкаОписание
        {
            get
            {
                if (Point == null) return "—";
                return NameHelper.Capitalize(Point.Type) + ", " + Point.Address;
            }
        }

        public string ОрганизацияНазвание
        {
            get { return Organization != null ? Organization.Name : "—"; }
        }

        public void SetPhoto(string fileName)
        {
            if (User != null)
                User.Photo = fileName;
        }

        public static UserSession FromUser(User user)
        {
            if (user == null)
                return null;

            var session = new UserSession { User = user };

            switch (user.RoleValue)
            {
                case UserRole.Manager:
                    session.Point = user.ManagedStores.FirstOrDefault();
                    break;

                case UserRole.Director:
                    session.Organization = user.Organizations.FirstOrDefault();
                    break;

                default:
                    session.Point = user.Store;
                    break;
            }

            if (session.Organization == null && session.Point != null)
                session.Organization = session.Point.Organization;

            return session;
        }
    }

    /// <summary>
    /// Строка «название — значение» для страницы профиля.
    /// </summary>
    public class InfoRow
    {
        public string Название { get; set; }

        public string Значение { get; set; }
    }
}
