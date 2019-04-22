SELECT 
    [users].[Id] AS [User_Id],
	[users].[Email] AS [User_Email],
	[roles].[Name] AS [Role_Name],
    [roles].[Id] AS [Role_Id]
FROM 
	[AspNetUsers] AS [users]

INNER JOIN 
	 [AspNetUserRoles] AS [usersXroles]
		ON [users].[Id] = [usersXroles].[UserId]

INNER JOIN
	[AspNetRoles] AS [roles]
		ON [usersXroles].[RoleId] = [roles].[Id]
