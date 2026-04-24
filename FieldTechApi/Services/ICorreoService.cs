namespace FieldTechApi.Services
{
    public interface ICorreoService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
    }
}