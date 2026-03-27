$(function () {
    $('#formRecuperar').validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: 'Ingrese su correo electrónico.',
                email: 'Ingrese un correo válido.'
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
