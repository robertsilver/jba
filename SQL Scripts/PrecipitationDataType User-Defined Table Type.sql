USE [JBA]
GO

/****** Object:  UserDefinedTableType [dbo].[PrecipitationDataType]    Script Date: 09/09/2021 16:45:10 ******/
CREATE TYPE [dbo].[PrecipitationDataType] AS TABLE(
	[Xref] [int] NOT NULL,
	[YRef] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Value] [int] NOT NULL
)
GO


