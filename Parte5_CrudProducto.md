# Tutorial: Frontend Blazor CRUD
# Parte 5: CRUD Completo de Producto

En esta parte creamos la pagina `Producto.razor` con las 4 operaciones CRUD completas. Esta es la pagina mas importante del tutorial porque sirve como modelo para las demas tablas.

**Tabla producto:**
| Columna | Tipo | Descripcion |
|---|---|---|
| codigo | varchar(30) | Clave primaria |
| nombre | varchar(100) | Nombre del producto |
| stock | int | Cantidad disponible |
| valorunitario | numeric | Precio unitario |

---

## 5.1 Estructura General de una Pagina CRUD

Cada pagina CRUD tiene 3 secciones principales:

```
┌──────────────────────────────────────────────────┐
│  1. ALERTA (mensaje de exito o error)            │
│     [verde] Registro creado exitosamente.        │
├──────────────────────────────────────────────────┤
│  2. FORMULARIO (crear o editar)                  │
│     Codigo: [________]                           │
│     Nombre: [________]                           │
│     Stock:  [________]                           │
│     Valor:  [________]                           │
│     [Guardar]  [Cancelar]                        │
├──────────────────────────────────────────────────┤
│  3. TABLA (lista de registros)                   │
│     ┌────────┬─────────┬───────┬────────┬──────┐ │
│     │ Codigo │ Nombre  │ Stock │ Valor  │ Acc. │ │
│     ├────────┼─────────┼───────┼────────┼──────┤ │
│     │ PR001  │ Laptop  │ 10    │ 2.5M   │ E  X │ │
│     │ PR002  │ Mouse   │ 50    │ 35K    │ E  X │ │
│     └────────┴─────────┴───────┴────────┴──────┘ │
│     E = Editar    X = Eliminar                   │
└──────────────────────────────────────────────────┘
```

---

## 5.2 El Codigo Completo

Creamos el archivo `Components/Pages/Producto.razor`:

