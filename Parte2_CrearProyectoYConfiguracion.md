# Tutorial: Frontend Blazor CRUD
# Parte 2: Crear el Proyecto y Configuracion

En esta parte creamos el proyecto Blazor Server, lo configuramos para conectarse a la API, y hacemos el primer commit en Git.

---

## 2.1 Entorno de Trabajo

En este tutorial trabajaremos con:

- **Visual Studio Code (VS Code)** — Editor de codigo gratuito de Microsoft. Lo usamos porque es ligero, tiene excelente soporte para C# con la extension "C# Dev Kit", y permite ejecutar comandos desde su terminal integrada.
- **PowerShell** — Terminal que viene incluida en Windows. Es la terminal por defecto de VS Code en Windows. Todos los comandos `dotnet`, `git`, etc. se ejecutan aqui.

Para abrir la terminal en VS Code: menu **Terminal → New Terminal** (o el atajo ``Ctrl + ` ``). Se abrira PowerShell en la parte inferior del editor.

---

## 2.2 Crear el Proyecto

En la terminal de PowerShell, nos ubicamos en la carpeta donde se creara el proyecto. Por ejemplo, una carpeta de proyectos C#:

```powershell
cd proyectoscsharp
```

Creamos el proyecto Blazor con interactividad del lado del servidor y sin HTTPS (para simplificar el desarrollo local):

```powershell
dotnet new blazor -n FrontBlazor_AppiGenericaCsharp --interactivity Server --no-https
```

**Que hace este comando:**
- `dotnet new blazor` — Crea un proyecto Blazor Web App
- `-n FrontBlazor_AppiGenericaCsharp` — Nombre del proyecto
- `--interactivity Server` — Usa Blazor Server (el codigo C# corre en el servidor, no en el navegador)
- `--no-https` — No configura HTTPS, solo HTTP. Para desarrollo local es suficiente

Entramos a la carpeta del proyecto:

```powershell
cd FrontBlazor_AppiGenericaCsharp
```

Verificamos que el proyecto se creo correctamente:

```powershell
dotnet build
```

Si compila sin errores, el proyecto esta listo.

---

## 2.3 Inicializar Git

Inicializamos el repositorio Git dentro de la carpeta del proyecto:

```powershell
git init
```

Creamos un archivo `.gitignore` para que Git ignore archivos innecesarios (compilados, paquetes, archivos temporales):

```powershell
dotnet new gitignore
```

Este comando genera un `.gitignore` con todas las exclusiones necesarias para proyectos .NET (carpetas `bin/`, `obj/`, archivos `.user`, etc.).

Conectamos el repositorio local con el repositorio remoto en GitHub:

```powershell
git remote add origin https://github.com/ccastro2050/FrontBlazor_AppiGenericaCsharp.git
```

---

## 2.4 Configurar el Puerto del Frontend

El frontend debe correr en un puerto diferente al de la API para que no haya conflicto:

- **API ApiGenericaCsharp**: puerto **5034**
- **Frontend Blazor**: puerto **5100**

Abrimos el archivo `Properties/launchSettings.json` y cambiamos la URL del perfil `http`:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5100",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Que cambiamos:**
- `applicationUrl` ahora es `http://localhost:5100` (antes era un puerto aleatorio)
- `launchBrowser` en `true` para que abra el navegador automaticamente al ejecutar

---

## 2.5 Configurar HttpClient para Conectar con la API

El HttpClient es el objeto que Blazor usa para hacer peticiones HTTP a la API. Lo configuramos en `Program.cs`.

Abrimos `Program.cs` y lo modificamos asi:

```csharp
using FrontBlazor_AppiGenericaCsharp.Components;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para conectarse a la API
// La URL base apunta a la API ApiGenericaCsharp que corre en el puerto 5034
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5034")
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**Que agregamos:**
- `builder.Services.AddScoped(...)` — Registra un HttpClient con la URL base `http://localhost:5034`
- Esto significa que cuando hagamos `await http.GetAsync("/api/producto")`, la peticion se enviara a `http://localhost:5034/api/producto`
- `AddScoped` crea una instancia de HttpClient por cada conexion de usuario (sesion de SignalR)

---

## 2.6 Limpiar Archivos de Ejemplo

El template de Blazor trae paginas de ejemplo que no necesitamos (Counter, Weather). Las eliminamos para tener un proyecto limpio:

**Archivos a eliminar:**
- `Components/Pages/Counter.razor` — Pagina de ejemplo con un contador
- `Components/Pages/Weather.razor` — Pagina de ejemplo con datos ficticios de clima

**Nota:** No eliminamos `Home.razor` porque la modificaremos como pagina de bienvenida, ni `Error.razor` porque maneja errores del framework.

---

## 2.7 Modificar Home.razor

Abrimos `Components/Pages/Home.razor` y lo cambiamos por una pagina de bienvenida:

```razor
@page "/"

<PageTitle>CRUD Facturas</PageTitle>

<div class="container mt-4">
    <h1>CRUD - Base de Datos Facturas</h1>
    <p class="lead">
        Frontend Blazor que consume la API generica
        <strong>ApiGenericaCsharp</strong> para gestionar las tablas
        de <code>bdfacturas_sqlserver_local</code>.
    </p>

    <div class="alert alert-info">
        <strong>Tablas disponibles:</strong> Empresa, Persona, Producto, Rol, Ruta, Usuario.
        <br />
        Use el menu lateral para navegar a cada tabla.
    </div>
</div>
```

**Que hace:**
- `@page "/"` — Esta pagina se muestra en la URL raiz (http://localhost:5100/)
- `<PageTitle>` — Titulo que aparece en la pestaña del navegador
- Clases Bootstrap: `container`, `mt-4`, `lead`, `alert alert-info` — dan estilo profesional sin escribir CSS

---

## 2.8 Primer Commit

Verificamos el estado de los archivos:

```powershell
git status
```

Agregamos todos los archivos al staging area:

```powershell
git add .
```

**Que hace `git add .`:** Marca todos los archivos nuevos y modificados para incluirlos en el proximo commit. El punto (`.`) significa "todos los archivos del directorio actual". Los archivos listados en `.gitignore` se excluyen automaticamente.

Creamos el primer commit:

```powershell
git commit -m "Proyecto inicial: Blazor Server configurado con HttpClient a API puerto 5034"
```

**Que hace `git commit -m`:** Guarda una captura (snapshot) de los archivos marcados con `git add`. El mensaje entre comillas describe que cambios se hicieron.

Subimos al repositorio remoto en GitHub:

```powershell
git branch -M main
git push -u origin main
```

**Que hacen estos comandos:**
- `git branch -M main` — Renombra la rama actual a `main` (por convencion de GitHub)
- `git push -u origin main` — Sube los archivos a GitHub. `-u` establece `origin/main` como la rama remota por defecto para futuros `git push`

---

## 2.9 Verificar

Despues de ejecutar estos pasos debes tener:

1. **Carpeta del proyecto** `FrontBlazor_AppiGenericaCsharp/` con la estructura de Blazor
2. **Puerto 5100** configurado en `launchSettings.json`
3. **HttpClient** apuntando a `http://localhost:5034` en `Program.cs`
4. **Paginas de ejemplo eliminadas** (Counter, Weather)
5. **Home.razor** con pagina de bienvenida
6. **Primer commit** subido a GitHub

Para probar que el proyecto corre:

```powershell
dotnet run
```

Abre `http://localhost:5100` en el navegador. Debe aparecer la pagina de bienvenida.

---

## Siguiente Parte

En la **Parte 3** crearemos el `ApiService` — un servicio generico reutilizable que encapsula todas las llamadas HTTP a la API (Listar, Crear, Actualizar, Eliminar).
