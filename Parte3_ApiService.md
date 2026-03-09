# Tutorial: Frontend Blazor CRUD
# Parte 3: Servicio Generico para la API (ApiService)

En esta parte creamos un servicio reutilizable que encapsula todas las llamadas HTTP a la API. Todas las paginas CRUD lo usaran para comunicarse con el backend.

---

## 3.1 Por que un Servicio

En lugar de que cada pagina haga sus propias llamadas HTTP directamente, centralizamos esa logica en un solo lugar:

```
SIN servicio (malo):                    CON servicio (bueno):
┌──────────────┐                        ┌──────────────┐
│ Producto.razor│──► HttpClient          │ Producto.razor│──┐
└──────────────┘                        └──────────────┘  │
┌──────────────┐                        ┌──────────────┐  │   ┌────────────┐       ┌─────┐
│ Empresa.razor │──► HttpClient          │ Empresa.razor │──┼──►│ ApiService │──────►│ API │
└──────────────┘                        └──────────────┘  │   └────────────┘       └─────┘
┌──────────────┐                        ┌──────────────┐  │
│ Persona.razor │──► HttpClient          │ Persona.razor │──┘
└──────────────┘                        └──────────────┘
Codigo repetido en cada pagina           Codigo en un solo lugar
```

**Ventajas:**
- Si la URL de la API cambia, se modifica en un solo archivo
- Si el formato de respuesta cambia, se ajusta una sola vez
- Las paginas quedan mas limpias: solo logica de interfaz

---

## 3.2 Estructura de las Respuestas de la API

Antes de escribir el servicio, necesitamos entender que devuelve la API.

**Respuesta de GET** (listar registros):
```json
{
    "tabla": "producto",
    "esquema": "por defecto",
    "limite": null,
    "total": 3,
    "datos": [
        { "codigo": "PR001", "nombre": "Laptop", "stock": 10, "valorunitario": 2500000 },
        { "codigo": "PR002", "nombre": "Mouse", "stock": 50, "valorunitario": 35000 },
        { "codigo": "PR003", "nombre": "Teclado", "stock": 30, "valorunitario": 75000 }
    ]
}
```

Lo que nos interesa es la propiedad `datos`, que es una lista de objetos. Cada objeto es un `Dictionary<string, object>` porque las columnas varian segun la tabla.

**Respuesta de POST, PUT, DELETE** (crear, actualizar, eliminar):
```json
{
    "estado": 200,
    "mensaje": "Registro creado exitosamente.",
    "tabla": "producto"
}
```

Aqui lo que nos interesa es `estado` (200 = exito) y `mensaje` (para mostrar al usuario).

---

## 3.3 Crear la Carpeta Services

Creamos la carpeta `Services` en la raiz del proyecto:

```powershell
mkdir Services
```

Dentro de esa carpeta crearemos el archivo `ApiService.cs`.

---

## 3.4 Codigo del ApiService

Creamos el archivo `Services/ApiService.cs` con el siguiente contenido:

