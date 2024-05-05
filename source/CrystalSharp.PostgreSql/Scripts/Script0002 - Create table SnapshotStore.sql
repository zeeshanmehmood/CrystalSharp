CREATE TABLE IF NOT EXISTS "$DbSchema$"."$DbSnapshotStoreTable$"
(
"SnapshotId" uuid NOT NULL,
"SnapshotAssembly" text NOT NULL,
"SnapshotVersion" bigint NOT NULL,
"StreamName" text NOT NULL,
"EntityStatus" integer NOT NULL,
"CreatedOn" timestamp without time zone NOT NULL,
"ModifiedOn" timestamp without time zone,
"Data" text NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "idx_$DbSnapshotStoreTable$_streamname_snapshotversion" ON "$DbSchema$"."$DbSnapshotStoreTable$"
(
"StreamName",
"SnapshotVersion"
);