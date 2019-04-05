SELECT 
    [users].[Id] AS [User_Id]
	, [users].[DisplayName] AS [User_DisplayName]
	, [users].[Email] AS [User_Email]
	, [users].[EmailConfirmed] AS [User_Is_Email_Confirmed]
	--, [users].[UserName] AS [User_UserName]
FROM
	[AspNetUsers] AS [users]