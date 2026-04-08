$paths = @(
    "src/Lancamentos/Lancamentos.UnitTests/Consumers",
    "src/Lancamentos/Lancamentos.UnitTests/UnitTest1.cs"
)

foreach ($path in $paths) {
    if (Test-Path $path) {
        Write-Host "Removendo $path"
        Remove-Item -Recurse -Force $path
    }
}
