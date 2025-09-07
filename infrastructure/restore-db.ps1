param(
    [Parameter(Mandatory = $true)] [string] $BackupFile,
    [Parameter(Mandatory = $true)] [string] $DbName,
    [string] $SaPassword = "Your_strong_password123!",
    [string] $ContainerName = "issue-tracking-sql"
)

$ErrorActionPreference = "Stop"

# Resolve paths
$hostBackupPath = Join-Path -Path $PSScriptRoot -ChildPath "sql-backup\$BackupFile"
if (-not (Test-Path -LiteralPath $hostBackupPath)) {
    Write-Error "Backup file not found at $hostBackupPath. Place it under infrastructure/sql-backup/."
}

$containerBackupPath = "/var/opt/mssql/backup/$BackupFile"

# Ensure container is running
Write-Host "Ensuring container '$ContainerName' is running..."
try {
    $state = (docker inspect -f '{{.State.Status}}' $ContainerName 2>$null)
} catch {
    $state = $null
}
if (-not $state) {
    Write-Error "Container '$ContainerName' not found. Start it with: docker compose up -d sql"
}
if ($state -ne "running") {
    Write-Host "Starting container '$ContainerName'..."
    docker start $ContainerName | Out-Null
}

# Build T-SQL to auto-detect logical names and restore
$tsql = @"
DECLARE @BackupFile NVARCHAR(4000) = N'$containerBackupPath';
DECLARE @DbName SYSNAME = N'$DbName';
DECLARE @DataLogical NVARCHAR(128);
DECLARE @LogLogical NVARCHAR(128);

DECLARE @files TABLE (
  LogicalName NVARCHAR(128), PhysicalName NVARCHAR(260), Type NVARCHAR(1), FileGroupName NVARCHAR(128),
  Size BIGINT, MaxSize BIGINT, FileId INT, CreateLSN NUMERIC(25,0), DropLSN NUMERIC(25,0),
  UniqueId UNIQUEIDENTIFIER, ReadOnlyLSN NUMERIC(25,0), ReadWriteLSN NUMERIC(25,0),
  BackupSizeInBytes BIGINT, SourceBlockSize INT, FileGroupId INT, LogGroupGUID UNIQUEIDENTIFIER,
  DifferentialBaseLSN NUMERIC(25,0), DifferentialBaseGUID UNIQUEIDENTIFIER, IsReadOnly BIT, IsPresent BIT, TDEThumbprint VARBINARY(32)
);
INSERT INTO @files
EXEC('RESTORE FILELISTONLY FROM DISK = ''' + @BackupFile + '''');

SELECT @DataLogical = LogicalName FROM @files WHERE Type = 'D' ORDER BY FileId;
SELECT @LogLogical  = LogicalName FROM @files WHERE Type = 'L' ORDER BY FileId;

IF @DataLogical IS NULL OR @LogLogical IS NULL
BEGIN
  RAISERROR('Could not detect logical file names from backup.', 16, 1);
  RETURN;
END

DECLARE @DataPath NVARCHAR(260) = '/var/opt/mssql/data/' + @DbName + '.mdf';
DECLARE @LogPath  NVARCHAR(260) = '/var/opt/mssql/data/' + @DbName + '_log.ldf';

DECLARE @sql NVARCHAR(MAX) = N'RESTORE DATABASE [' + @DbName + '] FROM DISK = ''' + @BackupFile + ''' WITH ' +
  'MOVE ''' + @DataLogical + ''' TO ''' + @DataPath + ''', ' +
  'MOVE ''' + @LogLogical  + ''' TO ''' + @LogPath  + ''', REPLACE, RECOVERY';

PRINT @sql;
EXEC(@sql);
"@

# Collapse newlines to avoid quoting issues with -Q
$tsqlOneLine = ($tsql -replace "`r?`n", " ")

Write-Host "Restoring database '$DbName' from '$BackupFile'..."
$cmd = @(
  "exec",
  "-i", $ContainerName,
  "/opt/mssql-tools18/bin/sqlcmd",
  "-S", "localhost",
  "-U", "sa",
  "-P", $SaPassword,
  "-C",
  "-b",
  "-Q", $tsqlOneLine
)

$process = Start-Process -FilePath "docker" -ArgumentList $cmd -NoNewWindow -PassThru -Wait
if ($process.ExitCode -ne 0) {
    throw "Restore failed. Check container logs and parameters."
}

Write-Host "Restore completed. Database '$DbName' is ready." -ForegroundColor Green

