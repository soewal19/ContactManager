// Frontend JavaScript logic for Contact Manager

// DataTables initialization
function initializeDataTable() {
    if ($.fn.DataTable.isDataTable('#contactsTable')) {
        $('#contactsTable').DataTable().destroy();
    }
    
    $('#contactsTable').DataTable({
        responsive: true,
        pageLength: 25,
        order: [[0, 'asc']],
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "Showing 0 to 0 of 0 entries",
            infoFiltered: "(filtered from _MAX_ total entries)",
            zeroRecords: "No matching records found",
            emptyTable: "No data available in table",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        }
    });
}

// CSV file upload handling
function handleCsvUpload() {
    $('#csvFile').on('change', function() {
        const file = this.files[0];
        
        if (file) {
            if (file.name.toLowerCase().endsWith('.csv')) {
                $('#uploadStatus').html('<div class="alert alert-info">File selected: ' + file.name + '</div>');
            } else {
                $('#uploadStatus').html('<div class="alert alert-danger">Please select a CSV file</div>');
                this.value = '';
            }
        }
    });
}

// CSV upload form submission
function submitCsvForm() {
    $('#csvUploadForm').on('submit', function(e) {
        e.preventDefault();
        
        const fileInput = $('#csvFile')[0];
        const file = fileInput.files[0];
        
        if (!file) {
            $('#uploadStatus').html('<div class="alert alert-warning">Please select a CSV file</div>');
            return;
        }

        $('#uploadStatus').html('<div class="alert alert-info">Uploading...</div>');
        $('#uploadButton').prop('disabled', true);

        const formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: '/Contacts/UploadCsv',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    $('#uploadStatus').html('<div class="alert alert-success">File successfully uploaded! Contacts added: ' + response.contactsAdded + '</div>');
                    setTimeout(function() {
                        location.reload();
                    }, 2000);
                } else {
                    $('#uploadStatus').html('<div class="alert alert-danger">Error: ' + response.message + '</div>');
                }
            },
            error: function(xhr, status, error) {
                $('#uploadStatus').html('<div class="alert alert-danger">Upload error: ' + error + '</div>');
            },
            complete: function() {
                $('#uploadButton').prop('disabled', false);
            }
        });
    });
}

// Contact editing function
function editContact(id) {
    const row = $('#contact-' + id);
    const cells = row.find('td');
    
    // Save current values
    const currentValues = {
        name: cells.eq(0).text(),
        dateOfBirth: cells.eq(1).text(),
        married: cells.eq(2).text() === 'Yes',
        phone: cells.eq(3).text(),
        salary: cells.eq(4).text()
    };

    // Create editing fields
    cells.eq(0).html('<input type="text" class="form-control form-control-sm" value="' + currentValues.name + '" id="edit-name-' + id + '">');
    cells.eq(1).html('<input type="date" class="form-control form-control-sm" value="' + currentValues.dateOfBirth + '" id="edit-dob-' + id + '">');
    cells.eq(2).html('<select class="form-control form-control-sm" id="edit-married-' + id + '">' +
        '<option value="true"' + (currentValues.married ? ' selected' : '') + '>Yes</option>' +
        '<option value="false"' + (!currentValues.married ? ' selected' : '') + '>No</option>' +
        '</select>');
    cells.eq(3).html('<input type="tel" class="form-control form-control-sm" value="' + currentValues.phone + '" id="edit-phone-' + id + '">');
    cells.eq(4).html('<input type="number" step="0.01" class="form-control form-control-sm" value="' + currentValues.salary.replace(',', '.') + '" id="edit-salary-' + id + '">');

    // Change action buttons
    const actionsCell = cells.eq(5);
    actionsCell.html(
        '<button class="btn btn-success btn-sm mr-1" onclick="saveContact(' + id + ')" title="Save">' +
        '<i class="fas fa-save"></i></button>' +
        '<button class="btn btn-secondary btn-sm" onclick="cancelEdit(' + id + ')" title="Cancel">' +
        '<i class="fas fa-times"></i></button>'
    );
}

// Save edited contact
function saveContact(id) {
    const data = {
        id: id,
        name: $('#edit-name-' + id).val(),
        dateOfBirth: $('#edit-dob-' + id).val(),
        married: $('#edit-married-' + id).val() === 'true',
        phone: $('#edit-phone-' + id).val(),
        salary: parseFloat($('#edit-salary-' + id).val())
    };

    $.ajax({
        url: '/Contacts/Update',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                showAlert('Contact successfully updated!', 'success');
                setTimeout(function() {
                    location.reload();
                }, 1500);
            } else {
                showAlert('Error: ' + response.message, 'danger');
            }
        },
        error: function(xhr, status, error) {
            showAlert('Save error: ' + error, 'danger');
        }
    });
}

// Cancel editing
function cancelEdit(id) {
    location.reload();
}

// Delete contact
function deleteContact(id, name) {
    if (confirm('Are you sure you want to delete contact "' + name + '"?')) {
        $.ajax({
            url: '/Contacts/Delete',
            type: 'POST',
            data: { id: id },
            success: function(response) {
                if (response.success) {
                    showAlert('Contact successfully deleted!', 'success');
                    setTimeout(function() {
                        location.reload();
                    }, 1500);
                } else {
                    showAlert('Error: ' + response.message, 'danger');
                }
            },
            error: function(xhr, status, error) {
                showAlert('Delete error: ' + error, 'danger');
            }
        });
    }
}

// Show alert message
function showAlert(message, type) {
    const alertHtml = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
        message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
        '</div>';
    
    $('#alertContainer').html(alertHtml);
    
    // Auto-hide after 5 seconds
    setTimeout(function() {
        $('.alert').alert('close');
    }, 5000);
}

// Initialize all functions when document is ready
$(document).ready(function() {
    initializeDataTable();
    handleCsvUpload();
    submitCsvForm();
    
    // Setup table filters
    $('#searchName').on('keyup', function() {
        const table = $('#contactsTable').DataTable();
        table.column(0).search(this.value).draw();
    });

    $('#searchPhone').on('keyup', function() {
        const table = $('#contactsTable').DataTable();
        table.column(3).search(this.value).draw();
    });

    $('#filterMarried').on('change', function() {
        const table = $('#contactsTable').DataTable();
        const value = this.value;
        if (value === '') {
            table.column(2).search('').draw();
        } else {
            table.column(2).search(value).draw();
        }
    });

    $('#minSalary, #maxSalary').on('keyup change', function() {
        const table = $('#contactsTable').DataTable();
        const minSalary = parseFloat($('#minSalary').val()) || 0;
        const maxSalary = parseFloat($('#maxSalary').val()) || Infinity;
        
        $.fn.dataTable.ext.search.push(function(settings, data, dataIndex) {
            const salary = parseFloat(data[4].replace(/[^0-9.-]+/g, "")) || 0;
            return salary >= minSalary && salary <= maxSalary;
        });
        
        table.draw();
        $.fn.dataTable.ext.search.pop();
    });
});