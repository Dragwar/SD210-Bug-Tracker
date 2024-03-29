﻿SELECT 
    [users].[Id] AS [User_Id],
	[users].[Email] AS [User_Email],
	[projects].[Name] AS [Project_Name],
    [projects].[Id] AS [Project_Id]
FROM 
	[AspNetUsers] AS [users]

INNER JOIN 
	[ApplicationUserProjects] AS [usersXprojects]
		ON [users].[Id] = [usersXprojects].[ApplicationUser_Id]

INNER JOIN
	[Projects] AS [projects] 
		ON [usersXprojects].[Project_Id] = [projects].[Id]