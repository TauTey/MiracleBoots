USE [Торговая организация]
GO
SELECT 
    COALESCE(t.ID, z.ID_торговой_точки) AS ID_точки,
    t.Тип,
    z.ID AS Номер_заказа,
    z.Название_товара
FROM [dbo].[Торговая точка] t
FULL OUTER JOIN [dbo].[Заказы поставщикам] z ON t.ID = z.ID_торговой_точки
ORDER BY ID_точки, Номер_заказа
GO