function deleteVehicle(id) {
    if (confirm('¿Está seguro que desea eliminar este vehículo?')) {
        $.ajax({
            url: '/Vehicles/Delete/' + id,
            type: 'POST',
            success: function(result) {
                if (result.success) {
                    $('#vehiclesTable').DataTable().ajax.reload();
                    toastr.success('Vehículo eliminado correctamente');
                } else {
                    toastr.error('Error al eliminar el vehículo');
                }
            },
            error: function() {
                toastr.error('Error al eliminar el vehículo');
            }
        });
    }
} 