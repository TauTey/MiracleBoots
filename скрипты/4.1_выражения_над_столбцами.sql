USE [Торговая организация]
GO
SELECT ID, Название_товара, Количество, Цена
FROM [dbo].[Заказы поставщикам]
WHERE Количество LIKE N'%кг%' 
  AND (Цена / CAST(LEFT(Количество, CHARINDEX(' ', Количество) - 1) AS FLOAT)) > 100
GO