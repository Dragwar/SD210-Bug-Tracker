SELECT
	[T].[Title],
	[T].[Description],
	[T].[DateCreated],
	[T].[DateUpdated],
	[TP].[Priority],
	[TS].[Status],
	[TT].[Type],
	[Project].[Name] AS [Project Name],
	[Author].[DisplayName] AS [Author DisplayName],
	[Author].[Email] AS [Author Email],
	[AssignedUser].[DisplayName] AS [AssignedUser DisplayName],
	[AssignedUser].[Email] AS [AssignedUser Email]
FROM 
	[Tickets] AS [T]

INNER JOIN 
	 [TicketPriorities] AS [TP]
		ON [T].[PriorityId] = [TP].[Id]

INNER JOIN
	[TicketStatuses] AS [TS] 
		ON [TS].[Id] = [T].[StatusId]

INNER JOIN
	[TicketTypes] AS [TT] 
		ON [TT].[Id] = [T].[TypeId]

INNER JOIN
	[Projects] AS [Project] 
		ON [Project].[Id] = [T].[ProjectId]
		
INNER JOIN
	[AspNetUsers] AS [Author] 
		ON [Author].[Id] = [T].[AuthorId]
		
INNER JOIN
	[AspNetUsers] AS [AssignedUser] 
		ON [AssignedUser].[Id] = [T].[AssignedUserId]