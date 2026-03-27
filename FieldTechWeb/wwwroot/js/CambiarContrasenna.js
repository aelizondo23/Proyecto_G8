$(function () {
    $('#formCambiarContrasenna').validate({
        rules: {
            NuevaContrasenna: {
                required: true,
                minlength: 8
            },
            ConfirmarContrasenna: {
                required: true,
                equalTo: '#NuevaContrasenna'
            }
        },
        messages: {
            NuevaContrasenna: {
                required: 'Ingrese la nueva contraseña.',
                minlength: 'Mínimo 8 caracteres.'
            },
            ConfirmarContrasenna: {
                required: 'Confirme la nueva contraseña.',
                equalTo: 'Las contraseñas no coinciden.'
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
