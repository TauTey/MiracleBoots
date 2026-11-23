USE [Торговая организация]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Торговая организация](
	[ID] [int] NOT NULL,
	[Название] [nvarchar](100) NOT NULL,
	[Адрес] [nvarchar](100) NOT NULL,
	[ФИО_директора] [nvarchar](150) NOT NULL,
	[Налоговый_номер] [int] NOT NULL,
 CONSTRAINT [PK_Торговая организация] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, 
	   STATISTICS_NORECOMPUTE = OFF, 
	   IGNORE_DUP_KEY = OFF, 
	   ALLOW_ROW_LOCKS = ON, 
	   ALLOW_PAGE_LOCKS = ON, 
	   OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Торговая_организация_Налоговый_номер] UNIQUE NONCLUSTERED 
(
	[Налоговый_номер] ASC
)WITH (PAD_INDEX = OFF, 
	   STATISTICS_NORECOMPUTE = OFF, 
	   IGNORE_DUP_KEY = OFF, 
	   ALLOW_ROW_LOCKS = ON, 
	   ALLOW_PAGE_LOCKS = ON, 
	   OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO