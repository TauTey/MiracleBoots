USE [Торговая организация]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Продавцы](
	[ID] [int] NOT NULL,
	[Имя] [nvarchar](100) NOT NULL,
	[Фамилия] [nvarchar](100) NOT NULL,
	[Отчество] [nvarchar](100) NULL,
	[ID_торговой_точки] [int] NOT NULL,
	[Должность] [nvarchar](100) NOT NULL,
	[Год_рождения] [date] NOT NULL,
	[Пол] [nvarchar](10) NOT NULL,
	[Адрес_проживания] [nvarchar](100) NOT NULL,
	[Город] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Продавцы] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, 
	   STATISTICS_NORECOMPUTE = OFF, 
	   IGNORE_DUP_KEY = OFF, 
	   ALLOW_ROW_LOCKS = ON, 
	   ALLOW_PAGE_LOCKS = ON, 
	   OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Продавцы]  
WITH CHECK ADD  
CONSTRAINT [FK_Продавцы_Торговая точка] FOREIGN KEY([ID_торговой_точки])
REFERENCES [dbo].[Торговая точка] ([ID])
GO
ALTER TABLE [dbo].[Продавцы] CHECK CONSTRAINT [FK_Продавцы_Торговая точка]
GO
ALTER TABLE [dbo].[Продавцы]  
WITH CHECK ADD  
CONSTRAINT [CHK_Продавцы_Пол] CHECK  (([Пол]='Ж' OR [Пол]='М'))
GO
ALTER TABLE [dbo].[Продавцы] CHECK CONSTRAINT [CHK_Продавцы_Пол]
GO