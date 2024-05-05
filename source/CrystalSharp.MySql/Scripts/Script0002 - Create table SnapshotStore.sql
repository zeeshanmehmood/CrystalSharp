CREATE TABLE IF NOT EXISTS `$DbSnapshotStoreTable$` (
`SnapshotId` CHAR(36) NOT NULL,
`SnapshotAssembly` VARCHAR(1000) NOT NULL,
`SnapshotVersion` BIGINT(20) NOT NULL,
`StreamName` VARCHAR(100) NOT NULL,
`EntityStatus` TINYINT(4) NOT NULL,
`CreatedOn` DATETIME NOT NULL,
`ModifiedOn` DATETIME NULL DEFAULT NULL,
`Data` VARCHAR(14000) NOT NULL
)
COLLATE='utf8mb4_unicode_520_ci'
ENGINE=MyISAM
;

SELECT IF (
    EXISTS(
        SELECT DISTINCT index_name from INFORMATION_SCHEMA.STATISTICS 
        WHERE table_schema = DATABASE() 
        AND table_name = '$DbSnapshotStoreTable$' AND index_name LIKE 'idx_$DbSnapshotStoreTable$_streamname_snapshotversion'
    )
    ,'SELECT ''INDEX idx_$DbSnapshotStoreTable$_streamname_snapshotversion EXISTS'' AS _______;'
    ,'CREATE UNIQUE INDEX idx_$DbSnapshotStoreTable$_streamname_snapshotversion on $DbSnapshotStoreTable$(StreamName,SnapshotVersion)') INTO @IndexScript;
PREPARE stmt FROM @IndexScript;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;