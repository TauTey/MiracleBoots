USE [Торговая организация]
GO
SELECT 
    p1.Фамилия AS Продавец1, 
    p2.Фамилия AS Продавец2,
    p1.ID_торговой_точки
FROM [dbo].[Продавцы] p1
INNER JOIN [dbo].[Продавцы] p2 ON p1.ID_торговой_точки = p2.ID_торговой_точки
WHERE p1.ID < p2.ID
ORDER BY p1.ID_торговой_точки
GO