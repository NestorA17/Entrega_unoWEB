using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    /// <summary>
    /// Servicio de autenticación que maneja login, sesión, roles y rutas permitidas.
    /// Usa EstadoSesion (Singleton) para persistir el estado entre navegaciones.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly EstadoSesion _estado;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // ─── PROPIEDADES QUE LEEN DEL ESTADO SINGLETON ───
        public bool EstaAutenticado           => _estado.EstaAutenticado;
        public string EmailUsuario            => _estado.EmailUsuario;
        public string TokenJwt                => _estado.TokenJwt;
        public bool DebeCambiarContrasena     => _estado.DebeCambiarContrasena;
        public List<string> RutasPermitidas   => _estado.RutasPermitidas;

        public AuthService(HttpClient http, EstadoSesion estado)
        {
            _http   = http;
            _estado = estado;
        }

        // ──────────────────────────────────────────────────
        // LOGIN
        // ──────────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> Login(string email, string contrasena)
        {
            try
            {
                // 1. Verificar credenciales con BCrypt
                var bodyVerificar = new Dictionary<string, object?>
                {
                    ["campoUsuario"]    = "email",
                    ["campoContrasena"] = "contrasena",
                    ["valorUsuario"]    = email,
                    ["valorContrasena"] = contrasena
                };

                var respVerificar = await _http.PostAsJsonAsync(
                    "/api/usuario/verificar-contrasena", bodyVerificar);

                if (!respVerificar.IsSuccessStatusCode)
                {
                    if (respVerificar.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return (false, "Contraseña incorrecta.");
                    if (respVerificar.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return (false, "Usuario no encontrado.");
                    return (false, "Error al iniciar sesión.");
                }

                // 2. Obtener token JWT
                string token = "";
                try
                {
                    var bodyToken = new Dictionary<string, object?>
                    {
                        ["email"]      = email,
                        ["contrasena"] = contrasena
                    };

                    var respToken = await _http.PostAsJsonAsync(
                        "/api/Autenticacion/token", bodyToken);

                    if (respToken.IsSuccessStatusCode)
                    {
                        var contenidoToken = await respToken.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(contenidoToken))
                        {
                            var jsonToken = JsonSerializer.Deserialize<JsonElement>(
                                contenidoToken, _jsonOptions);
                            if (jsonToken.TryGetProperty("token", out var t))
                                token = t.GetString() ?? "";
                        }
                    }
                }
                catch
                {
                    // Si falla el token JWT continuamos igual
                }

                // 3. Obtener rutas permitidas del usuario
                var rutas = await ObtenerRutasUsuario(email);

                // Si no tiene rutas asignadas dar acceso básico
                if (!rutas.Any())
                    rutas.Add("/");

                // 4. Guardar en el estado singleton
                _estado.EstaAutenticado       = true;
                _estado.EmailUsuario          = email;
                _estado.TokenJwt              = token;
                _estado.RutasPermitidas       = rutas;
                _estado.DebeCambiarContrasena = false;

                return (true, "Sesión iniciada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Login: {ex.Message}");
                return (false, "Error de conexión con el servidor.");
            }
        }

        // ──────────────────────────────────────────────────
        // OBTENER RUTAS DEL USUARIO SEGÚN SU ROL
        // ──────────────────────────────────────────────────
        private async Task<List<string>> ObtenerRutasUsuario(string email)
        {
            var rutas = new List<string>();

            try
            {
                // Obtener roles del usuario
                var rolesResp = await _http.GetAsync(
                    $"/api/rol_usuario/fkemail/{Uri.EscapeDataString(email)}");

                if (!rolesResp.IsSuccessStatusCode) return rutas;

                var contenidoRoles = await rolesResp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(contenidoRoles)) return rutas;

                var jsonRoles = JsonSerializer.Deserialize<JsonElement>(
                    contenidoRoles, _jsonOptions);

                if (!jsonRoles.TryGetProperty("datos", out var datos)) return rutas;

                foreach (var item in datos.EnumerateArray())
                {
                    if (!item.TryGetProperty("fkidrol", out var idRol)) continue;

                    string idRolStr = idRol.ToString();

                    // Obtener rutas de cada rol
                    var rutasResp = await _http.GetAsync(
                        $"/api/rutarol/fkidrol/{idRolStr}");

                    if (!rutasResp.IsSuccessStatusCode) continue;

                    var contenidoRutas = await rutasResp.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(contenidoRutas)) continue;

                    var jsonRutas = JsonSerializer.Deserialize<JsonElement>(
                        contenidoRutas, _jsonOptions);

                    if (!jsonRutas.TryGetProperty("datos", out var datosRutas)) continue;

                    foreach (var ruta in datosRutas.EnumerateArray())
                    {
                        if (!ruta.TryGetProperty("fkidruta", out var idRuta)) continue;

                        // Obtener detalle de la ruta
                        var rutaDetalleResp = await _http.GetAsync(
                            $"/api/ruta/id/{idRuta}");

                        if (!rutaDetalleResp.IsSuccessStatusCode) continue;

                        var contenidoRutaDetalle = await rutaDetalleResp.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(contenidoRutaDetalle)) continue;

                        var jsonRutaDetalle = JsonSerializer.Deserialize<JsonElement>(
                            contenidoRutaDetalle, _jsonOptions);

                        if (!jsonRutaDetalle.TryGetProperty("datos", out var datosRutaDetalle))
                            continue;

                        foreach (var r in datosRutaDetalle.EnumerateArray())
                        {
                            if (r.TryGetProperty("ruta", out var rutaVal))
                            {
                                var rutaStr = rutaVal.GetString() ?? "";
                                if (!string.IsNullOrWhiteSpace(rutaStr) &&
                                    !rutas.Contains(rutaStr))
                                    rutas.Add(rutaStr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo rutas: {ex.Message}");
            }

            return rutas;
        }

        // ──────────────────────────────────────────────────
        // CAMBIAR CONTRASEÑA
        // ──────────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> CambiarContrasena(string nuevaContrasena)
        {
            try
            {
                var datos = new Dictionary<string, object?>
                {
                    ["contrasena"] = nuevaContrasena
                };

                var respuesta = await _http.PutAsJsonAsync(
                    $"/api/usuario/email/{Uri.EscapeDataString(_estado.EmailUsuario)}?camposEncriptar=contrasena",
                    datos);

                if (respuesta.IsSuccessStatusCode)
                {
                    _estado.DebeCambiarContrasena = false;
                    return (true, "Contraseña actualizada correctamente.");
                }

                return (false, "No se pudo actualizar la contraseña.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────────
        // RECUPERAR CONTRASEÑA
        // ──────────────────────────────────────────────────
        public async Task<(bool exito, string mensaje, string contrasenaTemp)> RecuperarContrasena(
            string email)
        {
            try
            {
                // Verificar que el usuario existe
                var respUsuario = await _http.GetAsync(
                    $"/api/usuario/email/{Uri.EscapeDataString(email)}");

                if (!respUsuario.IsSuccessStatusCode)
                    return (false, "No se encontró un usuario con ese correo.", "");

                // Generar contraseña temporal
                string temporal = GenerarContrasenaTemp();

                var datos = new Dictionary<string, object?>
                {
                    ["contrasena"] = temporal
                };

                var respUpdate = await _http.PutAsJsonAsync(
                    $"/api/usuario/email/{Uri.EscapeDataString(email)}?camposEncriptar=contrasena",
                    datos);

                if (respUpdate.IsSuccessStatusCode)
                    return (true, "Contraseña temporal generada.", temporal);

                return (false, "No se pudo generar la contraseña temporal.", "");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", "");
            }
        }

        // ──────────────────────────────────────────────────
        // VERIFICAR ACCESO A RUTA
        // ──────────────────────────────────────────────────
        public bool TieneAcceso(string ruta)
        {
            if (!_estado.EstaAutenticado) return false;
            return _estado.RutasPermitidas.Contains(ruta) ||
                   _estado.RutasPermitidas.Contains("/");
        }

        // ──────────────────────────────────────────────────
        // CERRAR SESIÓN
        // ──────────────────────────────────────────────────
        public void Logout()
        {
            _estado.Limpiar();
        }

        // ──────────────────────────────────────────────────
        // HELPER: Genera contraseña temporal
        // ──────────────────────────────────────────────────
        private string GenerarContrasenaTemp()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}