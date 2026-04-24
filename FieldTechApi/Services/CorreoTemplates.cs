namespace FieldTechApi.Services
{
    public static class CorreoTemplates
    {
        private static string Base(string contenido)
        {
            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<style>
body{{font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:0}}
.container{{max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1)}}
.header{{background:#6c47ff;padding:28px 32px}}
.header h1{{color:#fff;margin:0;font-size:22px}}
.header p{{color:rgba(255,255,255,.8);margin:4px 0 0;font-size:13px}}
.body{{padding:32px;color:#333;line-height:1.6}}
.body h2{{color:#6c47ff;margin-top:0}}
.info-box{{background:#f8f7ff;border-left:4px solid #6c47ff;padding:16px 20px;border-radius:6px;margin:20px 0}}
.info-box p{{margin:4px 0;font-size:14px}}
.footer{{background:#f8f7ff;padding:16px 32px;text-align:center;font-size:12px;color:#888}}
</style>
</head>
<body>
<div class='container'>
<div class='header'>
<h1>⚡ FieldTech</h1>
<p>Plataforma de técnicos freelance</p>
</div>
<div class='body'>
{contenido}
</div>
<div class='footer'>© {DateTime.Now.Year} FieldTech — Correo automático, no responder.</div>
</div>
</body>
</html>";
        }

        public static string TecnicoAplicoOrden(string nombreCliente, string tituloOrden, string nombreTecnico)
        {
            var contenido = $@"<h2>¡Un técnico aplicó a tu orden!</h2>
<p>Hola <strong>{nombreCliente}</strong>,</p>
<p>El técnico <strong>{nombreTecnico}</strong> ha aplicado a tu orden de trabajo.</p>
<div class='info-box'>
<p><strong>Orden:</strong> {tituloOrden}</p>
<p><strong>Técnico:</strong> {nombreTecnico}</p>
</div>
<p>Ingresa a FieldTech para revisar su perfil y asignarlo si te parece adecuado.</p>";
            return Base(contenido);
        }

        public static string OrdenAsignada(string nombreTecnico, string tituloOrden, string nombreCliente, string ubicacion)
        {
            var contenido = $@"<h2>¡Te han asignado una orden!</h2>
<p>Hola <strong>{nombreTecnico}</strong>,</p>
<p>Has sido asignado a una nueva orden de trabajo.</p>
<div class='info-box'>
<p><strong>Orden:</strong> {tituloOrden}</p>
<p><strong>Cliente:</strong> {nombreCliente}</p>
<p><strong>Ubicación:</strong> {ubicacion}</p>
</div>
<p>Ingresa a FieldTech para ver todos los detalles.</p>";
            return Base(contenido);
        }

        public static string NuevoMensaje(string nombreDestinatario, string nombreRemitente, string tituloOrden, string preview)
        {
            var contenido = $@"<h2>Nuevo mensaje en tu orden</h2>
<p>Hola <strong>{nombreDestinatario}</strong>,</p>
<p><strong>{nombreRemitente}</strong> te ha enviado un mensaje en la orden <strong>{tituloOrden}</strong>.</p>
<div class='info-box'>
<p><em>""{preview}""</em></p>
</div>
<p>Ingresa a FieldTech para responder.</p>";
            return Base(contenido);
        }

        public static string OrdenCreada(string nombreCliente, string tituloOrden, string? categoria, string? ubicacion, decimal? presupuesto)
        {
            var presupuestoStr = presupuesto.HasValue ? $"₡{presupuesto:N0}" : "Por definir";
            var contenido = $@"<h2>Tu orden fue publicada</h2>
<p>Hola <strong>{nombreCliente}</strong>,</p>
<p>Tu orden de trabajo ha sido publicada exitosamente.</p>
<div class='info-box'>
<p><strong>Orden:</strong> {tituloOrden}</p>
<p><strong>Categoría:</strong> {categoria ?? "—"}</p>
<p><strong>Ubicación:</strong> {ubicacion ?? "—"}</p>
<p><strong>Presupuesto:</strong> {presupuestoStr}</p>
</div>
<p>Te notificaremos cuando un técnico aplique.</p>";
            return Base(contenido);
        }

        public static string FechaAgendada(string nombreDestinatario, string tituloOrden, DateTime inicio, DateTime fin)
        {
            var contenido = $@"<h2>Fecha agendada para tu orden</h2>
<p>Hola <strong>{nombreDestinatario}</strong>,</p>
<p>Se ha agendado una fecha para la orden <strong>{tituloOrden}</strong>.</p>
<div class='info-box'>
<p><strong>Inicio:</strong> {inicio:dd/MM/yyyy HH:mm}</p>
<p><strong>Fin estimado:</strong> {fin:dd/MM/yyyy HH:mm}</p>
</div>
<p>Ingresa a FieldTech para ver más detalles.</p>";
            return Base(contenido);
        }

        public static string RecuperarAcceso(string nombreUsuario, string nuevaContrasenna)
        {
            var contenido = $@"<h2>Recuperación de acceso</h2>
<p>Hola <strong>{nombreUsuario}</strong>,</p>
<p>Recibimos una solicitud para restablecer tu contraseña.</p>
<div class='info-box'>
<p><strong>Tu nueva contraseña temporal es:</strong></p>
<p style='font-size:20px;font-weight:bold;color:#6c47ff;letter-spacing:2px'>{nuevaContrasenna}</p>
</div>
<p>Por seguridad, te recomendamos cambiarla después de iniciar sesión.</p>";
            return Base(contenido);
        }
    }
}