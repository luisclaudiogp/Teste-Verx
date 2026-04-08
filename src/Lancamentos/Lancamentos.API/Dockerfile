FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todos os arquivos de projeto para restore (com novos caminhos)
COPY ["src/Lancamentos/Lancamentos.API/Lancamentos.API.csproj", "src/Lancamentos/Lancamentos.API/"]
COPY ["src/Lancamentos/Lancamentos.Application/Lancamentos.Application.csproj", "src/Lancamentos/Lancamentos.Application/"]
COPY ["src/Lancamentos/Lancamentos.Domain/Lancamentos.Domain.csproj", "src/Lancamentos/Lancamentos.Domain/"]
COPY ["src/Lancamentos/Lancamentos.Infrastructure/Lancamentos.Infrastructure.csproj", "src/Lancamentos/Lancamentos.Infrastructure/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]

RUN dotnet restore "src/Lancamentos/Lancamentos.API/Lancamentos.API.csproj"

# Copiar todo o resto e buildar
COPY . .
WORKDIR "/src/src/Lancamentos/Lancamentos.API"
RUN dotnet build "Lancamentos.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lancamentos.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lancamentos.API.dll"]
