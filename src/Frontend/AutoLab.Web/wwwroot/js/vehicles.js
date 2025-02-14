// Función para inicializar la tabla
function initializeVehiclesTable() {
    // Destruir la instancia existente si existe
    if ($.fn.DataTable.isDataTable('#vehiclesTable')) {
        $('#vehiclesTable').DataTable().destroy();
        $('#vehiclesTable thead tr.filters').remove();
    }
    
    // Clonar el header para los filtros
    $('#vehiclesTable thead tr')
        .clone(true)
        .addClass('filters')
        .appendTo('#vehiclesTable thead');

    var table = $('#vehiclesTable').DataTable({
        orderCellsTop: true,
        fixedHeader: true,
        serverSide: true,
        processing: true,
        ajax: {
            url: '/Vehicles/GetVehicles',
            type: 'GET',
            data: function(d) {
                // Agregar los filtros personalizados
                $('.filters input').each(function() {
                    d[$(this).data('column')] = $(this).val();
                });
            }
        },
        columns: [
            { data: 'country', name: 'Pais' },
            { data: 'brand', name: 'Marca' },
            { data: 'model', name: 'Modelo' },
            { data: 'year', name: 'Año' },
            { data: 'licensePlate', name: 'Patente' },
            { data: 'vinCode', name: 'Código VIN' },
            {
                data: null,
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <a href="/Vehicles/Edit/${row.id}" class="btn btn-sm btn-primary">
                            <i class="fas fa-edit"></i> Editar
                        </a>
                        <button class="btn btn-sm btn-danger delete-btn" data-id="${row.id}">
                            <i class="fas fa-trash"></i> Eliminar
                        </button>
                    `;
                }
            }
        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        }
    });

    // Manejar el evento de eliminar
    $('#vehiclesTable').on('click', '.delete-btn', function() {
        var id = $(this).data('id');
        if (confirm('¿Está seguro de que desea eliminar este vehículo?')) {
            $.ajax({
                url: `/api/vehicles/${id}`,
                type: 'DELETE',
                success: function() {
                    table.ajax.reload();
                },
                error: function(xhr, status, error) {
                    alert('Error al eliminar el vehículo: ' + error);
                }
            });
        }
    });

    return table;
}

// Inicializar cuando el documento esté listo
$(document).ready(function() {
    var table = initializeVehiclesTable();

    // Manejar los filtros
    $(document).on('keyup change', '.filters input', function() {
        table.ajax.reload();
    });
}); 