```csharp
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    // Servicio generico que consume la API REST para cualquier tabla.
    // Se inyecta en las paginas Blazor con @inject ApiService Api
    public class ApiService
    {
        // HttpClient configurado en Program.cs con la URL base de la API
        private readonly HttpClient _http;

        // Opciones para deserializar JSON sin distinguir mayusculas/minusculas
        // La API devuelve "datos", "estado", etc. en minuscula
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // El constructor recibe el HttpClient inyectado por DI
        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // ──────────────────────────────────────────────
        // LISTAR: GET /api/{tabla}
        // Devuelve la lista de registros como diccionarios
        // ──────────────────────────────────────────────
        public async Task<List<Dictionary<string, object?>>> ListarAsync(string tabla)
        {
            try
            {
                // Hace GET a la API y obtiene la respuesta como JSON
                var respuesta = await _http.GetFromJsonAsync<JsonElement>($"/api/{tabla}", _jsonOptions);

                // Extrae la propiedad "datos" de la respuesta
                if (respuesta.TryGetProperty("datos", out JsonElement datos))
                {
                    return ConvertirDatos(datos);
                }

                return new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al listar {tabla}: {ex.Message}");
                return new List<Dictionary<string, object?>>();
            }
        }

        // ──────────────────────────────────────────────
        // CREAR: POST /api/{tabla}
        // Envia los datos del formulario como JSON
        // Devuelve (exito, mensaje) para mostrar al usuario
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> CrearAsync(
            string tabla, Dictionary<string, object?> datos)
        {
            try
            {
                var respuesta = await _http.PostAsJsonAsync($"/api/{tabla}", datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ACTUALIZAR: PUT /api/{tabla}/{clave}/{valor}
        // Envia los campos a modificar como JSON
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> ActualizarAsync(
            string tabla, string nombreClave, string valorClave,
            Dictionary<string, object?> datos)
        {
            try
            {
                var respuesta = await _http.PutAsJsonAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}", datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ELIMINAR: DELETE /api/{tabla}/{clave}/{valor}
        // Solo necesita la clave primaria para identificar el registro
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> EliminarAsync(
            string tabla, string nombreClave, string valorClave)
        {
            try
            {
                var respuesta = await _http.DeleteAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}");
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // METODO AUXILIAR: Convierte JsonElement a lista de diccionarios
        // La API devuelve los datos como JSON generico, este metodo
        // lo transforma a Dictionary<string, object?> para trabajar
        // facilmente con @foreach y @bind en Blazor
        // ──────────────────────────────────────────────
        private List<Dictionary<string, object?>> ConvertirDatos(JsonElement datos)
        {
            var lista = new List<Dictionary<string, object?>>();

            foreach (var fila in datos.EnumerateArray())
            {
                var diccionario = new Dictionary<string, object?>();

                foreach (var propiedad in fila.EnumerateObject())
                {
                    // Convierte cada valor JSON a su tipo .NET correspondiente
                    diccionario[propiedad.Name] = propiedad.Value.ValueKind switch
                    {
                        JsonValueKind.String => propiedad.Value.GetString(),
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i) ? i : propiedad.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => propiedad.Value.GetRawText()
                    };
                }

                lista.Add(diccionario);
            }

            return lista;
        }
    }
}
```

---

## 3.5 Explicacion del Codigo

### Constructor y HttpClient

```csharp
private readonly HttpClient _http;

public ApiService(HttpClient http)
{
    _http = http;
}
```

El `HttpClient` se recibe por inyeccion de dependencias. Ya viene configurado con la URL base `http://localhost:5034` (lo configuramos en Program.cs en la Parte 2).

### Tipo de retorno: Tupla (bool, string)

```csharp
public async Task<(bool exito, string mensaje)> CrearAsync(...)
```

Los metodos de crear, actualizar y eliminar devuelven una **tupla** con dos valores:
- `exito` — `true` si la operacion fue exitosa, `false` si fallo
- `mensaje` — Texto de la API para mostrar al usuario ("Registro creado exitosamente." o el error)

Esto permite en la pagina hacer:

```csharp
var (exito, mensaje) = await Api.CrearAsync("producto", datos);
if (exito)
    // mostrar alerta verde
else
    // mostrar alerta roja con el mensaje de error
```

### ConvertirDatos: De JSON a Diccionarios

La API devuelve datos genericos (cualquier tabla, cualquier columna). No podemos crear una clase `Producto` porque las columnas cambian segun la tabla. Por eso usamos `Dictionary<string, object?>`:

```csharp
// Un registro de producto seria:
{ "codigo": "PR001", "nombre": "Laptop", "stock": 10, "valorunitario": 2500000 }

// Se convierte a:
Dictionary<string, object?> {
    ["codigo"] = "PR001",
    ["nombre"] = "Laptop",
    ["stock"] = 10,
    ["valorunitario"] = 2500000.0
}
```

---

## 3.6 Registrar el Servicio en Program.cs

Para que Blazor pueda inyectar el `ApiService` en las paginas, debemos registrarlo en `Program.cs`. Agregamos una linea despues del registro del HttpClient:

```csharp
// Configurar HttpClient para conectarse a la API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5034")
});

// Registrar el servicio generico de la API
builder.Services.AddScoped<FrontBlazor_AppiGenericaCsharp.Services.ApiService>();
```

Con `AddScoped`, Blazor crea una instancia de `ApiService` por cada sesion de usuario y la inyecta automaticamente cuando una pagina usa `@inject ApiService Api`.

---

## 3.7 Commit

```powershell
git add .
git commit -m "Agregar ApiService: servicio generico para consumir la API CRUD"
git push
```

---

## Siguiente Parte

En la **Parte 4** modificaremos el menu de navegacion (NavMenu.razor) para agregar links a las 6 tablas, y actualizaremos el layout.
