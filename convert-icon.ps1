Add-Type -AssemblyName System.Drawing

$pngPath = "D:\Projects\Lanceur_Claude\src\ClaudeLauncher\rocket-color.png"
$icoPath = "D:\Projects\Lanceur_Claude\src\ClaudeLauncher\icon.ico"

$png = [System.Drawing.Image]::FromFile($pngPath)
$bitmap = New-Object System.Drawing.Bitmap $png, 256, 256
$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())

$fs = [System.IO.FileStream]::new($icoPath, [System.IO.FileMode]::Create)
$icon.Save($fs)
$fs.Close()

$bitmap.Dispose()
$png.Dispose()

Write-Host "ICO created: $icoPath"
