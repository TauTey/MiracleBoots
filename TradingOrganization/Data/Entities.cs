using System;
using System.Collections.Generic;

namespace TradingOrganization
{
    /// <summary>
    /// Роли пользователей, хранящиеся в столбце Users.Role.
    /// </summary>
    public static class Roles
    {
        public const string Employee = "Employee";
        public const string Manager = "Manager";
        public const string Director = "Director";
    }

    /// <summary>
    /// Статусы заявок на изменение данных.
    /// </summary>
    public static class RequestStatus
    {
        public const string Pending = "Pending";     // ожидает решения заведующего
        public const string Applied = "Applied";     // данные изменены, сотрудник проверяет
        public const string Rejected = "Rejected";   // заведующий отклонил заявку
        public const string Confirmed = "Confirmed"; // сотрудник подтвердил изменения
        public const string NeedsFix = "NeedsFix";   // сотрудник нашёл ошибки, заявка переписана
    }

    /// <summary>
    /// Статусы заявок на заказы поставщикам.
    /// </summary>
    public static class OrderRequestStatus
    {
        public const string Pending = "Pending";     // ожидает решения директора
        public const string Approved = "Approved";   // директор подтвердил, заказ создан
        public const string Rejected = "Rejected";   // директор отклонил с причиной
        public const string Cancelled = "Cancelled"; // директор отменил подтверждённую заявку
    }

    public partial class Position
    {
        public Position()
        {
            Users = new HashSet<User>();
        }

        public int PositionID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Объединённая таблица пользователей: сотрудники, заведующие и директора.
    /// </summary>
    public partial class User
    {
        public User()
        {
            ManagedStores = new HashSet<Store>();
            Organizations = new HashSet<Organization>();
            ChangeRequests = new HashSet<ChangeRequest>();
            ResolvedRequests = new HashSet<ChangeRequest>();
        }

        public int UserID { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Patronymic { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Gender { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string Photo { get; set; }

        public string Role { get; set; }

        public int? PositionID { get; set; }

        public int? StoreID { get; set; }

        public virtual Position Position { get; set; }

        public virtual Store Store { get; set; }

        public virtual ICollection<Store> ManagedStores { get; set; }

        public virtual ICollection<Organization> Organizations { get; set; }

        public virtual ICollection<ChangeRequest> ChangeRequests { get; set; }

        public virtual ICollection<ChangeRequest> ResolvedRequests { get; set; }

        public UserRole RoleValue
        {
            get
            {
                switch (Role)
                {
                    case Roles.Manager: return UserRole.Manager;
                    case Roles.Director: return UserRole.Director;
                    default: return UserRole.Employee;
                }
            }
        }
    }

    public partial class Organization
    {
        public Organization()
        {
            Stores = new HashSet<Store>();
        }

        public int OrganizationID { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int DirectorID { get; set; }

        public virtual User Director { get; set; }

        public virtual ICollection<Store> Stores { get; set; }
    }

    public partial class Store
    {
        public Store()
        {
            Employees = new HashSet<User>();
            Orders = new HashSet<Order>();
        }

        public int StoreID { get; set; }

        public string Type { get; set; }

        public int OrganizationID { get; set; }

        public string Address { get; set; }

        public int ManagerID { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual User Manager { get; set; }

        public virtual ICollection<User> Employees { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    public partial class Supplier
    {
        public Supplier()
        {
            Orders = new HashSet<Order>();
        }

        public int SupplierID { get; set; }

        public string Name { get; set; }

        public string ActivityType { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    public partial class Order
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public int StoreID { get; set; }

        public int SupplierID { get; set; }

        public string ProductName { get; set; }

        public string Quantity { get; set; }

        public decimal Price { get; set; }

        public virtual Store Store { get; set; }

        public virtual Supplier Supplier { get; set; }
    }

    /// <summary>
    /// Заявка сотрудника на изменение своих данных.
    /// </summary>
    public partial class ChangeRequest
    {
        public int RequestID { get; set; }

        public int UserID { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int? ManagerID { get; set; }

        public string ManagerComment { get; set; }

        public virtual User User { get; set; }

        public virtual User Manager { get; set; }
    }

    /// <summary>
    /// Заявка заведующего на заказ товара у поставщика.
    /// Директор организации подтверждает её (создаётся заказ) или отклоняет с причиной.
    /// </summary>
    public partial class OrderRequest
    {
        public int OrderRequestID { get; set; }

        public int StoreID { get; set; }

        public int ManagerID { get; set; }

        public int DirectorID { get; set; }

        public int SupplierID { get; set; }

        public string ProductName { get; set; }

        public decimal Quantity { get; set; }

        public string Unit { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string DirectorComment { get; set; }

        public int? OrderID { get; set; }

        public virtual Store Store { get; set; }

        public virtual User Manager { get; set; }

        public virtual User Director { get; set; }

        public virtual Supplier Supplier { get; set; }

        public virtual Order Order { get; set; }
    }
}