```razor
@page "/producto"
@rendermode InteractiveServer
@inject FrontBlazor_AppiGenericaCsharp.Services.ApiService Api

<PageTitle>Productos</PageTitle>

<div class="container mt-4">
    <h3>Productos</h3>

    @* ───────── ALERTA DE EXITO O ERROR ───────── *@
    @if (!string.IsNullOrEmpty(mensaje))
    {
        <div class="alert @(exito ? "alert-success" : "alert-danger") alert-dismissible fade show">
            @mensaje
            <button type="button" class="btn-close" @onclick="() => mensaje = string.Empty"></button>
        </div>
    }

    @* ───────── BOTON PARA MOSTRAR FORMULARIO DE CREAR ───────── *@
    @if (!mostrarFormulario)
    {
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nuevo Producto</button>
    }

    @* ───────── FORMULARIO (CREAR / EDITAR) ───────── *@
    @if (mostrarFormulario)
    {
        <div class="card mb-3">
            <div class="card-header">
                @(editando ? "Editar Producto" : "Nuevo Producto")
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Codigo</label>
                        <input class="form-control" @bind="campoCodigo" disabled="@editando" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Nombre</label>
                        <input class="form-control" @bind="campoNombre" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Stock</label>
                        <input class="form-control" type="number" @bind="campoStock" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Valor Unitario</label>
                        <input class="form-control" type="number" step="0.01" @bind="campoValor" />
                    </div>
                </div>
                <button class="btn btn-success me-2" @onclick="GuardarRegistro">Guardar</button>
                <button class="btn btn-secondary" @onclick="Cancelar">Cancelar</button>
            </div>
        </div>
    }

    @* ───────── SPINNER DE CARGA ───────── *@
    @if (cargando)
    {
        <div class="d-flex justify-content-center my-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }

    @* ───────── TABLA DE REGISTROS ───────── *@
    @if (!cargando && registros.Any())
    {
        <table class="table table-striped table-hover">
            <thead class="table-dark">
                <tr>
                    <th>Codigo</th>
                    <th>Nombre</th>
                    <th>Stock</th>
                    <th>Valor Unitario</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["codigo"]</td>
                        <td>@reg["nombre"]</td>
                        <td>@reg["stock"]</td>
                        <td>@reg["valorunitario"]</td>
                        <td>
                            <button class="btn btn-warning btn-sm me-1"
                                    @onclick="() => EditarRegistro(reg)">
                                Editar
                            </button>
                            <button class="btn btn-danger btn-sm"
                                    @onclick="() => EliminarRegistro(reg)">
                                Eliminar
                            </button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    @* ───────── MENSAJE CUANDO NO HAY DATOS ───────── *@
    @if (!cargando && !registros.Any())
    {
        <div class="alert alert-warning">No se encontraron registros en la tabla producto.</div>
    }
</div>

@code {
    // ───────── VARIABLES DE ESTADO ─────────

    // Lista de registros obtenidos de la API
    private List<Dictionary<string, object?>> registros = new();

    // Controla si se muestra el spinner de carga
    private bool cargando = true;

    // Controla si se muestra el formulario
    private bool mostrarFormulario = false;

    // Indica si estamos editando (true) o creando (false)
    private bool editando = false;

    // Mensaje de alerta para el usuario
    private string mensaje = string.Empty;

    // Indica si la ultima operacion fue exitosa (para color de alerta)
    private bool exito = false;

    // Campos del formulario
    private string campoCodigo = string.Empty;
    private string campoNombre = string.Empty;
    private int campoStock = 0;
    private double campoValor = 0;

    // ───────── CICLO DE VIDA ─────────

    // Se ejecuta al cargar la pagina por primera vez
    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    // ───────── METODOS ─────────

    // Obtiene todos los registros de la tabla producto
    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("producto");
        cargando = false;
    }

    // Muestra el formulario vacio para crear un nuevo registro
    private void NuevoRegistro()
    {
        editando = false;
        campoCodigo = string.Empty;
        campoNombre = string.Empty;
        campoStock = 0;
        campoValor = 0;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    // Llena el formulario con los datos del registro seleccionado
    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoCodigo = reg["codigo"]?.ToString() ?? "";
        campoNombre = reg["nombre"]?.ToString() ?? "";
        campoStock = int.TryParse(reg["stock"]?.ToString(), out int s) ? s : 0;
        campoValor = double.TryParse(reg["valorunitario"]?.ToString(), out double v) ? v : 0;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    // Crea o actualiza un registro segun el modo (editando o creando)
    private async Task GuardarRegistro()
    {
        // Arma el diccionario con los datos del formulario
        var datos = new Dictionary<string, object?>
        {
            ["codigo"] = campoCodigo,
            ["nombre"] = campoNombre,
            ["stock"] = campoStock,
            ["valorunitario"] = campoValor
        };

        if (editando)
        {
            // PUT /api/producto/codigo/{valor}
            // No incluimos el codigo en los datos porque es la clave
            datos.Remove("codigo");
            var resultado = await Api.ActualizarAsync("producto", "codigo", campoCodigo, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            // POST /api/producto
            var resultado = await Api.CrearAsync("producto", datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }

        if (exito)
        {
            mostrarFormulario = false;
            await CargarRegistros(); // Recargar la tabla
        }
    }

    // Elimina un registro despues de confirmacion
    private async Task EliminarRegistro(Dictionary<string, object?> reg)
    {
        string codigo = reg["codigo"]?.ToString() ?? "";

        // DELETE /api/producto/codigo/{valor}
        var resultado = await Api.EliminarAsync("producto", "codigo", codigo);
        exito = resultado.exito;
        mensaje = resultado.mensaje;

        if (exito)
        {
            await CargarRegistros(); // Recargar la tabla
        }
    }

    // Oculta el formulario sin guardar
    private void Cancelar()
    {
        mostrarFormulario = false;
        mensaje = string.Empty;
    }
}
```

---

## 5.3 Explicacion Seccion por Seccion

### Directivas (las 3 primeras lineas)

```razor
@page "/producto"
@rendermode InteractiveServer
@inject FrontBlazor_AppiGenericaCsharp.Services.ApiService Api
```

- `@page "/producto"` — Esta pagina se muestra cuando la URL es `http://localhost:5100/producto`
- `@rendermode InteractiveServer` — Activa la interactividad del lado del servidor. Sin esto, los botones y formularios no funcionarian (serian solo HTML estatico)
- `@inject ... Api` — Pide al contenedor de DI una instancia de `ApiService` y la asigna a la variable `Api`

### Alerta de exito o error

```razor
@if (!string.IsNullOrEmpty(mensaje))
{
    <div class="alert @(exito ? "alert-success" : "alert-danger") ...">
        @mensaje
    </div>
}
```

- Solo se muestra si hay un mensaje
- `@(exito ? "alert-success" : "alert-danger")` — Expresion ternaria de C# dentro de HTML. Si `exito` es `true`, usa la clase verde; si es `false`, usa la roja
- El boton `btn-close` limpia el mensaje al hacer click

### Formulario con @bind

```razor
<input class="form-control" @bind="campoCodigo" disabled="@editando" />
```

- `@bind="campoCodigo"` — Enlace bidireccional: lo que el usuario escribe se guarda en `campoCodigo`, y si `campoCodigo` cambia desde el codigo, el input se actualiza
- `disabled="@editando"` — Si estamos editando, el codigo se deshabilita (no se puede cambiar la clave primaria)
- `type="number"` en stock y valor permite solo numeros

### Tabla con @foreach

```razor
@foreach (var reg in registros)
{
    <tr>
        <td>@reg["codigo"]</td>
        ...
    </tr>
}
```

