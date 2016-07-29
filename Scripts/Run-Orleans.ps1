# Ensure the Azure Storage Emulator is running.

# Check if storage is named "WAStorageEmulator or AzureStorageEmulator"
$waStorageEmulator = Get-Process "WAStorageEmulator" -ErrorAction Ignore
$storageEmulator = Get-Process "AzureStorageEmulator" -ErrorAction Ignore

# Start the emulator.
if ($storageEmulator -eq $null -and $waStorageEmulator -eq $null) {
    $path = "C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\WAStorageEmulator.exe"
	$testPath = test-path $path
	if(-not $testPath)
	{
	    $path = "C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe"
	}
	& $path start
}

# Start Orleans.

$orleansPath = "..\OrleansSilo\bin\Debug\OrleansSilo.exe"

$orleansTestPath = test-path $orleansPath
if(-not $orleansTestPath)
{
	$orleansTestPath = "..\OrleansSilo\bin\Debug\OrleansSilo.exe"
}

& $orleansPath