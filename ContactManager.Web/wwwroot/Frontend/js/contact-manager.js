// Contact Manager JavaScript functionality

// File upload handling
function handleFileSelect(input) {
    const fileName = input.files[0]?.name || '';
    const selectedFile = document.getElementById('selectedFile');
    const fileNameSpan = document.getElementById('fileName');
    
    if (fileName) {
        fileNameSpan.textContent = fileName;
        selectedFile.classList.remove('d-none');
    } else {
        selectedFile.classList.add('d-none');
    }
}

// Show formatting guide
function showFormattingGuide() {
    const guide = document.getElementById('formattingGuide');
    guide.classList.toggle('d-none');
}

// Reset filters
function resetFilters() {
    document.getElementById('searchName').value = '';
    document.getElementById('searchPhone').value = '';
    document.getElementById('filterMarried').value = '';
    document.getElementById('minSalary').value = '';
    document.getElementById('maxSalary').value = '';
    
    // Check if DataTable is initialized
    if (!$.fn.DataTable.isDataTable('#contactsTable')) {
        console.error('DataTable is not initialized yet');
        return;
    }
    
    // Clear all DataTable filters
    const table = $('#contactsTable').DataTable();
    table.search('').columns().search('').draw();
    
    // Remove any custom search filters
    $.fn.dataTable.ext.search = [];
    
    updateStatistics(table);
}

// Apply filters
function applyFilters() {
    // Check if DataTable is initialized
    if (!$.fn.DataTable.isDataTable('#contactsTable')) {
        console.error('DataTable is not initialized yet');
        return;
    }
    
    const table = $('#contactsTable').DataTable();
    
    const nameFilter = document.getElementById('searchName').value;
    table.column(0).search(nameFilter);
    
    const phoneFilter = document.getElementById('searchPhone').value;
    table.column(3).search(phoneFilter);
    
    const marriedFilter = document.getElementById('filterMarried').value;
    if (marriedFilter) {
        table.column(2).search(marriedFilter);
    } else {
        table.column(2).search('');
    }
    
    const minSalary = document.getElementById('minSalary').value;
    const maxSalary = document.getElementById('maxSalary').value;
    
    // Remove existing salary filter if exists
    const existingSearchIndex = $.fn.dataTable.ext.search.findIndex(func => 
        func.toString().includes('salaryValue')
    );
    if (existingSearchIndex !== -1) {
        $.fn.dataTable.ext.search.splice(existingSearchIndex, 1);
    }
    
    // Add new salary filter
    if (minSalary || maxSalary) {
        $.fn.dataTable.ext.search.push(function(settings, data, dataIndex) {
            const salaryText = String(data[4] || '').replace(/<[^>]*>/g, '').replace(/[^\d.-]/g, '');
            const salary = parseFloat(salaryText) || 0;
            const min = minSalary ? parseFloat(minSalary) : 0;
            const max = maxSalary ? parseFloat(maxSalary) : Infinity;
            
            return salary >= min && salary <= max;
        });
    }
    
    table.draw();
    updateStatistics(table);
}

