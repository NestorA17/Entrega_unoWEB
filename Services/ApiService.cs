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
       public async Task<List<Dictionary<string, object?>>> ListarAsync(string tabla, int? limite = null)
{
    try
    {
        string url = $"/api/{tabla}";
        if (limite.HasValue)
            url += $"?limite={limite.Value}";

        var respuesta = await _http.GetAsync(url);

        // Si la respuesta es 204 (sin contenido) retorna lista vacía
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NoContent)
            return new List<Dictionary<string, object?>>();

        // Si no fue exitosa, retorna lista vacía
        if (!respuesta.IsSuccessStatusCode)
            return new List<Dictionary<string, object?>>();

        var contenido = await respuesta.Content.ReadAsStringAsync();

        // Si el contenido está vacío, retorna lista vacía
        if (string.IsNullOrWhiteSpace(contenido))
            return new List<Dictionary<string, object?>>();

        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
            contenido, _jsonOptions);

        if (json.TryGetProperty("datos", out System.Text.Json.JsonElement datos))
            return ConvertirDatos(datos);

        return new List<Dictionary<string, object?>>();
    }
    catch (Exception ex)
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
            string tabla, Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PostAsJsonAsync(url, datos);
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
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}/{nombreClave}/{valorClave}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PutAsJsonAsync(url, datos);
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
        // DIAGNOSTICO: GET /api/diagnostico/conexion
        // Devuelve info del servidor de BD conectado
        // ──────────────────────────────────────────────
        public async Task<Dictionary<string, string>?> ObtenerDiagnosticoAsync()
        {
            try
            {
                var respuesta = await _http.GetFromJsonAsync<JsonElement>(
                    "/api/diagnostico/conexion", _jsonOptions);

                if (respuesta.TryGetProperty("servidor", out JsonElement servidor))
                {
                    var info = new Dictionary<string, string>();
                    foreach (var prop in servidor.EnumerateObject())
                    {
                        info[prop.Name] = prop.Value.ToString();
                    }
                    return info;
                }

                return null;
            }
            catch
            {
                return null;
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
