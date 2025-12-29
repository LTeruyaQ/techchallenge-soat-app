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

# Download and install the Tracer
RUN apt-get update \
    && apt-get install -y curl dpkg \
    && mkdir -p /opt/datadog \
    && mkdir -p /var/log/datadog \
    && TRACER_VERSION=$(curl -s https://api.github.com/repos/DataDog/dd-trace-dotnet/releases/latest | grep tag_name | cut -d '"' -f 4 | cut -c2-) \
    && curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && dpkg -i ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && rm ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb

# Enable the tracer
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
ENV DD_ENV="staging"
ENV DD_SERVICE="mecanicaos-api"
ENV DD_VERSION="1.0.0"
ENV DD_LOGS_INJECTION=true
ENV DD_APPSEC_ENABLED=true
ENV DD_IAST_ENABLED=true
ENV DD_APPSEC_SCA_ENABLED=true

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Garante que os templates de e-mail estejam presentes
COPY --from=build /src/MecanicaOS/API/Templates ./Templates

# Expondo a porta padrão do Kestrel
EXPOSE 80

# Variável de ambiente para ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "API.dll"]