// Export to CSV
function exportToCSV() {
    // Check if DataTable is initialized
    if (!$.fn.DataTable.isDataTable('#contactsTable')) {
        console.error('DataTable is not initialized yet');
        return;
    }
    
    const table = $('#contactsTable').DataTable();
    const data = table.rows({ filter: 'applied' }).data();
    
    let csvContent = "Name,DateOfBirth,Married,Phone,Salary\n";
    
    table.rows({ filter: 'applied' }).every(function() {
        const row = this.data();
        csvContent += `${row[0]},${row[1]},${row[2]},${row[3]},${row[4]}\n`;
    });
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', 'contacts_export.csv');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Refresh table
function refreshTable() {
    location.reload();
}

// Edit contact
function editContact(id) {
    const row = document.getElementById(`contact-${id}`);
    const cells = row.getElementsByTagName('td');
    
    for (let i = 0; i < cells.length - 1; i++) {
        const cell = cells[i];
        const currentValue = cell.textContent.trim();
        
        if (i === 2) {
            const married = currentValue === 'Yes';
            cell.innerHTML = `<select class="form-select form-select-sm">
                <option value="true" ${married ? 'selected' : ''}>Yes</option>
                <option value="false" ${!married ? 'selected' : ''}>No</option>
            </select>`;
        } else if (i === 4) {
            const salary = currentValue.replace(/[^\d.-]/g, '');
            cell.innerHTML = `<input type="number" class="form-control form-control-sm" value="${salary}" step="0.01">`;
        } else {
            cell.innerHTML = `<input type="text" class="form-control form-control-sm" value="${currentValue}">`;
        }
    }
    
    // Change action buttons
    const actionCell = cells[cells.length - 1];
    actionCell.innerHTML = `
        <button class="btn btn-sm btn-success" onclick="saveContact(${id})" title="Save">
            <i class="fas fa-save"></i>
        </button>
        <button class="btn btn-sm btn-secondary" onclick="cancelEdit(${id})" title="Cancel">
            <i class="fas fa-times"></i>
        </button>
    `;
}

// Save contact
async function saveContact(id) {
    const row = document.getElementById(`contact-${id}`);
    const cells = row.getElementsByTagName('td');
    
    const contact = {
        id: id,
        name: cells[0].querySelector('input').value,
        dateOfBirth: cells[1].querySelector('input').value,
        married: cells[2].querySelector('select').value === 'true',
        phone: cells[3].querySelector('input').value,
        salary: parseFloat(cells[4].querySelector('input').value)
    };
    
    try {
        const response = await fetch('/Contacts/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(contact)
        });
        
        if (response.ok) {
            showAlert('Contact successfully updated!', 'success');
            refreshTable();
        } else {
            showAlert('Error updating contact', 'danger');
        }
    } catch (error) {
        showAlert('Error updating contact: ' + error.message, 'danger');
    }
}

// Cancel edit
function cancelEdit(id) {
    refreshTable();
}

// Delete contact
async function deleteContact(id, name) {
    if (confirm(`Are you sure you want to delete contact "${name}"?`)) {
        try {
            const response = await fetch('/Contacts/Delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `id=${id}`
            });
            
            if (response.ok) {
                showAlert('Contact successfully deleted!', 'success');
                refreshTable();
            } else {
                showAlert('Error deleting contact', 'danger');
            }
        } catch (error) {
            showAlert('Error deleting contact: ' + error.message, 'danger');
        }
    }
}

// View contact details
function viewContactDetails(id) {
    const row = document.getElementById(`contact-${id}`);
    const cells = row.getElementsByTagName('td');
    
    const name = cells[0].textContent;
    const dob = cells[1].textContent;
    const married = cells[2].textContent;
    const phone = cells[3].textContent;
    const salary = cells[4].textContent;
    
    const content = `
        <div class="row">
            <div class="col-md-6">
                <h6><i class="fas fa-user me-2"></i>Name:</h6>
                <p class="mb-3">${name}</p>
                
                <h6><i class="fas fa-calendar me-2"></i>Date of Birth:</h6>
                <p class="mb-3">${dob}</p>
                
                <h6><i class="fas fa-ring me-2"></i>Marital Status:</h6>
                <p class="mb-3">${married}</p>
            </div>
            <div class="col-md-6">
                <h6><i class="fas fa-phone me-2"></i>Phone:</h6>
                <p class="mb-3">${phone}</p>
                
                <h6><i class="fas fa-money-bill-wave me-2"></i>Salary:</h6>
                <p class="mb-3">${salary}</p>
                
                <h6><i class="fas fa-calculator me-2"></i>Age:</h6>
                <p class="mb-3">${calculateAge(dob)} years</p>
            </div>
        </div>
    `;
    
    document.getElementById('contactDetailsContent').innerHTML = content;
    new bootstrap.Modal(document.getElementById('contactDetailsModal')).show();
}

// Calculate age
function calculateAge(dob) {
    const birthDate = new Date(dob);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
    }
    
    return age;
}

// Show alert
function showAlert(message, type) {
    const alertContainer = document.getElementById('alertContainer');
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    alertContainer.appendChild(alert);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 5000);
}

