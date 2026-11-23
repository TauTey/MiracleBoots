USE [Торговая организация]
GO
SELECT p.Фамилия, p.Имя, p.Должность, t.Тип, t.Адрес
FROM [dbo].[Продавцы] p
INNER JOIN [dbo].[Торговая точка] t ON p.ID_торговой_точки = t.ID
GO