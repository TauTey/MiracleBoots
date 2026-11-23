USE [Торговая организация]
GO
CREATE VIEW Точки_без_заказов AS
SELECT 
    t.ID AS Код_точки,
    t.Тип,
    t.Адрес,
    t.ФИО_заведующего,
    t.ID_Торговой_организации
FROM [dbo].[Торговая точка] t
LEFT JOIN [dbo].[Заказы поставщикам] z ON t.ID = z.ID_торговой_точки
WHERE z.ID IS NULL
GO