$(document).ready(function() {
    $("#vehicleForm").validate({
        errorElement: "span",
        errorClass: "field-validation-error",
        errorPlacement: function(error, element) {
            error.attr("data-valmsg-for", element.attr("name"));
            error.insertAfter(element);
        },
        highlight: function(element) {
            $(element).addClass("input-validation-error");
        },
        unhighlight: function(element) {
            $(element).removeClass("input-validation-error");
        }
    });

    // Función para mostrar errores
    window.showErrors = function(errors) {
        const errorDiv = $('#errorMessages');
        errorDiv.empty();
        
        if (typeof errors === 'string') {
            errorDiv.append(`<p class="mb-0">${errors}</p>`);
        } else if (Array.isArray(errors)) {
            const ul = $('<ul class="mb-0"></ul>');
            errors.forEach(error => ul.append(`<li>${error}</li>`));
            errorDiv.append(ul);
        }
        
        $('#errorAlert').removeClass('d-none').addClass('show');
    };

    $.validator.addMethod(
        "maxYear",
        function(value, element) {
            return this.optional(element) || value <= new Date().getFullYear();
        },
        "El año no puede ser mayor al año actual"
    );

    $("form").validate({
        rules: {
            Year: {
                maxYear: true
            }
        }
    });
}); 