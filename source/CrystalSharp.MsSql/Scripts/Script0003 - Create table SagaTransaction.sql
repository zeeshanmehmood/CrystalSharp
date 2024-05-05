IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = N'$DbSchema$' AND table_name = N'$DbSagaStoreTable$')
BEGIN
	CREATE TABLE [$DbSchema$].[$DbSagaStoreTable$](
		[Id] [nvarchar](max) NOT NULL,
		[CorrelationId] [nvarchar](100) NOT NULL,
		[StartedBy] [nvarchar](max) NOT NULL,
		[Step] [nvarchar](max) NOT NULL,
		[State] [tinyint] NOT NULL,
		[ErrorTrail] [nvarchar](max) NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		[ModifiedOn] [datetime2](7) NULL
	)

	CREATE UNIQUE NONCLUSTERED INDEX [IX_$DbSagaStoreTable$_CorrelationId] ON [$DbSchema$].[$DbSagaStoreTable$]
	(
		[CorrelationId] ASC
	)
END