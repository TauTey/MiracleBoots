USE [Торговая организация]
GO
-- Предоставление прав роли заведующего точкой
GRANT SELECT, INSERT, UPDATE ON [dbo].[Торговая точка] TO STORE_MANAGER
GRANT SELECT, INSERT, UPDATE ON [dbo].[Продавцы] TO STORE_MANAGER
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Заказы поставщикам] TO STORE_MANAGER
GRANT SELECT ON [dbo].[Поставщики] TO STORE_MANAGER
GO