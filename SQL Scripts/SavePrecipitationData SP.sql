USE [JBA]
GO

/****** Object:  StoredProcedure [dbo].[SavePrecipitationData]    Script Date: 09/09/2021 16:44:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Robert Silver
-- Create date: 09-Sep-2021
-- Description:	Saves the precipitation data to the DB
-- =============================================
CREATE PROCEDURE [dbo].[SavePrecipitationData]
	@rainData	PrecipitationDataType	READONLY
AS
BEGIN
	SET NOCOUNT ON;

	 BEGIN TRAN
		BEGIN TRY
			MERGE PrecipitationData AS [targetTable]
				USING	(
							SELECT
								Xref,
								Yref,
								[Date],
								[Value]
							FROM @rainData
						) AS [sourceTable]
				ON	targetTable.Xref = sourceTable.Xref 
					AND targetTable.Yref = sourceTable.Yref
					AND targetTable.[Date] = sourceTable.[Date]
				WHEN MATCHED THEN
					UPDATE
						SET
							targetTable.[Value] = sourceTable.[Value]
				WHEN NOT MATCHED THEN 
					INSERT
						(
							Xref,
							Yref,
							[Date],
							[Value]
						)
					VALUES
						(
							sourceTable.Xref,
							sourceTable.Yref,
							sourceTable.[Date],
							sourceTable.[Value]
						);
			
			COMMIT
			
			SELECT
				1	[IsSaved]
		END TRY
		BEGIN CATCH

			ROLLBACK

			SELECT
				0				[IsSaved], 
				ERROR_MESSAGE()
		END CATCH
END
GO


