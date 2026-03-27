using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace FieldTechApi.Services
{
    public class Utilitario(IConfiguration _config) : IUtilitario
    {
        public string Encrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("G7kP2mX9Qa4ZtL8wR1bY6HcD3sN5uFjV");
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(texto);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        public string Decrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(texto);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("G7kP2mX9Qa4ZtL8wR1bY6HcD3sN5uFjV");
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            return streamReader.ReadToEnd();
        }

        public void EnviarCorreo(string destinatario, string asunto, string contenido)
        {
            var host = _config.GetValue<string>("ConfiguracionCorreo:Host")!;
            var puerto = _config.GetValue<int>("ConfiguracionCorreo:Puerto");
            var remitente = _config.GetValue<string>("ConfiguracionCorreo:Remitente")!;
            var contrasenna = _config.GetValue<string>("ConfiguracionCorreo:Contrasenna")!;

            var mensaje = new MailMessage(remitente, destinatario, asunto, contenido);
            mensaje.IsBodyHtml = true;

            using var smtp = new SmtpClient(host, puerto);
            smtp.Credentials = new NetworkCredential(remitente, contrasenna);
            smtp.EnableSsl = true;

            if (!string.IsNullOrEmpty(contrasenna))
            {
                smtp.Send(mensaje);
            }
        }
    }

    public interface IUtilitario
    {
        string Encrypt(string texto);
        string Decrypt(string texto);
        void EnviarCorreo(string destinatario, string asunto, string contenido);
    }
}
