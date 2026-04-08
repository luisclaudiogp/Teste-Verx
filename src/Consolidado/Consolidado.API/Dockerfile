FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todos os arquivos de projeto para restore (com novos caminhos)
COPY ["src/Consolidado/Consolidado.API/Consolidado.API.csproj", "src/Consolidado/Consolidado.API/"]
COPY ["src/Consolidado/Consolidado.Application/Consolidado.Application.csproj", "src/Consolidado/Consolidado.Application/"]
COPY ["src/Consolidado/Consolidado.Domain/Consolidado.Domain.csproj", "src/Consolidado/Consolidado.Domain/"]
COPY ["src/Consolidado/Consolidado.Infrastructure/Consolidado.Infrastructure.csproj", "src/Consolidado/Consolidado.Infrastructure/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]

RUN dotnet restore "src/Consolidado/Consolidado.API/Consolidado.API.csproj"

# Copiar apenas os diretórios necessários e buildar
COPY src/Consolidado/ src/Consolidado/
COPY src/Shared.Contracts/ src/Shared.Contracts/
WORKDIR "/src/src/Consolidado/Consolidado.API"
RUN dotnet build "Consolidado.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Consolidado.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "Consolidado.API.dll"]
