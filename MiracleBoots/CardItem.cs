using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiracleBoots
{
    public class CartItem
    {
        public int ID_StockItem { get; set; }
        public int ID_Product { get; set; }
        public string ProductName { get; set; }
        public decimal Size { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Total => Quantity * Price;
    }
}
