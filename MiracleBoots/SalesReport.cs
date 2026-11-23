using System;
using System.Collections.Generic;

namespace MiracleBoots
{
    /// <summary>
    /// Строка отчёта «Топ-5 моделей».
    /// </summary>
    public class TopModelRow
    {
        public int Rank { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public int Pairs { get; set; }
    }

    /// <summary>
    /// Подготовленные данные отчёта о продажах за выбранный период.
    /// </summary>
    public class SalesReport
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalSales { get; set; }
        public List<TopModelRow> TopModels { get; set; }
    }
}
