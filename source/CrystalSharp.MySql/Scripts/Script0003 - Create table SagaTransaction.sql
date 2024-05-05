CREATE TABLE IF NOT EXISTS `$DbSagaStoreTable$` (
`Id` VARCHAR(1000) NOT NULL,
`CorrelationId` VARCHAR(100) NOT NULL,
`StartedBy` VARCHAR(100) NOT NULL,
`Step` VARCHAR(100) NOT NULL,
`State` TINYINT(4) NOT NULL,
`ErrorTrail` VARCHAR(5000) DEFAULT NULL,
`CreatedOn` DATETIME NOT NULL,
`ModifiedOn` DATETIME NULL DEFAULT NULL
)
COLLATE='utf8mb4_unicode_520_ci'
ENGINE=MyISAM
;

SELECT IF (
    EXISTS(
        SELECT DISTINCT index_name from INFORMATION_SCHEMA.STATISTICS 
        WHERE table_schema = DATABASE() 
        AND table_name = '$DbSagaStoreTable$' AND index_name LIKE 'idx_$DbSagaStoreTable$_correlationid'
    )
    ,'SELECT ''INDEX idx_$DbSagaStoreTable$_correlationid EXISTS'' AS _______;'
    ,'CREATE UNIQUE INDEX idx_$DbSagaStoreTable$_correlationid on $DbSagaStoreTable$(CorrelationId)') INTO @IndexScript;
PREPARE stmt FROM @IndexScript;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;