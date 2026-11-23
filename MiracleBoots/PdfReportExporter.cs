using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ITextFont = iTextSharp.text.Font;
using PdfDocument = iTextSharp.text.Document;

namespace MiracleBoots
{
    /// <summary>
    /// Формирует PDF-отчёт о продажах за выбранный период.
    /// </summary>
    public static class PdfReportExporter
    {
        public static void Export(string path, SalesReport report)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Не указан путь для сохранения отчёта.", nameof(path));

            if (report == null)
                throw new ArgumentNullException(nameof(report));

            string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (string.IsNullOrWhiteSpace(fontsFolder))
                fontsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");

            string regularFontPath = Path.Combine(fontsFolder, "arial.ttf");
            if (!File.Exists(regularFontPath))
                regularFontPath = Path.Combine(fontsFolder, "tahoma.ttf");

            if (!File.Exists(regularFontPath))
                throw new FileNotFoundException("Не найден шрифт Arial или Tahoma для формирования PDF.");

            BaseFont baseFont = BaseFont.CreateFont(regularFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            ITextFont titleFont = new ITextFont(baseFont, 16, ITextFont.BOLD);
            ITextFont headingFont = new ITextFont(baseFont, 12, ITextFont.BOLD);
            ITextFont textFont = new ITextFont(baseFont, 11, ITextFont.NORMAL);
            ITextFont smallFont = new ITextFont(baseFont, 9, ITextFont.ITALIC);

            var document = new PdfDocument(PageSize.A4, 40, 40, 40, 40);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                var title = new Paragraph("Отчёт по продажам «Чудо Обувь»", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph(" ", textFont));

                document.Add(new Paragraph("Период: " + report.Start.ToString("dd.MM.yyyy") +
                                           " — " + report.End.ToString("dd.MM.yyyy"), textFont));
                document.Add(new Paragraph("Общее количество заказов за период: " +
                                           report.OrdersCount, textFont));
                document.Add(new Paragraph("Общая сумма продаж за период: " +
                                           report.TotalSales.ToString("N2") + " руб.", textFont));
                document.Add(new Paragraph(" ", textFont));

                document.Add(new Paragraph("Топ-5 моделей обуви по количеству проданных пар", headingFont));
                document.Add(new Paragraph(" ", textFont));

                var table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 8f, 48f, 30f, 14f });

                table.AddCell(CreateCell("№", headingFont));
                table.AddCell(CreateCell("Наименование модели", headingFont));
                table.AddCell(CreateCell("Производитель", headingFont));
                table.AddCell(CreateCell("Количество пар", headingFont));

                if (report.TopModels == null || report.TopModels.Count == 0)
                {
                    var emptyCell = new PdfPCell(new Phrase("За выбранный период заказов нет", textFont))
                    {
                        Colspan = 4,
                        Padding = 6,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(emptyCell);
                }
                else
                {
                    foreach (var row in report.TopModels)
                    {
                        table.AddCell(CreateCell(row.Rank.ToString(), textFont));
                        table.AddCell(CreateCell(row.Model, textFont));
                        table.AddCell(CreateCell(row.Manufacturer, textFont));
                        table.AddCell(CreateCell(row.Pairs.ToString(), textFont));
                    }
                }

                document.Add(table);
                document.Add(new Paragraph(" ", textFont));
                document.Add(new Paragraph("Отчёт сформирован: " +
                                           DateTime.Now.ToString("dd.MM.yyyy HH:mm"), smallFont));

                document.Close();
            }
        }

        private static PdfPCell CreateCell(string text, ITextFont font)
        {
            return new PdfPCell(new Phrase(text ?? string.Empty, font))
            {
                Padding = 6
            };
        }
    }
}
