USE [Торговая организация]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Заказы поставщикам](
	[ID] [int] NOT NULL,
	[Дата_заказа] [date] NOT NULL,
	[ID_торговой_точки] [int] NOT NULL,
	[ID_поставщика] [int] NOT NULL,
	[Название_товара] [nvarchar](100) NOT NULL,
	[Количество] [nvarchar](100) NOT NULL,
	[Цена] [numeric](16, 2) NOT NULL,
 CONSTRAINT [PK_Заказы поставщикам] PRIMARY KEY CLUSTERED 
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
ALTER TABLE [dbo].[Заказы поставщикам] ADD  

CONSTRAINT [DF_Заказы_поставщикам_Дата_заказа]  
DEFAULT (getdate()) FOR [Дата_заказа]
GO

ALTER TABLE [dbo].[Заказы поставщикам]  
WITH CHECK ADD  
CONSTRAINT [FK_Заказы поставщикам_Поставщики] FOREIGN KEY([ID_поставщика])
REFERENCES [dbo].[Поставщики] ([ID])
GO
ALTER TABLE [dbo].[Заказы поставщикам] 
CHECK CONSTRAINT [FK_Заказы поставщикам_Поставщики]
GO
ALTER TABLE [dbo].[Заказы поставщикам]  
WITH CHECK ADD  
CONSTRAINT [FK_Заказы поставщикам_Торговая точка] FOREIGN KEY([ID_торговой_точки])
REFERENCES [dbo].[Торговая точка] ([ID])
GO
ALTER TABLE [dbo].[Заказы поставщикам] CHECK CONSTRAINT [FK_Заказы поставщикам_Торговая точка]
GO


