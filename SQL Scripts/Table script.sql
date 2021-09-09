USE [JBA]
GO

/****** Object:  Table [dbo].[PrecipitationData]    Script Date: 09/09/2021 16:43:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PrecipitationData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Xref] [int] NOT NULL,
	[Yref] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Value] [nchar](10) NOT NULL
) ON [PRIMARY]
GO


