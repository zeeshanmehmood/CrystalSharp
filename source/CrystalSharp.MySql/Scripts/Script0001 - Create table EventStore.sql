CREATE TABLE IF NOT EXISTS `$DbEventStoreTable$` (
`Id` CHAR(36) NOT NULL,
`GlobalSequence` BIGINT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY,
`StreamId` CHAR(36) NOT NULL,
`StreamName` VARCHAR(100) NOT NULL,
`Sequence` BIGINT(20) NOT NULL,
`EventId` CHAR(36) NOT NULL,
`EventType` VARCHAR(1000) NOT NULL,
`EventAssembly` VARCHAR(1000) NOT NULL,
`EntityStatus` TINYINT(4) NOT NULL,
`CreatedOn` DATETIME NOT NULL,
`ModifiedOn` DATETIME NULL DEFAULT NULL,
`OccuredOn` DATETIME NOT NULL,
`Version` BIGINT(20) NOT NULL,
`Data` VARCHAR(14000) NOT NULL
)
COLLATE='utf8mb4_unicode_520_ci'
ENGINE=MyISAM
;

SELECT IF (
    EXISTS(
        SELECT DISTINCT index_name from INFORMATION_SCHEMA.STATISTICS 
        WHERE table_schema = DATABASE() 
        AND table_name = '$DbEventStoreTable$' AND index_name LIKE 'idx_$DbEventStoreTable$_streamname_version'
    )
    ,'SELECT ''INDEX idx_$DbEventStoreTable$_streamname_version EXISTS'' AS _______;'
    ,'CREATE UNIQUE INDEX idx_$DbEventStoreTable$_streamname_version on $DbEventStoreTable$(StreamName,Version)') INTO @IndexScript;
PREPARE stmt FROM @IndexScript;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;