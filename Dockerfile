# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências
COPY ["MecanicaOS/API/API.csproj", "MecanicaOS/API/"]
COPY ["MecanicaOS/Adapters/Adapters.csproj", "MecanicaOS/Adapters/"]
COPY ["MecanicaOS/Infraestrutura/Infraestrutura.csproj", "MecanicaOS/Infraestrutura/"]
RUN dotnet restore "MecanicaOS/API/API.csproj"

# Copia todo o código-fonte
COPY . .

WORKDIR "/src/MecanicaOS/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y ca-certificates

# Datadog Serverless Init + .NET Tracer
COPY --from=datadog/serverless-init:1 /datadog-init /app/datadog-init
COPY --from=datadog/dd-lib-dotnet-init /datadog-init/monitoring-home/ /dd_tracer/dotnet/

# Datadog config
ENV DD_SITE="datadoghq.com"
ENV DD_HOSTNAME="mecanicaos-api"
ENV DD_ENV="staging"
ENV DD_SERVICE="mecanicaos-api"
ENV DD_VERSION="1.0.0"
ENV DD_LOGS_INJECTION=true
ENV DD_APPSEC_ENABLED=true
ENV DD_IAST_ENABLED=true
ENV DD_APPSEC_SCA_ENABLED=true

COPY --from=build /app/publish .
COPY --from=build /src/MecanicaOS/API/Templates ./Templates

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["/app/datadog-init"]
CMD ["dotnet", "API.dll"]