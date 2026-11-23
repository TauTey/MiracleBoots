USE [Торговая организация]
GO
SELECT 
    ID_торговой_точки,
    COUNT(*) AS Количество_продавцов
FROM [dbo].[Продавцы]
GROUP BY ID_торговой_точки
HAVING COUNT(*) > 2
ORDER BY Количество_продавцов DESC
GO