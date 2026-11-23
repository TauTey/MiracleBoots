USE [Торговая организация]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Поставщики](
	[ID] [int] NOT NULL,
	[Название] [nvarchar](100) NOT NULL,
	[Тип_деятельности] [nvarchar](100) NOT NULL,
	[Страна] [nvarchar](50) NOT NULL,
	[Город] [nvarchar](100) NOT NULL,
	[Адрес] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Поставщики] PRIMARY KEY CLUSTERED 
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


