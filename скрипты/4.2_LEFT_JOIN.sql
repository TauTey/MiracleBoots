USE [Торговая организация]
GO
SELECT t.ID, t.Тип, t.Адрес, COUNT(z.ID) AS Количество_заказов
FROM [dbo].[Торговая точка] t
LEFT JOIN [dbo].[Заказы поставщикам] z ON t.ID = z.ID_торговой_точки
GROUP BY t.ID, t.Тип, t.Адрес
ORDER BY Количество_заказов DESC
GO