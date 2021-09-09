USE [JBA]
GO

/****** Object:  StoredProcedure [dbo].[CreateTablesIfNecessary]    Script Date: 09/09/2021 16:44:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Robert Silver
-- Create date: 09-Sep-2021
-- Description:	If the tables do not exist, then the code creates them.
-- =============================================
CREATE PROCEDURE [dbo].[CreateTablesIfNecessary]
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID ( N'PrecipitationData', N'U' ) IS NULL   
		CREATE TABLE [dbo].[PrecipitationData](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[Xref] [int] NOT NULL,
			[Yref] [int] NOT NULL,
			[Date] [date] NOT NULL,
			[Value] [nchar](10) NOT NULL
		) ON [PRIMARY]

	IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'PrecipitationDataType')
		CREATE TYPE [dbo].[PrecipitationDataType] AS TABLE(
			Xref int NOT NULL,
			YRef int NOT NULL,
			[Date] Date NOT NULL,
			[Value] int not null
)
END
GO


