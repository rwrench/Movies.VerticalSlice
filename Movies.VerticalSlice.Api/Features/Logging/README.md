# API Request Logging to Database

## Overview
All API requests are automatically logged to the `ApplicationLogs` database table with sensitive data redaction.

## What Gets Logged
- ? **Request Method** (GET, POST, PUT, DELETE, etc.)
- ? **Request Path** (e.g., `/api/movies`)
- ? **Query String** parameters
- ? **Request Body** (up to 4000 characters, with sensitive fields redacted)
- ? **Response Status Code** (200, 404, 500, etc.)
- ? **Response Body** (up to 4000 characters, with sensitive fields redacted)
- ? **Duration** (how long the request took in milliseconds)
- ? **User ID** (from JWT token if authenticated)
- ? **User Name** (from JWT token if authenticated)
- ? **Timestamp** (when the request occurred)
- ? **Exception Details** (if an error occurred)

> **Security Note:** Sensitive fields such as passwords, authentication tokens, API keys, and personally identifiable information (PII) are automatically redacted from logged request and response bodies. Authentication endpoints (e.g., `/api/users/register`, `/api/users/login`) have their entire request/response bodies replaced with `[REDACTED - Sensitive Authentication Data]` to prevent credential exposure.

## Implementation Details

### Middleware
- **File**: `Movies.VerticalSlice.Api/Middleware/ApiRequestLoggingMiddleware.cs`
- Automatically logs every API request to the database
- Skips logging for static files, Swagger, and health checks
- Logs are saved asynchronously to avoid slowing down requests

### Database Table
- **Table**: `ApplicationLogs`
- **Model**: `Movies.VerticalSlice.Api.Data/Models/ApplicationLog.cs`

### Log Levels
- **Information**: Successful requests (2xx status codes)
- **Warning**: Client errors (4xx status codes)
- **Error**: Server errors (5xx status codes) or exceptions

## API Endpoints

### Get All Logs
```http
GET /api/logs?page=1&pageSize=50&level=Error&category=ApiRequest&startDate=2025-01-01
```

**Parameters:**
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 50) - Number of logs per page
- `level` (optional) - Filter by log level (Information, Warning, Error)
- `category` (optional) - Filter by category (e.g., "ApiRequest")
- `startDate` (optional) - Filter logs from this date
- `endDate` (optional) - Filter logs up to this date

**Authorization**: Required (AdminOnly policy - authenticated users only)

> **Note:** Access to this endpoint is restricted to authenticated users. In a production environment, this should be further restricted to admin/support roles only. Sensitive fields such as passwords, JWT tokens, and secrets are redacted from returned log entries.

### Create Custom Log
```http
POST /api/logs
```

**Body:**
```json
{
  "level": "Information",
  "category": "CustomCategory",
  "message": "Custom log message",
  "exception": null,
  "requestPath": "/custom/path",
  "properties": "{\"key\":\"value\"}"
}
```

**Authorization**: Anonymous (no authentication required)

## Querying Logs in SQL

### Recent API Requests
```sql
SELECT TOP 100 
    Timestamp, 
    Level, 
    Message, 
    UserName,
    RequestPath
FROM ApplicationLogs
WHERE Category = 'ApiRequest'
ORDER BY Timestamp DESC
```

### Failed Requests
```sql
SELECT 
    Timestamp, 
    Message, 
    Exception,
    UserName,
    RequestPath
FROM ApplicationLogs
WHERE Level = 'Error' AND Category = 'ApiRequest'
ORDER BY Timestamp DESC
```

### Request Statistics
```sql
SELECT 
    RequestPath,
    COUNT(*) as RequestCount,
    AVG(CAST(REPLACE(JSON_VALUE(Properties, '$.Duration'), 'ms', '') AS INT)) as AvgDurationMs
FROM ApplicationLogs
WHERE Category = 'ApiRequest'
GROUP BY RequestPath
ORDER BY RequestCount DESC
```

### Request Statistics by User
```sql
SELECT 
    UserName,
    COUNT(*) as RequestCount,
    AVG(CAST(REPLACE(JSON_VALUE(Properties, '$.Duration'), 'ms', '') AS INT)) as AvgDurationMs
FROM ApplicationLogs
WHERE Category = 'ApiRequest' AND UserName IS NOT NULL
GROUP BY UserName
ORDER BY RequestCount DESC
```

### Recent Requests by Specific User
```sql
SELECT 
    Timestamp, 
    Message,
    RequestPath,
    JSON_VALUE(Properties, '$.Duration') as Duration,
    JSON_VALUE(Properties, '$.StatusCode') as StatusCode
FROM ApplicationLogs
WHERE Category = 'ApiRequest' 
  AND UserName = 'YourUsername'
ORDER BY Timestamp DESC
```

## Configuration

In `Program.cs`:
```csharp
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

// Custom API request logging middleware (logs to database)
// MUST be after UseAuthentication() to have access to authenticated user
app.UseMiddleware<ApiRequestLoggingMiddleware>();
```

**Important**: The middleware **must be placed AFTER** `UseAuthentication()` and `UseAuthorization()` to have access to the authenticated user's claims from the JWT token. This ensures the correct user ID and username are logged.

## Performance Notes
- Logging is done asynchronously (fire-and-forget) to avoid blocking API requests
- Request and response bodies are limited to 4000 characters
- Static files and Swagger endpoints are excluded from logging
- Consider adding database cleanup jobs for old logs to prevent table growth

## Testing in Swagger

See [SWAGGER-TESTING.md](./SWAGGER-TESTING.md) for detailed instructions on:
- How to authenticate in Swagger
- Understanding which endpoints require authentication (padlock icons)
- Verifying that user information is correctly logged to the database
