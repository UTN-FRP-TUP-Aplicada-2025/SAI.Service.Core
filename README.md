# SAI

## Como correr

Requisito unico del host: Docker (el SDK de .NET 10 vive dentro del Dev Container).

1. **Abrir en el Dev Container**: en VS Code, *Reopen in Container*; o por CLI:

   ```bash
   devcontainer up --workspace-folder .
   ```

2. **Compilar toda la solucion** (Release, cero warnings):

   ```bash
   ./scripts/build-all.sh
   ```

3. **Correr el panel** (el host es el unico proceso del web-monolith):

   ```bash
   ./scripts/run.sh SAI.Service.Core.Web
   # equivalente: ./scripts/run-all.sh
   ```

   Queda escuchando en `http://localhost:8080` y, en Development, `https://localhost:8443`
   (certificado de desarrollo; la primera vez: `dotnet dev-certs https --trust`).
   Endpoint de salud anonimo: `http://localhost:8080/health`.

4. **Depurar**: F5 sobre `SAI.Service.Core.Web` (config *coreclr* de `.vscode/launch.json`).
   La depuracion va por F5, nunca por los scripts.

5. **Pruebas**:

   ```bash
   dotnet test SAI.Service.Core.sln
   ```

> Sprint 0 es andamiaje: la solucion compila, corre y muestra el panel base (menu
> lateral + barra superior + sello de version). Sin logica de negocio todavia.