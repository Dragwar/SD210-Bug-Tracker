SELECT
	[T].[Title],
	[T].[Description],
	--[T].[DateCreated],
	--[T].[DateUpdated],
	[TP].[Priority],
	[TS].[Status],
	[TT].[Type],
	[TA].[Description] AS [Attachment Description],
	[TC].[Comment] AS [Comment],
	[Project].[Name] AS [Project Name],
	[Author].[Email] AS [Author Email],
	[AssignedUser].[DisplayName] AS [AssignedUser DisplayName],
	[AssignedUser].[Email] AS [AssignedUser Email]
FROM 
	[Tickets] AS [T]

LEFT JOIN 
	 [TicketPriorities] AS [TP]
		ON [T].[PriorityId] = [TP].[Id]

LEFT JOIN
	[TicketStatuses] AS [TS] 
		ON [TS].[Id] = [T].[StatusId]

LEFT JOIN
	[TicketTypes] AS [TT] 
		ON [TT].[Id] = [T].[TypeId]

LEFT JOIN
	[TicketAttachments] AS [TA] 
		ON [TA].[TicketId] = [T].[Id]

LEFT JOIN
	[TicketComments] AS [TC] 
		ON [TC].[TicketId] = [T].[Id]
LEFT JOIN
	[Projects] AS [Project] 
		ON [Project].[Id] = [T].[ProjectId]
		
LEFT JOIN
	[AspNetUsers] AS [Author] 
		ON [Author].[Id] = [T].[AuthorId]
		
LEFT JOIN
	[AspNetUsers] AS [AssignedUser] 
		ON [AssignedUser].[Id] = [T].[AssignedUserId]
