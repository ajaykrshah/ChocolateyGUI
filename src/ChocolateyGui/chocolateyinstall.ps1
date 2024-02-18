$packageName = $env:ChocolateyPackageName

$toolsPath = (Split-Path -parent $MyInvocation.MyCommand.Definition)

# Check for MSI file
$msiFile = Join-Path $toolsPath "$packageName.msi"

# Check for EXE file
$exeFile = Join-Path $toolsPath "$packageName.exe"

if (Test-Path $msiFile -PathType Leaf) {
    $packageArgs = @{
        packageName   = $packageName
        fileType      = 'msi'
        silentArgs    = "/qn /quiet /norestart /qb /passive /qb! /quiet /q /log `"$env:TEMP\chocolatey\$($env:ChocolateyPackageName)\$($env:ChocolateyPackageVersion)\$packageName.MsiInstall.log`""  # Additional silent arguments for MSI
        file          = $msiFile
        validExitCodes= @(0)
    }

    Install-ChocolateyPackage @packageArgs
} elseif (Test-Path $exeFile -PathType Leaf) {
    $packageArgs = @{
        packageName   = $packageName
        fileType      = 'exe'
        silentArgs    = "/S /s /quiet /qn /verysilent /suppressmsgboxes /log `"$env:TEMP\chocolatey\$($env:ChocolateyPackageName)\$($env:ChocolateyPackageVersion)\$packageName.MsiInstall.log`""  # Additional silent arguments for EXE
        file          = $exeFile
        validExitCodes= @(0)
    }

    Install-ChocolateyPackage @packageArgs
} else {
    Write-Output ("$packageName exe or msi file not found.")
}
