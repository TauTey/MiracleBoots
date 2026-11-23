USE [master]
GO

-- Создание учётных записей на уровне сервера
EXEC sp_addlogin 'Ivanov_Dir', 'pass123', 'Торговая организация'
EXEC sp_addlogin 'Petrov_Manager', 'pass456', 'Торговая организация'
EXEC sp_addlogin 'Sidorov_Seller', 'pass789', 'Торговая организация'
GO

USE [Торговая организация]
GO

-- Создание пользователей базы данных и их привязка к ролям
EXEC sp_adduser 'Ivanov_Dir', 'Ivanov_Dir'
EXEC sp_addrolemember 'DIRECTOR', 'Ivanov_Dir'
GO

EXEC sp_adduser 'Petrov_Manager', 'Petrov_Manager'
EXEC sp_addrolemember 'STORE_MANAGER', 'Petrov_Manager'
GO

EXEC sp_adduser 'Sidorov_Seller', 'Sidorov_Seller'
EXEC sp_addrolemember 'SELLER', 'Sidorov_Seller'
GO