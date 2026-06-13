# 09072318206_ANALISISB2026FINAL

Proyecto de prototipo de API REST para la gestión de incidentes de red de NetGuard GT, desarrollado en C# con ASP.NET Core 8, EF Core + SQLite y pruebas xUnit.

## Objetivo

Implementar un prototipo funcional para:
- registrar incidentes,
- asignar técnicos,
- cambiar estados,
- liberar reasignaciones,
- auditar historial,
- y generar reportes de incidentes.

## Reglas de negocio cubiertas

1. Tiempo máximo de resolución por severidad.
2. Máximo 3 incidentes activos por técnico.
3. Transiciones de estado controladas en una sola dirección.
4. Reasignación y liberación del incidente.
5. Escalado automático de incidentes críticos/urgentes sin atención en 2 horas.
6. Validación de especialidad del técnico por tipo de incidente.
7. Historial de cambios de estado.
8. Reportes de incidentes.

## Estructura del proyecto

- NetGuardGT.Api: API REST y lógica de negocio.
- NetGuardGT.Tests: pruebas unitarias con xUnit.
- docs/UserStories.md: 12 historias de usuario.
- docs/IAUsage.md: informe de uso de IA.
- docs/DiagramaFlujo.md: diagrama de flujo / secuencia.

## Requisitos

- .NET SDK 8.0
- Visual Studio Code / dotnet CLI

## Cómo ejecutar localmente

1. Restaurar dependencias:
   dotnet restore NetGuardGT.sln
2. Ejecutar pruebas:
   dotnet test NetGuardGT.sln
3. Iniciar la API:
   dotnet run --project NetGuardGT.Api
4. Abrir Swagger:
   http://localhost:5219/swagger

## Endpoints principales

- GET /api/incidents
- GET /api/incidents/{id}
- POST /api/incidents
- PUT /api/incidents/{id}/assign?technicianId=1
- PUT /api/incidents/{id}/status?status=InProgress
- POST /api/incidents/{id}/release
- GET /api/incidents/{id}/history
- GET /api/reports/summary

## Despliegue en Render

El archivo render.yaml ya está preparado para desplegar la API como servicio Web.

Pasos exactos para terminarlo en Render:
1. Abre https://dashboard.render.com/ y entra a New > Web Service.
2. Conecta tu repositorio GitHub: EnmaRamirez/09072318206ANALISISB2026FINAL.
3. Render debería detectar el archivo render.yaml automáticamente.
4. Si no lo detecta, usa estos valores manuales:
   - Build Command: dotnet publish NetGuardGT.Api/NetGuardGT.Api.csproj -c Release -o publish
   - Start Command: dotnet ./publish/NetGuardGT.Api.dll
5. Confirma el despliegue y espera a que termine el build.

Nota: la URL final quedará disponible en el panel de Render una vez termine el despliegue.

## Prueba rápida con Swagger

Después de iniciar la API, use Swagger para:
- crear un incidente,
- asignar un técnico,
- actualizar el estado,
- liberar el incidente,
- consultar reportes.

## Nota de evaluación

La solución está lista para presentar como prototipo funcional y puede ampliarse con autenticación, monitoreo y persistencia en producción.
