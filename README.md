# Desafío práctico #3 – Sistema de Recetas de Cocina

API en ASP.NET Core 9.0 con Entity Framework Core (SQL Server), ASP.NET Core Identity (roles **Administrador** y **Usuario**) y reportes con SQL Server Reporting Services (SSRS).


# Integrantes:

Angel Eduardo moreno Escobar  ME220001
Ricardo Iván Escobar Umaña    EU220488
Rafael Adolfo Ruiz Garcia     RG210380



---

## Estructura del proyecto

```
Solucion/
├── RecetasAPI.sln
├── RecetasAPI/                         Web API (.NET 9)
│   ├── Controllers/
│   │   ├── RecetasController.cs            CRUD de recetas
│   │   ├── IngredientesController.cs       CRUD de ingredientes
│   │   ├── PasosPreparacionController.cs   CRUD de pasos de preparación
│   │   ├── UsuariosController.cs           Registro, asignación de rol, usuario actual, cerrar sesión
│   │   └── ReportesController.cs           URLs del visor de reportes de SSRS
│   ├── Data/
│   │   ├── RecetasDbContext.cs             IdentityDbContext<Usuario> + data seed
│   │   └── DbInitializer.cs                Crea roles y usuarios de prueba al iniciar
│   ├── Migrations/                         Migración inicial de EF Core
│   ├── Models/                             Receta, Ingrediente, PasoPreparacion, Usuario, DTOs
│   ├── wwwroot/index.html                  Página para iniciar sesión y abrir los reportes
│   ├── Properties/launchSettings.json
│   ├── appsettings.json                    Cadena de conexión y configuración de SSRS
│   ├── Program.cs
│   └── RecetasAPI.http
├── RecetasAPI.Tests/                   Pruebas unitarias (xUnit)
└── Reportes/
    ├── ReporteRecetasIngredientes.rdl      Reporte 1
    └── ReportePasosPreparacion.rdl         Reporte 2
```

---

## Requisitos

- **SDK de .NET 9** (`dotnet --list-sdks` debe mostrar una versión 9.0.x)
- **SQL Server** con autenticación de Windows
- **SQL Server Reporting Services 2022** (edición Developer) y **Report Builder**
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

---

## Instalación

### 1. Configurar la conexión a la base de datos

En `RecetasAPI/appsettings.json`, ajustar el servidor si no es `localhost`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RecetasDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 2. Crear la base de datos

Desde la carpeta `Solucion`:

```powershell
dotnet restore
dotnet ef database update --project RecetasAPI
```

La migración ya está incluida. Crea la base `RecetasDB` con las tablas de Identity, `Recetas`, `Ingredientes` y `PasosPreparacion`, y carga los datos iniciales: 3 recetas con sus ingredientes y pasos.

> En Visual Studio se puede usar la Consola del Administrador de paquetes: `Update-Database`.

### 3. Ejecutar la API

```powershell
dotnet run --project RecetasAPI --launch-profile http
```

Al iniciar por primera vez se crean los roles y los usuarios de prueba.

| URL | Descripción |
|---|---|
| http://localhost:5080/scalar/v1 | Documentación y pruebas de la API (Scalar) |
| http://localhost:5080/ | Página de inicio de sesión y reportes |

### 4. Usuarios de prueba

| Usuario | Contraseña | Rol | Permisos |
|---|---|---|---|
| admin@recetas.com | Admin123$ | Administrador | Consultar, crear, modificar y eliminar |
| usuario@recetas.com | Usuario123$ | Usuario | Solo consultar |

Iniciar sesión en Scalar con `POST /api/auth/login?useCookies=true&useSessionCookies=true`:

```json
{ "email": "admin@recetas.com", "password": "Admin123$" }
```
z
Sin sesión, la API responde **401**. Si el rol no tiene permiso, responde **403**.

### 5. Configurar SSRS

Este proyecto usa SSRS en el **puerto 8080**. Si SSRS está en otro puerto (por ejemplo, el 80), cambiar `UrlServidor` en `appsettings.json`:

```json
"ReportesSSRS": {
  "UrlServidor": "http://localhost:8080/ReportServer",
  "Carpeta": "/Recetas",
  "ReporteRecetasIngredientes": "ReporteRecetasIngredientes",
  "ReportePasosPreparacion": "ReportePasosPreparacion"
}
```

### 6. Publicar los reportes

1. Abrir el portal de SSRS (`http://localhost:8080/Reports`) y crear la carpeta **Recetas** en Inicio, con **Nuevo → Carpeta**.
2. Entrar a la carpeta **Recetas**, usar **Cargar** y subir los dos archivos de la carpeta `Reportes/`:
   - `ReporteRecetasIngredientes.rdl`
   - `ReportePasosPreparacion.rdl`
3. Si el servidor de SQL Server no es `localhost`: abrir cada reporte en **Report Builder**, cambiar el origen de datos `RecetasDB` y guardarlo de nuevo en la carpeta **Recetas**. El origen de datos actual es `Data Source=localhost;Initial Catalog=RecetasDB` con seguridad integrada de Windows.

> Los reportes deben quedar en `/Recetas` con esos nombres exactos. Si no, la API no los encuentra.

### 7. Ver los reportes desde la aplicación

1. Abrir http://localhost:5080/ e iniciar sesión.
2. **Ver reporte 1**: recetas con sus ingredientes, filtradas por tiempo de preparación (minutos, desde/hasta).
3. **Ver reporte 2**: pasos de preparación de cada receta, con la cantidad de pasos y su orden.

Los reportes se abren en el visor de reportes de SSRS, en una pestaña nueva.

### 8. Ejecutar las pruebas unitarias

```powershell
dotnet test
```

---

## Solución de problemas

| Problema | Solución |
|---|---|
| `You must install or update .NET to run this application` (framework 9.0.0) | Instalar el SDK de .NET 9: `winget install Microsoft.DotNet.SDK.9` |
| `Add-Migration` / `Update-Database` no se reconoce en PowerShell | Esos comandos son solo de Visual Studio. En PowerShell usar `dotnet ef database update --project RecetasAPI` |
| `http://localhost/Reports` muestra "Not Found" de Apache | Otro programa (por ejemplo XAMPP) usa el puerto 80. Cambiar SSRS al puerto 8080 en Report Server Configuration Manager, en **Dirección URL del servicio web** y en **Dirección URL del Portal web**, y reiniciar el servicio |
| El reporte no se encuentra desde la aplicación | Verificar que los reportes estén en la carpeta `/Recetas` del portal y que `UrlServidor` tenga el puerto correcto |
| Error al conectar a la base de datos al iniciar la API | Ejecutar primero `dotnet ef database update --project RecetasAPI` y revisar la cadena de conexión |
