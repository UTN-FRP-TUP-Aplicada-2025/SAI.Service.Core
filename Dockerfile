# Dockerfile — SAI.Service.Core
#
# Build multi-stage: compila en la imagen del SDK y corre en el runtime minimo.
# El host que construye NO necesita el SDK de .NET: alcanza con Docker.
#
# Contexto de build: raiz del repositorio SAI.Service.Core/
#
#   docker build -t sai-service-core:latest .
#
# Tambien admite contexto remoto (Docker clona el repo y busca este archivo en
# la raiz del arbol clonado):
#
#   docker build -t sai-service-core:latest \
#     https://github.com/UTN-FRP-TUP-Aplicada-2025/SAI.Service.Core.git#main
#
# La base SQLite (append-only) y el keyring de DataProtection se persisten en
# volumenes: /app/data y /keys. Ver ADR-18 (migraciones al arranque) y la nota
# "DataProtection" de appsettings.json.

# ── Etapa 1: build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Directory.Build.props aplica a TODOS los proyectos (net10.0, Nullable,
# TreatWarningsAsErrors, analizadores). Debe estar presente ANTES del restore
# para que restore y publish sean consistentes: si falta, el obj/ generado queda
# inconsistente y el publish puede no emitir los static web assets del framework
# Blazor (wwwroot/_framework/blazor.web.js), rompiendo la interactividad.
COPY ["Directory.Build.props", "./"]

# Copiar solo los .csproj de las cinco capas antes del codigo, para que la capa
# de restore quede cacheada y no se rehaga en cada cambio de fuente.
# Restaurar el proyecto Web arrastra las cuatro capas por ProjectReference
# (Web → Api + Infrastructure → Application → Domain); los tests no se compilan
# en la imagen (corren en CI, ver .github/workflows/ci.yml).
COPY ["src/SAI.Service.Core/SAI.Service.Core.Domain/SAI.Service.Core.Domain.csproj", \
      "src/SAI.Service.Core/SAI.Service.Core.Domain/"]
COPY ["src/SAI.Service.Core/SAI.Service.Core.Application/SAI.Service.Core.Application.csproj", \
      "src/SAI.Service.Core/SAI.Service.Core.Application/"]
COPY ["src/SAI.Service.Core/SAI.Service.Core.Infrastructure/SAI.Service.Core.Infrastructure.csproj", \
      "src/SAI.Service.Core/SAI.Service.Core.Infrastructure/"]
COPY ["src/SAI.Service.Core/SAI.Service.Core.Api/SAI.Service.Core.Api.csproj", \
      "src/SAI.Service.Core/SAI.Service.Core.Api/"]
COPY ["src/SAI.Service.Core/SAI.Service.Core.Web/SAI.Service.Core.Web.csproj", \
      "src/SAI.Service.Core/SAI.Service.Core.Web/"]
RUN dotnet restore "src/SAI.Service.Core/SAI.Service.Core.Web/SAI.Service.Core.Web.csproj"

# Copiar el resto del codigo y publicar. Se deja que publish rehaga el restore
# (sin --no-restore) para que los targets de static web assets de Blazor corran
# con el arbol completo presente.
COPY . .
RUN dotnet publish "src/SAI.Service.Core/SAI.Service.Core.Web/SAI.Service.Core.Web.csproj" \
    -c Release \
    -o /app/publish

# ── Etapa 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Version de build: se inyecta desde el pipeline con el tag o el SHA.
#   docker build --build-arg BUILD_VERSION=0.1.0 ...
ARG BUILD_VERSION=dev
ENV BUILD_VERSION=$BUILD_VERSION

# Usuario no-root (minimo privilegio).
RUN groupadd --system --gid 1001 appgroup \
 && useradd  --system --uid 1001 --gid 1001 --no-create-home appuser

# /app/data  → base SQLite (append-only, ADR-18: se migra al arranque)
# /keys      → keyring de DataProtection. Si no persiste, cada reinicio invalida
#              la cookie de sesion y rompe los tokens antiforgery de los POST SSR.
RUN mkdir -p /app/data /keys \
 && chown appuser:appgroup /app/data /keys

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

VOLUME ["/app/data", "/keys"]

# Defaults del contenedor; se sobrescriben por variables de entorno en el compose.
# NO se define ASPNETCORE_URLS a proposito: el binding lo fija "Kestrel:Endpoints"
# en appsettings.json (http://0.0.0.0:8080) y ASPNETCORE_URLS lo pisaria entero,
# descartando la ranura de TLS de produccion (ADR-20, P-04).
#
# Sai__Adaptador=Simulado es el default seguro; con SAI real se pone "Nut" y se
# apunta Sai__Nut__Host al servidor NUT (ADR-02/ADR-03).
# Jwt__ClaveFirma NO se hornea aca (ADR-20): se inyecta en el despliegue.
ENV ConnectionStrings__Sai="Data Source=/app/data/sai.db" \
    DataProtection__RutaLlaves=/keys \
    Sai__Adaptador=Simulado \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "SAI.Service.Core.Web.dll"]
