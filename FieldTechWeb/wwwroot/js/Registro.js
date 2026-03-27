$(function () {
    $('#formRegistro').validate({
        rules: {
            Nombre: {
                required: true,
                minlength: 2
            },
            Apellido: {
                required: true,
                minlength: 2
            },
            Email: {
                required: true,
                email: true
            },
            Contrasenna: {
                required: true,
                minlength: 8
            },
            ConfirmarContrasenna: {
                required: true,
                equalTo: '#Contrasenna'
            }
        },
        messages: {
            Nombre: {
                required: 'Ingrese su nombre.',
                minlength: 'Mínimo 2 caracteres.'
            },
            Apellido: {
                required: 'Ingrese su apellido.',
                minlength: 'Mínimo 2 caracteres.'
            },
            Email: {
                required: 'Ingrese su correo electrónico.',
                email: 'Ingrese un correo válido.'
            },
            Contrasenna: {
                required: 'Ingrese una contraseña.',
                minlength: 'La contraseña debe tener al menos 8 caracteres.'
            },
            ConfirmarContrasenna: {
                required: 'Confirme su contraseña.',
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
