USE [Торговая организация]
GO
-- Создание роли директора
EXEC sp_addrole 'DIRECTOR'
GO
-- Создание роли заведующего торговой точкой
EXEC sp_addrole 'STORE_MANAGER'
GO
-- Создание роли продавца
EXEC sp_addrole 'SELLER'
GO