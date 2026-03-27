$(function () {
    $('#formPerfil').validate({
        rules: {
            HourlyRate: {
                min: 0,
                number: true
            },
            PortfolioUrl: {
                url: true
            },
            ContactPhone: {
                minlength: 8
            }
        },
        messages: {
            HourlyRate: {
                min: 'La tarifa no puede ser negativa.',
                number: 'Ingrese un valor numérico.'
            },
            PortfolioUrl: {
                url: 'Ingrese una URL válida (https://...).'
            },
            ContactPhone: {
                minlength: 'Ingrese un teléfono válido.'
            }
        },
        errorPlacement: function (error, element) {
            error.addClass('text-danger').css('font-size', '.82rem');
            error.insertAfter(element);
        },
        highlight: function (element) {
            $(element).css('border-color', 'rgba(239,68,68,.55)');
        },
        unhighlight: function (element) {
            $(element).css('border-color', '');
        }
    });
});
document.addEventListener('DOMContentLoaded', function () {
  const input = document.getElementById('avatarInput');
  const preview = document.getElementById('avatarPreview');
  if (!input || !preview) return;

  input.addEventListener('change', function () {
    const file = this.files && this.files[0];
    if (!file) return;

    // Validación básica en cliente
    if (!file.type.startsWith('image/')) {
      alert('Seleccione un archivo de imagen válido.');
      return;
    }
    const maxBytes = 5 * 1024 * 1024; // 5 MB
    if (file.size > maxBytes) {
      alert('La imagen debe ser menor a 5 MB.');
      return;
    }

    // Previsualizar
    try {
      preview.src = URL.createObjectURL(file);
    } catch (e) {
      console.error(e);
    }
  });
});
