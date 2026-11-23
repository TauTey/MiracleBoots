USE [Торговая организация]
GO
CREATE VIEW Итоги_заказов_по_поставщикам AS
SELECT 
    p.ID AS Код_поставщика,
    p.Название AS Поставщик,
    COUNT(z.ID) AS Количество_заказов,
    SUM(z.Цена) AS Общая_сумма,
    AVG(z.Цена) AS Средняя_сумма_заказа
FROM [dbo].[Поставщики] p
LEFT JOIN [dbo].[Заказы поставщикам] z ON p.ID = z.ID_поставщика
GROUP BY p.ID, p.Название
GO