// Update statistics
function updateStatistics(tableInstance) {
    let marriedCount = 0;
    let totalSalary = 0;
    let totalAge = 0;
    let totalCount = 0;
    let visibleCount = 0;

    if (tableInstance) {
        const totalRows = tableInstance.rows().data().toArray();
        const visibleRows = tableInstance.rows({ filter: 'applied' }).data().toArray();
        totalCount = totalRows.length;
        visibleCount = visibleRows.length;

        visibleRows.forEach(row => {
            const marriedText = String(row[2] || '').replace(/<[^>]*>/g, '').trim();
            const salaryText = String(row[4] || '').replace(/<[^>]*>/g, '').replace(/[^\d.-]/g, '');
            const dobText = String(row[1] || '').replace(/<[^>]*>/g, '').trim();
            const married = marriedText === 'Yes';
            const salary = parseFloat(salaryText) || 0;

            if (married) marriedCount++;
            totalSalary += salary;
            totalAge += calculateAge(dobText);
        });
    } else {
        // Fallback to DOM elements if DataTable is not available
        const rows = document.querySelectorAll('.contact-row');
        totalCount = rows.length;
        visibleCount = rows.length;
        rows.forEach(row => {
            const married = row.querySelector('.contact-married span').textContent.trim() === 'Yes';
            const salary = parseFloat(row.querySelector('.contact-salary').textContent.replace(/[^\d.-]/g, '')) || 0;
            const dob = row.querySelector('.contact-dob').textContent;

            if (married) marriedCount++;
            totalSalary += salary;
            totalAge += calculateAge(dob);
        });
    }

    document.getElementById('totalContacts').textContent = totalCount;
    document.getElementById('marriedCount').textContent = marriedCount;
    document.getElementById('avgSalary').textContent = visibleCount > 0 ? (totalSalary / visibleCount).toFixed(0) + ' $' : '0 $';
    document.getElementById('avgAge').textContent = visibleCount > 0 ? Math.round(totalAge / visibleCount) : '0';
    document.getElementById('visibleCount').textContent = visibleCount;
    document.getElementById('totalCount').textContent = totalCount;
}

// Initialize DataTable
document.addEventListener('DOMContentLoaded', function() {
    const table = $('#contactsTable').DataTable({
        "language": {
            "search": "Search:",
            "lengthMenu": "Show _MENU_ entries",
            "info": "Showing _START_ to _END_ of _TOTAL_ entries",
            "infoEmpty": "Showing 0 to 0 of 0 entries",
            "infoFiltered": "(filtered from _MAX_ total entries)",
            "zeroRecords": "No matching records found",
            "emptyTable": "No data available in table",
            "paginate": {
                "first": "First",
                "last": "Last",
                "next": "Next",
                "previous": "Previous"
            }
        },
        "order": [[0, "asc"]],
        "pageLength": 25,
        "responsive": true
    });
    
    updateStatistics(table);
    
    // Add event listeners for real-time filtering
    document.getElementById('searchName').addEventListener('input', function() {
        applyFilters();
    });
    
    document.getElementById('searchPhone').addEventListener('input', function() {
        applyFilters();
    });
    
    document.getElementById('filterMarried').addEventListener('change', function() {
        applyFilters();
    });
    
    document.getElementById('minSalary').addEventListener('input', function() {
        applyFilters();
    });
    
    document.getElementById('maxSalary').addEventListener('input', function() {
        applyFilters();
    });
    
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Form submission handling
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('csvUploadForm');
    if (form) {
        form.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const fileInput = document.getElementById('csvFile');
            const file = fileInput.files[0];
            
            if (!file) {
                showAlert('Please select a CSV file', 'warning');
                return;
            }
            
            const formData = new FormData();
            formData.append('file', file);
            
            // Get Anti-Forgery Token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
            
            try {
                console.log('Uploading file:', file.name, 'Size:', file.size);
                const response = await fetch('/Contacts/Upload', {
                    method: 'POST',
                    body: formData
                });
                
                console.log('Response status:', response.status);
                
                if (response.ok) {
                    // Redirect to refresh the page
                    window.location.href = '/Contacts';
                } else {
                    const errorText = await response.text();
                    console.error('Upload error:', errorText);
                    showAlert('Error uploading file: ' + errorText, 'danger');
                }
            } catch (error) {
                console.error('Upload exception:', error);
                showAlert('Error uploading file: ' + error.message, 'danger');
            }
        });
    }
});
