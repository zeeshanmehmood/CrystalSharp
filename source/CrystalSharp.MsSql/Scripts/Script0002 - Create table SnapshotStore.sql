IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = N'$DbSchema$' AND table_name = N'$DbSnapshotStoreTable$')
BEGIN
	CREATE TABLE [$DbSchema$].[$DbSnapshotStoreTable$](
		[SnapshotId] [uniqueidentifier] NOT NULL,
		[SnapshotAssembly] [nvarchar](max) NOT NULL,
		[SnapshotVersion] [bigint] NOT NULL,
		[StreamName] [nvarchar](100) NOT NULL,
		[EntityStatus] [tinyint] NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		[ModifiedOn] [datetime2](7) NULL,
		[Data] [nvarchar](max) NOT NULL
	)

	CREATE UNIQUE NONCLUSTERED INDEX [IX_$DbSnapshotStoreTable$_StreamName_SnapshotVersion] ON [$DbSchema$].[$DbSnapshotStoreTable$]
	(
		[StreamName] ASC,
		[SnapshotVersion] ASC
	)
END