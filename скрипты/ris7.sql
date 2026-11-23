USE [Торговая организация]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Торговая точка](
	[ID] [int] NOT NULL,
	[Тип] [nvarchar](100) NOT NULL,
	[ID_Торговой_организации] [int] NOT NULL,
	[Адрес] [nvarchar](100) NOT NULL,
	[ФИО_заведующего] [nvarchar](150) NOT NULL,
 CONSTRAINT [PK_Торговая точка] PRIMARY KEY CLUSTERED 
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
ALTER TABLE [dbo].[Торговая точка]  
WITH CHECK ADD  
CONSTRAINT [FK_Торговая точка_Торговая организация] FOREIGN KEY([ID_Торговой_организации])
REFERENCES [dbo].[Торговая организация] ([ID])
GO
ALTER TABLE [dbo].[Торговая точка] CHECK CONSTRAINT [FK_Торговая точка_Торговая организация]
GO
ALTER TABLE [dbo].[Торговая точка]  
WITH CHECK ADD  
CONSTRAINT [CHK_Торговая_точка_Тип] CHECK  (([Тип]='лоток' OR [Тип]='киоск' OR [Тип]='магазин' OR [Тип]='универмаг'))
GO
ALTER TABLE [dbo].[Торговая точка] CHECK CONSTRAINT [CHK_Торговая_точка_Тип]
GO