- `registros` es una `List<Dictionary<string, object?>>` que viene del ApiService
- `reg["codigo"]` accede al valor de la columna "codigo" del diccionario
- Cada fila tiene botones "Editar" y "Eliminar"

### Eventos con @onclick y lambdas

Una **lambda** es una funcion corta sin nombre. En C# se escribe con `() =>` y se lee como "ejecuta esto":

```csharp
// Funcion normal (con nombre):
private void Saludar()
{
    Console.WriteLine("Hola");
}

// Lambda (sin nombre, hace lo mismo):
() => Console.WriteLine("Hola")
```

En Blazor las lambdas son necesarias cuando un evento necesita pasar parametros:

```razor
@* SIN parametro — se pasa el nombre del metodo directamente *@
<button @onclick="NuevoRegistro">Nuevo</button>

@* CON parametro — se necesita lambda *@
<button @onclick="() => EditarRegistro(reg)">Editar</button>
```

**Por que no se puede escribir `@onclick="EditarRegistro(reg)"` directamente?**
Porque eso ejecutaria `EditarRegistro(reg)` inmediatamente al renderizar la pagina, no al hacer click. La lambda `() =>` le dice a Blazor: "no ejecutes esto ahora, guardalo y ejecutalo solo cuando el usuario haga click".

```
Sin lambda:  @onclick="EditarRegistro(reg)"     → se ejecuta AL CARGAR (mal)
Con lambda:  @onclick="() => EditarRegistro(reg)" → se ejecuta AL HACER CLICK (bien)
```

### Guardar: Crear vs Actualizar

```csharp
if (editando)
{
    datos.Remove("codigo"); // No se envia la clave en el body del PUT
    var resultado = await Api.ActualizarAsync("producto", "codigo", campoCodigo, datos);
}
else
{
    var resultado = await Api.CrearAsync("producto", datos);
}
```

- Si estamos editando, se hace **PUT** y se remueve el codigo del body (la API lo recibe en la URL)
- Si estamos creando, se hace **POST** con todos los campos incluyendo el codigo

---

## 5.4 Flujo de Cada Operacion

### Listar (al abrir la pagina)
```
OnInitializedAsync() → Api.ListarAsync("producto") → GET /api/producto → tabla con datos
```

### Crear (click en "Nuevo Producto" → llenar formulario → "Guardar")
```
NuevoRegistro() → mostrarFormulario = true → usuario llena campos
GuardarRegistro() → Api.CrearAsync("producto", datos) → POST /api/producto
Si exito → ocultar formulario, recargar tabla, mostrar alerta verde
```

### Editar (click en "Editar" de una fila → modificar campos → "Guardar")
```
EditarRegistro(reg) → llena campos con datos existentes → editando = true
GuardarRegistro() → Api.ActualizarAsync("producto", "codigo", valor, datos) → PUT /api/producto/codigo/PR001
Si exito → ocultar formulario, recargar tabla, mostrar alerta verde
```

### Eliminar (click en "Eliminar" de una fila)
```
EliminarRegistro(reg) → Api.EliminarAsync("producto", "codigo", valor) → DELETE /api/producto/codigo/PR001
Si exito → recargar tabla, mostrar alerta verde
```

---

## 5.5 Commit

```powershell
git add .
git commit -m "Agregar CRUD completo de Producto con tabla, formulario y alertas"
git push
```

---

## 5.6 Resumen de Conceptos de esta Parte

| Concepto | Que hace | Ejemplo |
|---|---|---|
| `@page` | Define la URL de la pagina | `@page "/producto"` |
| `@rendermode InteractiveServer` | Activa botones, formularios y eventos | Sin esto, la pagina es solo HTML estatico |
| `@inject` | Inyecta un servicio para usarlo en la pagina | `@inject ApiService Api` |
| Alerta con ternario | Cambia el color segun exito o error | `@(exito ? "alert-success" : "alert-danger")` |
| `@bind` | Enlace bidireccional input ↔ variable C# | `<input @bind="campoNombre" />` |
| `@foreach` | Recorre una lista para generar filas HTML | `@foreach (var reg in registros)` |
| Lambda en `@onclick` | Ejecuta un metodo con parametros al hacer click | `@onclick="() => EditarRegistro(reg)"` |
| POST vs PUT | POST crea un registro nuevo, PUT modifica uno existente | `CrearAsync` vs `ActualizarAsync` |
| `OnInitializedAsync` | Carga datos al abrir la pagina (ciclo de vida) | `await Api.ListarAsync("producto")` |
| Spinner | Indicador visual mientras se cargan datos | `<div class="spinner-border">` |

---

## Siguiente Parte

En la **Parte 6** crearemos las paginas CRUD para las 5 tablas restantes (Empresa, Persona, Rol, Ruta, Usuario) siguiendo el mismo patron de Producto.
