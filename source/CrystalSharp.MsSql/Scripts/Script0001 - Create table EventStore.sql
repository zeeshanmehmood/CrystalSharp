IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = N'$DbSchema$' AND table_name = N'$DbEventStoreTable$')
BEGIN
	CREATE TABLE [$DbSchema$].[$DbEventStoreTable$](
		[Id] [uniqueidentifier] NOT NULL,
		[GlobalSequence] [bigint] IDENTITY(1,1) NOT NULL,
		[StreamId] [uniqueidentifier] NOT NULL,
		[StreamName] [nvarchar](100) NOT NULL,
		[Sequence] [bigint] NOT NULL,
		[EventId] [uniqueidentifier] NOT NULL,
		[EventType] [nvarchar](max) NOT NULL,
		[EventAssembly] [nvarchar](max) NOT NULL,
		[EntityStatus] [tinyint] NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		[ModifiedOn] [datetime2](7) NULL,
		[OccuredOn] [datetime2](7) NOT NULL,
		[Version] [bigint] NOT NULL,
		[Data] [nvarchar](max) NOT NULL,
		CONSTRAINT [PK_$DbEventStoreTable$] PRIMARY KEY CLUSTERED
		(
				[GlobalSequence] ASC
		)
	)

	CREATE UNIQUE NONCLUSTERED INDEX [IX_$DbEventStoreTable$_StreamName_Version] ON [$DbSchema$].[$DbEventStoreTable$]
	(
		[StreamName] ASC,
		[Version] ASC
	)
END