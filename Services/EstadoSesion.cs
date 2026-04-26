namespace FrontBlazor_AppiGenericaCsharp.Services
{
    /// <summary>
    /// Singleton que guarda el estado de sesión entre navegaciones.
    /// Solo guarda datos simples, no depende de HttpClient.
    /// </summary>
    public class EstadoSesion
    {
        public bool EstaAutenticado { get; set; } = false;
        public string EmailUsuario  { get; set; } = "";
        public string TokenJwt      { get; set; } = "";
        public bool DebeCambiarContrasena { get; set; } = false;
        public List<string> RutasPermitidas { get; set; } = new();

        public void Limpiar()
        {
            EstaAutenticado       = false;
            EmailUsuario          = "";
            TokenJwt              = "";
            DebeCambiarContrasena = false;
            RutasPermitidas       = new();
        }
    }
}