$(document).ready(function() {
    $.validator.addMethod("vinCode", function(value, element) {
        return this.optional(element) || (value.length >= 14 && value.length <= 17);
    }, "El código VIN debe tener entre 14 y 17 caracteres");

    $.validator.addMethod("licensePlate", function(value, element) {
        return this.optional(element) || (value.length >= 6 && value.length <= 8);
    }, "La patente debe tener entre 6 y 8 caracteres");

    $.validator.addMethod("maxCurrentYear", function(value, element) {
        return this.optional(element) || value <= new Date().getFullYear();
    }, "El año no puede ser mayor al actual");

    $("form").validate({
        rules: {
            Country: "required",
            Brand: "required",
            Model: "required",
            Year: {
                required: true,
                number: true,
                min: 1900,
                maxCurrentYear: true
            },
            LicensePlate: {
                required: true,
                licensePlate: true
            },
            VinCode: {
                required: true,
                vinCode: true
            }
        },
        messages: {
            Country: "Por favor seleccione un país",
            Brand: "Por favor ingrese la marca",
            Model: "Por favor ingrese el modelo",
            Year: {
                required: "Por favor ingrese el año",
                number: "Por favor ingrese un año válido",
                min: "El año debe ser mayor a 1900"
            },
            LicensePlate: "Por favor ingrese una patente válida",
            VinCode: "Por favor ingrese un código VIN válido"
        },
        errorElement: "span",
        errorClass: "text-danger"
    });
}); 