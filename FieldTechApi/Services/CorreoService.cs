using System.Net;
using System.Net.Mail;

namespace FieldTechApi.Services
{
    public class CorreoService(IConfiguration _config) : ICorreoService
    {
        public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var host = _config.GetValue<string>("ConfiguracionCorreo:Host")!;
            var puerto = _config.GetValue<int>("ConfiguracionCorreo:Puerto");
            var remitente = _config.GetValue<string>("ConfiguracionCorreo:Remitente")!;
            var contrasenna = _config.GetValue<string>("ConfiguracionCorreo:Contrasenna")!;

            using var mensaje = new MailMessage();
            mensaje.From = new MailAddress(remitente, "FieldTech");
            mensaje.To.Add(destinatario);
            mensaje.Subject = asunto;
            mensaje.Body = cuerpoHtml;
            mensaje.IsBodyHtml = true;

            using var smtp = new SmtpClient(host, puerto);
            smtp.Credentials = new NetworkCredential(remitente, contrasenna);
            smtp.EnableSsl = true;

            await smtp.SendMailAsync(mensaje);
        }
    }
}