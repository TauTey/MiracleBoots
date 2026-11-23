USE [Торговая организация]
GO
SELECT p.ID, p.Название AS Поставщик, COUNT(z.ID) AS Количество_заказов
FROM [dbo].[Заказы поставщикам] z
RIGHT JOIN [dbo].[Поставщики] p ON z.ID_поставщика = p.ID
GROUP BY p.ID, p.Название
ORDER BY Количество_заказов DESC
GO