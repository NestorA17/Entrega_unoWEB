# Tutorial: Frontend Blazor CRUD
# Parte 1: Conceptos Fundamentales

Este tutorial construye un frontend web con **Blazor** que consume una API REST generica para hacer operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre tablas de una base de datos SQL Server.

**Proyecto**: FrontBlazor_AppiGenericaCsharp
**Base de datos**: bdfacturas_sqlserver_local
**API backend**: ApiGenericaCsharp (http://localhost:5034)

---

## 1.1 Que es Blazor

Blazor es un framework de Microsoft para construir interfaces web interactivas usando C# en lugar de JavaScript. Forma parte de ASP.NET Core.

### Tres modalidades de Blazor

```
┌─────────────────────────────────────────────────────────────┐
│                    BLAZOR SERVER                            │
│                                                             │
│  El codigo C# se ejecuta en el SERVIDOR.                    │
│  El navegador solo muestra HTML.                            │
│  La comunicacion es en tiempo real via SignalR (WebSocket).  │
│                                                             │
│  Ventaja: Simple, rapido de cargar, facil de depurar.       │
│  Desventaja: Necesita conexion constante al servidor.       │
│                                                             │
│  >>> Esta es la modalidad que usaremos en este tutorial <<< │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                 BLAZOR WEBASSEMBLY (WASM)                   │
│                                                             │
│  El codigo C# se descarga y ejecuta en el NAVEGADOR.        │
│  No necesita servidor despues de la carga inicial.          │
│                                                             │
│  Ventaja: Funciona offline, no depende del servidor.        │
│  Desventaja: Carga inicial lenta, archivos grandes.         │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   BLAZOR WEB APP                            │
│                                                             │
│  Combinacion de Server + WASM.                              │
│  Cada pagina puede elegir su modo de renderizado.           │
│                                                             │
│  Ventaja: Lo mejor de ambos mundos.                         │
│  Desventaja: Mas complejo de configurar.                    │
└─────────────────────────────────────────────────────────────┘
```

### Como funciona Blazor Server (nuestro caso)

```
┌──────────────┐         SignalR          ┌──────────────────┐
│  NAVEGADOR   │ ◄═══════════════════════►│  SERVIDOR .NET   │
│              │    (WebSocket en         │                  │
│  Solo HTML   │     tiempo real)         │  Codigo C#       │
│  y CSS       │                          │  Logica          │
│              │  1. Usuario hace click   │  Componentes     │
│              │  ──────────────────────► │                  │
│              │                          │  2. Ejecuta C#   │
│              │  3. HTML actualizado     │                  │
│              │  ◄────────────────────── │                  │
└──────────────┘                          └──────────────────┘
```

El navegador no ejecuta C#. Todo el codigo corre en el servidor y el navegador recibe las actualizaciones de HTML en tiempo real.

---

## 1.2 Componentes Razor (.razor)

Un componente Razor es un archivo `.razor` que combina HTML y C# en un solo lugar. Es la unidad basica de construccion en Blazor.

### Estructura de un componente

```razor
@page "/producto"                    ← Ruta URL de esta pagina

<h3>Lista de Productos</h3>          ← HTML normal

@if (cargando)                       ← Logica C# mezclada con HTML
{
    <p>Cargando...</p>
}
else
{
    <table class="table">
        @foreach (var item in productos)
        {
            <tr>
                <td>@item.Nombre</td>
                <td>@item.Precio</td>
            </tr>
        }
    </table>
}

@code {                              ← Bloque de codigo C#
    private bool cargando = true;
    private List<Producto> productos = new();

    protected override async Task OnInitializedAsync()
    {
        productos = await CargarProductos();
        cargando = false;
    }
}
```

**Reglas clave:**
- `@page "/ruta"` — Define en que URL se muestra la pagina
- `@` — Cambia de HTML a C# (expresiones, if, foreach, etc.)
- `@code { }` — Bloque donde se declaran variables y metodos C#
- El HTML y el C# conviven en el mismo archivo

---

## 1.3 Data Binding (Enlace de Datos)

Data Binding es como Blazor conecta los datos de C# con los elementos de HTML. Hay tres tipos principales:

### Mostrar datos (una direccion: C# → HTML)

```razor
<p>Hola, @nombre</p>          ← Muestra el valor de la variable

@code {
    private string nombre = "Carlos";
}
```

### Enlace bidireccional con @bind (C# ↔ HTML)

```razor
<input @bind="nombre" />      ← Lo que el usuario escribe se guarda en la variable
<p>Escribiste: @nombre</p>    ← Y la variable se muestra en tiempo real

@code {
    private string nombre = "";
}
```

Cuando el usuario escribe en el input, la variable `nombre` se actualiza automaticamente. Y si el codigo cambia `nombre`, el input tambien se actualiza.

### Eventos con @onclick

```razor
<button @onclick="Saludar">Click aqui</button>
<p>@mensaje</p>

@code {
    private string mensaje = "";

    private void Saludar()
    {
        mensaje = "Hola desde Blazor!";
    }
}
```

Cuando el usuario hace click en el boton, se ejecuta el metodo `Saludar()` y el HTML se actualiza solo.

---

## 1.4 Ciclo de Vida de un Componente

Cuando una pagina se carga, Blazor ejecuta una serie de metodos en orden. Los dos mas importantes son:

```
    Usuario navega a /producto
              │
              ▼
    ┌─────────────────────┐
    │  Se crea el          │
    │  componente           │
    └──────────┬────────────┘
               │
               ▼
    ┌─────────────────────────────┐
    │  OnInitializedAsync()       │ ← Se ejecuta UNA vez al cargar
    │                             │   Ideal para: cargar datos de la API
    │  Aqui llamamos a la API     │
    │  para obtener los datos     │
    └──────────┬──────────────────┘
               │
               ▼
    ┌─────────────────────────────┐
    │  Se renderiza el HTML       │ ← Blazor genera el HTML con los datos
    └──────────┬──────────────────┘
               │
               ▼
    ┌─────────────────────────────┐
    │  StateHasChanged()          │ ← Se llama cuando necesitas
    │                             │   forzar una actualizacion del HTML
    │  Ejemplo: despues de crear  │   (Blazor lo llama automaticamente
    │  o eliminar un registro     │    en la mayoria de casos)
    └─────────────────────────────┘
```

En la practica, el patron mas comun es:

```csharp
protected override async Task OnInitializedAsync()
{
    // Cargar datos al abrir la pagina
    productos = await apiService.ListarAsync("producto");
}
```

---

## 1.5 HttpClient: Como Consumir una API REST

HttpClient es la clase de .NET que permite hacer peticiones HTTP (GET, POST, PUT, DELETE) a una API.

En nuestro proyecto, el frontend Blazor se comunica con la API generica asi:

```
┌───────────────────┐    HTTP GET/POST/PUT/DELETE    ┌────────────────────┐
│  BLAZOR           │ ──────────────────────────────►│  API               │
│  (puerto 5100)    │                                │  (puerto 5034)     │
│                   │◄────────────────────────────── │                    │
│  Frontend         │         JSON                   │  ApiGenericaCsharp│
└───────────────────┘                                └────────────────────┘
```

### Las 4 operaciones basicas

| Operacion | Metodo HTTP | Endpoint de la API | Que hace |
|---|---|---|---|
| Listar | GET | `/api/producto` | Obtiene todos los registros |
| Crear | POST | `/api/producto` | Inserta un nuevo registro |
| Actualizar | PUT | `/api/producto/codigo/PR001` | Modifica un registro existente |
| Eliminar | DELETE | `/api/producto/codigo/PR001` | Borra un registro |

### Ejemplo de cada operacion

```csharp
// LISTAR - Obtener todos los productos
var respuesta = await http.GetFromJsonAsync<RespuestaApi>("/api/producto");

// CREAR - Enviar un nuevo producto
var datos = new Dictionary<string, object?>
{
    { "codigo", "PR099" },
    { "nombre", "Laptop HP" },
    { "stock", 10 },
    { "valorunitario", 2500000 }
};
await http.PostAsJsonAsync("/api/producto", datos);

// ACTUALIZAR - Modificar un producto existente
var cambios = new Dictionary<string, object?>
{
    { "nombre", "Laptop HP Actualizada" },
    { "stock", 15 }
};
await http.PutAsJsonAsync("/api/producto/codigo/PR099", cambios);

// ELIMINAR - Borrar un producto
await http.DeleteAsync("/api/producto/codigo/PR099");
```

---

## 1.6 Inyeccion de Dependencias en Blazor

Inyeccion de dependencias (DI) es un mecanismo que permite que Blazor le entregue automaticamente los servicios que un componente necesita, sin que el componente tenga que crearlos.

### Como funciona

**Paso 1**: Registras el servicio en `Program.cs` (se hace una sola vez)

```csharp
builder.Services.AddScoped<ApiService>();
```

**Paso 2**: Lo pides en cualquier componente con `@inject`

```razor
@inject ApiService Api

<button @onclick="Cargar">Cargar datos</button>

@code {
    private async Task Cargar()
    {
        var datos = await Api.ListarAsync("producto");
    }
}
```

Blazor se encarga de crear la instancia de `ApiService` y entregartela. No necesitas hacer `new ApiService()` manualmente.

### Por que es util

- **Reutilizacion**: El mismo servicio se usa en todas las paginas
- **Configuracion centralizada**: El HttpClient se configura UNA vez en Program.cs con la URL de la API
- **Testeable**: Se puede reemplazar el servicio real por uno de prueba

---

## 1.7 Bootstrap Basico

Bootstrap es una libreria CSS que viene incluida en los proyectos Blazor. Permite crear interfaces profesionales con solo agregar clases CSS a los elementos HTML.

### Tablas

```html
<table class="table table-striped table-hover">
    <thead class="table-dark">
        <tr>
            <th>Codigo</th>
            <th>Nombre</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>PR001</td>
            <td>Laptop</td>
            <td>
                <button class="btn btn-warning btn-sm">Editar</button>
                <button class="btn btn-danger btn-sm">Eliminar</button>
            </td>
        </tr>
    </tbody>
</table>
```

- `table` — Estilo basico de tabla
- `table-striped` — Filas alternadas con color
- `table-hover` — Resalta la fila al pasar el mouse
- `table-dark` — Encabezado oscuro

### Formularios

```html
<div class="mb-3">
    <label class="form-label">Nombre</label>
    <input class="form-control" type="text" />
</div>

<button class="btn btn-primary">Guardar</button>
<button class="btn btn-secondary">Cancelar</button>
```

- `form-control` — Estilo de input profesional
- `form-label` — Estilo de etiqueta
- `mb-3` — Margen inferior (espaciado)

### Botones

| Clase | Color | Uso tipico |
|---|---|---|
| `btn btn-primary` | Azul | Guardar, Crear |
| `btn btn-success` | Verde | Confirmar |
| `btn btn-warning` | Amarillo | Editar |
| `btn btn-danger` | Rojo | Eliminar |
| `btn btn-secondary` | Gris | Cancelar |
| `btn btn-sm` | (pequeño) | Botones dentro de tablas |

### Alertas

```html
<div class="alert alert-success">Registro creado exitosamente.</div>
<div class="alert alert-danger">Error al eliminar el registro.</div>
```

### Spinner (indicador de carga)

```html
<div class="spinner-border text-primary" role="status">
    <span class="visually-hidden">Cargando...</span>
</div>
```

---

## 1.8 Estructura del Proyecto

Este es el mapa del proyecto que construiremos:

```
FrontBlazor_AppiGenericaCsharp/
│
├── Program.cs                          ← Punto de entrada. Configura el HttpClient
│                                         y registra los servicios.
│
├── FrontBlazor_AppiGenericaCsharp.csproj ← Archivo del proyecto. Lista las dependencias.
│
├── appsettings.json                    ← Configuracion (URL de la API).
│
├── Properties/
│   └── launchSettings.json             ← Puerto donde corre el frontend (5100).
│
├── Services/
│   └── ApiService.cs                   ← Servicio que hace las llamadas HTTP a la API.
│                                         Metodos: Listar, Crear, Actualizar, Eliminar.
│
├── Components/
│   ├── App.razor                       ← Componente raiz de la aplicacion.
│   ├── Routes.razor                    ← Configura el enrutamiento de paginas.
│   ├── _Imports.razor                  ← Usings compartidos por todos los componentes.
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor            ← Estructura general de la pagina (navbar + contenido).
│   │   └── NavMenu.razor               ← Menu de navegacion con links a cada tabla.
│   │
│   └── Pages/
│       ├── Home.razor                  ← Pagina de bienvenida.
│       ├── Producto.razor              ← CRUD de la tabla producto.
│       ├── Empresa.razor               ← CRUD de la tabla empresa.
│       ├── Persona.razor               ← CRUD de la tabla persona.
│       ├── Rol.razor                   ← CRUD de la tabla rol.
│       ├── Ruta.razor                  ← CRUD de la tabla ruta.
│       └── Usuario.razor               ← CRUD de la tabla usuario.
│
└── wwwroot/
    ├── css/                            ← Estilos CSS (Bootstrap viene incluido).
    └── favicon.png                     ← Icono de la aplicacion.
```

---

## 1.9 Flujo Completo de una Operacion

Cuando el usuario hace click en "Eliminar" un producto, esto es lo que sucede:

```
  1. Usuario hace click en "Eliminar"
              │
              ▼
  2. Blazor ejecuta el metodo EliminarProducto() en el componente
              │
              ▼
  3. El metodo llama a ApiService.EliminarAsync("producto", "codigo", "PR001")
              │
              ▼
  4. ApiService hace: DELETE http://localhost:5034/api/producto/codigo/PR001
              │
              ▼
  5. La API recibe la peticion en EntidadesController.EliminarAsync()
              │
              ▼
  6. El controlador llama a ServicioCrud → Repositorio → SQL Server
              │
              ▼
  7. SQL Server ejecuta: DELETE FROM producto WHERE codigo = 'PR001'
              │
              ▼
  8. La API responde: { "estado": 200, "mensaje": "Registro eliminado exitosamente." }
              │
              ▼
  9. ApiService recibe la respuesta y la devuelve al componente
              │
              ▼
  10. El componente recarga la lista y muestra una alerta verde de exito
              │
              ▼
  11. Blazor actualiza el HTML en el navegador via SignalR
```

---

## 1.10 Tablas que Vamos a Gestionar

Estas son las 6 tablas de `bdfacturas_sqlserver_local` que no tienen clave foranea (son tablas maestras independientes):

| Tabla | Columnas | Clave | Descripcion |
|---|---|---|---|
| empresa | codigo, nombre | codigo | Empresas registradas |
| persona | codigo, nombre, email, telefono | codigo | Personas (clientes potenciales) |
| producto | codigo, nombre, stock, valorunitario | codigo | Catalogo de productos |
| rol | id, nombre | id | Roles del sistema |
| ruta | ruta, descripcion | ruta | Rutas de navegacion/permisos |
| usuario | email, contrasena | email | Usuarios del sistema |

Cada tabla tendra su propia pagina con las 4 operaciones CRUD completas.

---

## 1.11 Prerequisitos

Antes de comenzar necesitas tener instalado:

**SDK de .NET 9.0+**
```bash
dotnet --version
```

**Visual Studio Code** con la extension **C# Dev Kit** (Microsoft)

**Git**
```bash
git --version
```

**La API ApiGenericaCsharp** funcionando en http://localhost:5034

**SQL Server** con la base de datos `bdfacturas_sqlserver_local` y las tablas creadas

---

## Siguiente Parte

En la **Parte 2** crearemos el proyecto, configuraremos el HttpClient para conectar con la API, y haremos el primer `git add`, `commit` y `push`.
