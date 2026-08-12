$exe = 'C:\Users\steam\Desktop\123\Minecraft-Resource-Extractor\MinecraftResourceExtractor\bin\Debug\MinecraftResourceExtractor.exe'
$dir = 'C:\Users\steam\Desktop\123\Minecraft-Resource-Extractor\MinecraftResourceExtractor\bin\Debug'
$p = Start-Process -FilePath $exe -WorkingDirectory $dir -PassThru
Start-Sleep -Seconds 5
if ($p.HasExited) {
    Write-Host "Exit code: $($p.ExitCode)"
} else {
    Write-Host "Running - PID: $($p.Id)"
    $p.Kill()
}
