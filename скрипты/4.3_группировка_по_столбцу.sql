USE [Торговая организация]
GO
SELECT 
    ID_торговой_точки,
    COUNT(*) AS Количество_продавцов
FROM [dbo].[Продавцы]
GROUP BY ID_торговой_точки
ORDER BY ID_торговой_точки
GO