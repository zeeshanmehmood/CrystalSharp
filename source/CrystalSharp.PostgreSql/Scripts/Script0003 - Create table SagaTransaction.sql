CREATE TABLE IF NOT EXISTS "$DbSchema$"."$DbSagaStoreTable$"
(
"Id" text NOT NULL,
"CorrelationId" text NOT NULL,
"StartedBy" text NOT NULL,
"Step" text NOT NULL,
"State" integer NOT NULL,
"ErrorTrail" text,
"CreatedOn" timestamp without time zone NOT NULL,
"ModifiedOn" timestamp without time zone
);

CREATE UNIQUE INDEX IF NOT EXISTS "idx_$DbSagaStoreTable$_correlationid" ON "$DbSchema$"."$DbSagaStoreTable$"
(
"CorrelationId"
);