Write-Host "Iniciando processo de Scan DUPLO do SonarQube..."

# 1. Instalar ferramenta se nao existir
dotnet tool install --global dotnet-sonarscanner

$token = "sqa_8935b2f42b3fd03e95b3ac60d6578f6ca64fa0f3"

# ========================================================
# PROJETO 1: Lançamentos
# ========================================================
Write-Host "`n---> Analisando Microsservico de Lancamentos..."
dotnet sonarscanner begin /k:"Verx-Lancamentos" /n:"API Lancamentos" /d:sonar.host.url="http://localhost:9000" /d:sonar.login=$token /d:sonar.cs.opencover.reportsPaths="src/Lancamentos/Lancamentos.UnitTests/coverage.opencover.xml" /d:sonar.coverage.exclusions="**/Program.cs,**/Properties/**,**/Data/**"
dotnet build src/Lancamentos/Lancamentos.UnitTests/Lancamentos.UnitTests.csproj
dotnet test src/Lancamentos/Lancamentos.UnitTests/Lancamentos.UnitTests.csproj --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="./coverage.opencover.xml"
dotnet sonarscanner end /d:sonar.login=$token

# ========================================================
# PROJETO 2: Consolidado
# ========================================================
Write-Host "`n---> Analisando Microsservico de Consolidado..."
dotnet sonarscanner begin /k:"Verx-Consolidado" /n:"API Consolidado" /d:sonar.host.url="http://localhost:9000" /d:sonar.login=$token /d:sonar.cs.opencover.reportsPaths="src/Consolidado/Consolidado.UnitTests/coverage.opencover.xml" /d:sonar.coverage.exclusions="**/Program.cs,**/Properties/**,**/Data/**"
dotnet build src/Consolidado/Consolidado.UnitTests/Consolidado.UnitTests.csproj
dotnet test src/Consolidado/Consolidado.UnitTests/Consolidado.UnitTests.csproj --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="./coverage.opencover.xml"
dotnet sonarscanner end /d:sonar.login=$token

Write-Host "`nProcesso finalizado duplo! Acesse http://localhost:9000 para ver as duas APIs separadas no dashboard."
