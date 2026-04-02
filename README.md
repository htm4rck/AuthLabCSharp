# Auth - Servicio de Autenticación

API REST de autenticación y gestión de sesiones construida con .NET 9, JWT y SQL Server.

## Alcance

Este microservicio cubre:

- **Registro de usuarios** con hash de contraseña (HMACSHA512)
- **Login** con generación de token JWT
- **Gestión de sesiones** (registro, cierre, validación)
- **Migraciones automáticas** de base de datos con EF Core

No cubre: autorización por roles, refresh tokens, recuperación de contraseña, verificación de email.

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Runtime | .NET 9 |
| Base de datos | SQL Server 2022 (Docker) |
| ORM | Entity Framework Core 9 |
| Autenticación | JWT Bearer |
| Hashing | HMACSHA512 |

## Arquitectura

```mermaid
graph TB
    Client[Cliente HTTP] -->|REST API| Controller
    
    subgraph Auth Service [".NET 9 - Auth API"]
        Controller[AuthController] --> AuthSvc[AuthService]
        Controller --> SessionSvc[SessionService]
        AuthSvc --> SessionSvc
        AuthSvc --> DB[(AuthContext<br/>EF Core)]
        SessionSvc --> DB
    end
    
    DB -->|TCP 1433| SQLServer[(SQL Server 2022<br/>Docker)]
    AuthSvc -->|Genera| JWT{{Token JWT}}
    JWT -->|Respuesta| Client
```

## Flujo de Registro

```mermaid
sequenceDiagram
    actor C as Cliente
    participant AC as AuthController
    participant AS as AuthService
    participant DB as SQL Server

    C->>AC: POST /api/auth/registrar
    AC->>AS: UserIs(email)
    AS->>DB: SELECT email
    DB-->>AS: exists?
    
    alt Email ya registrado
        AS-->>AC: true
        AC-->>C: 400 Bad Request
    else Email disponible
        AS-->>AC: false
        AC->>AS: Register(user, password)
        AS->>AS: CrearPasswordHash(password)
        AS->>DB: INSERT User
        DB-->>AS: User creado
        AS-->>AC: User
        AC-->>C: 200 OK {UserId, Email}
    end
```

## Flujo de Login

```mermaid
sequenceDiagram
    actor C as Cliente
    participant AC as AuthController
    participant AS as AuthService
    participant SS as SessionService
    participant DB as SQL Server

    C->>AC: POST /api/auth/login
    AC->>AS: Login(email, password)
    AS->>DB: SELECT User by email
    DB-->>AS: User
    AS->>AS: VerificarPasswordHash()
    
    alt Credenciales inválidas
        AS-->>AC: null
        AC-->>C: 401 Unauthorized
    else Credenciales válidas
        AS->>AS: GenerarToken(user)
        AS->>SS: RegisterSesion(userId, token)
        SS->>DB: INSERT Session
        AS-->>AC: token
        AC-->>C: 200 OK {Token}
    end
```

## Modelo de Datos

```mermaid
erDiagram
    USER {
        int UserId PK
        string Name
        string Email UK
        bytes PasswordHash
        bytes PasswordSalt
        bool Active
        datetime CreationDate
    }
    
    SESSION {
        int SessionId PK
        int UserId FK
        string Token
        datetime StartDate
        datetime EndDate
        string Device
        string Ip
    }
    
    USER ||--o{ SESSION : "tiene"
```

## Estructura del Proyecto

```
Auth/
├── Controller/
│   └── AuthController.cs      # Endpoints REST + DTOs
├── Data/
│   └── AuthContext.cs          # DbContext EF Core
├── Entity/
│   ├── User.cs                 # Entidad usuario
│   └── Session.cs              # Entidad sesión
├── Service/
│   ├── IAuthService.cs         # Contrato autenticación
│   ├── AuthService.cs          # Lógica de auth + JWT
│   ├── ISessionService.cs      # Contrato sesiones
│   └── SessionService.cs       # Lógica de sesiones
├── Migrations/                 # Migraciones EF Core
├── Program.cs                  # Configuración y startup
├── appsettings.json            # Configuración
└── docker-compose.yml          # SQL Server en Docker
```

## Endpoints

| Método | Ruta | Descripción | Body |
|---|---|---|---|
| POST | `/api/auth/registrar` | Registro de usuario | `{ nombre, email, password }` |
| POST | `/api/auth/login` | Login y obtención de JWT | `{ email, password }` |

## Inicio Rápido

```bash
# 1. Levantar SQL Server
docker compose up -d

# 2. Ejecutar la API (las migraciones se aplican automáticamente)
dotnet run
```

La API estará disponible en `http://localhost:5109`.
