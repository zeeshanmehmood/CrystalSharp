CREATE TABLE IF NOT EXISTS "$DbSchema$"."$DbEventStoreTable$"
(
"Id" uuid NOT NULL,
"GlobalSequence" bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 ),
"StreamId" uuid NOT NULL,
"StreamName" text NOT NULL,
"Sequence" bigint NOT NULL,
"EventId" uuid NOT NULL,
"EventType" text NOT NULL,
"EventAssembly" text NOT NULL,
"EntityStatus" integer NOT NULL,
"CreatedOn" timestamp without time zone NOT NULL,
"ModifiedOn" timestamp without time zone,
"OccuredOn" timestamp without time zone NOT NULL,
"Version" bigint NOT NULL,
"Data" text NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "idx_$DbEventStoreTable$_streamname_version" ON "$DbSchema$"."$DbEventStoreTable$"
(
"StreamName",
"Version"
);