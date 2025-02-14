// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Mostrar alerta de error
function showError(message) {
    const errorDiv = document.getElementById('errorMessages');
    const alertDiv = errorDiv.closest('.alert');
    
    errorDiv.textContent = message;
    alertDiv.classList.remove('d-none');
    alertDiv.classList.add('show');
}

// Auto-ocultar después de 5 segundos
setTimeout(() => {
    const alertDiv = document.querySelector('.alert');
    if (alertDiv) {
        alertDiv.classList.remove('show');
        setTimeout(() => alertDiv.classList.add('d-none'), 150);
    }
}, 5000);
