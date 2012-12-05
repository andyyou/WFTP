If OBJECT_ID('GeneratePath') IS Not NULL
  DROP Procedure GeneratePath
Go
Create procedure GeneratePath
  @FileId int
AS
  Begin
    Declare @Path nvarchar(500)
    Set @Path =
    (
		SELECT '/' + LV1.[ClassName] + '/'
           + LV2.[CompanyName] + '/'
           + LV3.[BranchName] + '/'
           + LV4.[LineName] + '/'
           + LV5.[ClassName] + '/'
           + LV6.[FileName]
			FROM [WFTP].[dbo].[Files] LV6,
				 [WFTP].[dbo].[FileCategorys] LV5,
				 [WFTP].[dbo].[Lv4Lines] LV4,
				 [WFTP].[dbo].[Lv3CustomerBranches] LV3,
				 [WFTP].[dbo].[Lv2Customers] LV2,
				 [WFTP].[dbo].[Lv1Classifications] LV1
			WHERE LV6.[IsDeleted] = 0
				  AND LV6.[FileId] = @FileId
				  AND LV5.[FileCategoryId] = LV6.[FileCategoryId]
				  AND LV4.[LineId] = LV6.[LineId]
				  AND LV3.[BranchId] = LV4.[BranchId]
			      AND LV2.[CompanyId] = LV3.[CompanyId]
				  AND LV1.[ClassifyId] = LV2.[ClassifyId]
    )
    Select @Path AS [Path]
    End
GO
Exec GeneratePath '6'