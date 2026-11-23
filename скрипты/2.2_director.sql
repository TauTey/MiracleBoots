USE [Торговая организация]
GO
-- Предоставление полных прав роли директора
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Торговая организация] TO DIRECTOR
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Торговая точка] TO DIRECTOR
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Продавцы] TO DIRECTOR
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Поставщики] TO DIRECTOR
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Заказы поставщикам] TO DIRECTOR
GRANT EXECUTE TO DIRECTOR
GO