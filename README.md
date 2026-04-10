# ApiMonitor

Servicio REST en .NET 9 que recibe logs de APIs externas, los persiste en SQL Server y envía alertas a Telegram cuando detecta fallos (StatusCode != 200).


## Requisitos previos

- .NET 9 SDK
- SQL Server 
- Bot de Telegram configurado

---

## 1. Configurar appsettings.json

Copia el archivo de ejemplo y completa con tus credenciales:

```bash
cp ApiMonitor/appsettings.Example.json ApiMonitor/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ApiMonitorDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True"
  },
  "Alerts": {
    "Telegram": {
      "Enabled": true,
      "BotToken": "TU_BOT_TOKEN",
      "ChatId": "TU_CHAT_ID"
    }
  }
}
```

> **Nota:** `appsettings.json` está en `.gitignore` para proteger credenciales. Nunca subas ese archivo al repositorio.

---

## 2. Instalar herramienta de migraciones

```bash
dotnet tool install --global dotnet-ef
```

---

## 3. Crear la base de datos

Desde la carpeta `ApiMonitor/`:

```bash
dotnet ef database update
```

Esto crea la tabla `ApiLogs` en SQL Server automáticamente a partir de las migraciones existentes.

---

## 4. Levantar el proyecto

```bash
dotnet run --project ApiMonitor
```

La API queda disponible en:
- HTTP: `http://localhost:5263`
- Swagger: `https://localhost:7255/swagger`

---

## 5. Endpoint

### `POST /api/logs`

Recibe el log de una llamada a una API externa.

**Body:**

```json
{
  "statusCode":    200,
  "method":        "POST",
  "path":          "/api/pagos/procesar",
  "providerName":  "ServicioPagos",
  "host":          "pagos.miempresa.com",
  "originHost":    "app.miempresa.com",
  "requestBody":   "{\"monto\": 100}",
  "responseBody":  "{\"status\": \"ok\"}",
  "interfaceBody": "BACKEND"
}
```

**Campos requeridos:** `statusCode`, `method`, `path`, `providerName`, `host`, `originHost`

**Campos opcionales:** `requestBody`, `responseBody`, `interfaceBody`

---

### Caso exitoso (statusCode 200 — no dispara Telegram)

```http
POST http://localhost:5263/api/logs
Content-Type: application/json

{
  "statusCode":   200,
  "method":       "POST",
  "path":         "/api/pagos/procesar",
  "providerName": "ServicioPagos",
  "host":         "pagos.miempresa.com",
  "originHost":   "app.miempresa.com"
}
```

Resultado: guarda en BD, **no** manda alerta.

---

### Caso de fallo (statusCode != 200 — dispara Telegram)

```http
POST http://localhost:5263/api/logs
Content-Type: application/json

{
  "statusCode":   500,
  "method":       "POST",
  "path":         "/api/pagos/procesar",
  "providerName": "ServicioPagos",
  "host":         "pagos.miempresa.com",
  "originHost":   "app.miempresa.com",
  "responseBody": "{\"error\": \"Internal Server Error\"}"
}
```

Resultado: envía alerta a Telegram y guarda en BD con `AlertSent = true`.

---

## 6. Verificar en BD

```sql
SELECT * FROM ApiLogs ORDER BY ReceivedAt DESC;
```

| Columna     | Descripción                                      |
|-------------|--------------------------------------------------|
| `Id`        | GUID único del log                               |
| `StatusCode`| Código HTTP recibido                             |
| `AlertSent` | `1` si se envió alerta a Telegram, `0` si no     |
| `ReceivedAt`| Timestamp UTC de recepción                       |

---

## Estructura del proyecto

```
ApiMonitor/
├── Controllers/
│   └── ApiLogController.cs     ← POST /api/logs
├── Data/
│   └── AppDbContext.cs         ← contexto EF Core
├── Dto/
│   └── ApiLogDto.cs            ← validación de entrada
├── Migrations/                 ← migraciones EF Core
├── Models/
│   ├── ApiLog.cs               ← entidad / tabla en BD
│   └── AlertSettings.cs        ← configuración Telegram
├── Services/
│   ├── ApiLogService.cs        ← lógica de procesamiento
│   └── AlertService.cs         ← envío de alertas a Telegram
├── appsettings.Example.json    ← plantilla de configuración (sin credenciales)
└── Program.cs                  ← configuración DI y middleware
```

---

## Tecnologías

| Tecnología | Uso |
|---|---|
| .NET 9 | Framework principal |
| ASP.NET Core Web API | Servidor HTTP |
| Entity Framework Core 9 | 
| SQL Server | Base de datos |
| Telegram Bot API | Notificaciones de alerta |
| Swagger / OpenAPI | Documentación del endpoint |
