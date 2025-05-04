const apiBase = 'https://localhost:7003/api/CarModels';
const imgApi = 'https://localhost:7003/api/CarModelImages';
const imgBaseUrl = 'https://localhost:7003'; // Add this line for image base URL
const brandApi = 'https://localhost:7003/api/CarModels/Brands';
const classApi = 'https://localhost:7003/api/CarModels/Classes';

$(document).ready(function () {
    loadDropdownData();
    initCarModelsTable();
    initValidation();
    initDragAndDrop();

    // Add buttons for opening modals
    $('#addNewModelBtn').on('click', function () {
        openCarModelModal();
    });

    // Fix modal close buttons
    $('.modal .close, .modal .btn-secondary').on('click', function () {
        $(this).closest('.modal').modal('hide');
    });
});

function loadDropdownData() {
    // Load brands
    $.get(brandApi)
        .done(function (brands) {
            $('#BrandId').empty();
            $('#BrandId').append('<option value="">Select Brand</option>');
            brands.forEach(function (brand) {
                $('#BrandId').append(`<option value="${brand.brandId}">${brand.brandName}</option>`);
            });
        })
        .fail(function (error) {
            console.error('Error loading brands:', error);
        });

    // Load classes
    $.get(classApi)
        .done(function (classes) {
            $('#ClassId').empty();
            $('#ClassId').append('<option value="">Select Class</option>');
            classes.forEach(function (cls) {
                $('#ClassId').append(`<option value="${cls.classId}">${cls.className}</option>`);
            });
        })
        .fail(function (error) {
            console.error('Error loading classes:', error);
        });
}

function initCarModelsTable() {
    $('#carModelsTable').DataTable({
        ajax: {
            url: apiBase,
            dataSrc: ''
        },
        rowReorder: {
            dataSrc: 'sortOrder'
        },
        columns: [
            { data: 'modelName' },
            { data: 'modelCode' },
            { data: 'brandName' },
            { data: 'className' },
            { data: 'sortOrder' },
            {
                data: null,
                render: function (data) {
                    return `
                        <div data-model-id="${data.modelId}">
                            <button class="btn btn-sm btn-info view-btn">View</button>
                            <button class="btn btn-sm btn-primary edit-btn">Edit</button>
                            <button class="btn btn-sm btn-danger delete-btn">Delete</button>
                            <button class="btn btn-sm btn-info images-btn">Images</button>
                        </div>
                    `;
                }
            }
        ],
        createdRow: function (row, data) {
            $(row).attr('data-model-id', data.modelId);
        }
    });

    // Setup button event handlers using delegated events
    $('#carModelsTable').on('click', '.view-btn', function () {
        const modelId = $(this).parent().data('model-id');
        viewCarModelDetails(modelId);
    });

    $('#carModelsTable').on('click', '.edit-btn', function () {
        const modelId = $(this).parent().data('model-id');
        openCarModelModal(modelId);
    });

    $('#carModelsTable').on('click', '.delete-btn', function () {
        const modelId = $(this).parent().data('model-id');
        deleteCarModel(modelId);
    });

    $('#carModelsTable').on('click', '.images-btn', function () {
        const modelId = $(this).parent().data('model-id');
        openImageModal(modelId);
    });
}

function initDragAndDrop() {
    const table = $('#carModelsTable').DataTable();

    table.on('row-reorder', function (e, diff, edit) {
        const updates = [];
        for (let i = 0; i < diff.length; i++) {
            // Get the model ID from the data attribute
            const modelId = $(diff[i].node).data('model-id');
            updates.push({
                modelId: modelId,
                sortOrder: diff[i].newPosition
            });
        }

        if (updates.length > 0) {
            $.ajax({
                url: `${apiBase}/UpdateSortOrder`,
                type: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(updates)
            })
                .fail(function (error) {
                    alert('Error updating sort order: ' + error.responseText);
                    table.ajax.reload();
                });
        }
    });
}

function openCarModelModal(id = null) {
    resetForm('#carModelForm');

    // Always ensure dropdown data is loaded
    loadDropdownData();

    if (id) {
        $.get(`${apiBase}/${id}`)
            .done(function (data) {
                // Set values immediately and then update dropdowns after they're loaded
                $('#Id').val(data.modelId);
                $('#ModelName').val(data.modelName);
                $('#ModelCode').val(data.modelCode);
                $('#Description').val(data.description);
                $('#Features').val(data.features);
                $('#Price').val(data.price);

                // Fix date formatting before setting it in the date picker
                if (data.dateofManufacturing) {
                    // Convert the date string to a Date object and format it as YYYY-MM-DD for the date picker
                    const date = new Date(data.dateofManufacturing);
                    if (!isNaN(date.getTime())) {
                        const formattedDate = date.toISOString().split('T')[0];
                        $('#DateofManufacturing').val(formattedDate);
                    }
                }

                // Set dropdown values after a short delay to ensure they're loaded
                setTimeout(function () {
                    $('#BrandId').val(data.brandId);
                    $('#ClassId').val(data.classId);
                }, 200);

                $('#carModelModal').modal('show');
            })
            .fail(function (error) {
                alert('Error fetching car model: ' + JSON.stringify(error));
            });
    } else {
        $('#carModelModal').modal('show');
    }
}

function saveCarModel() {
    const form = $('#carModelForm');

    // Check form validation
    if (!$(form).valid()) {
        return false;
    }

    const id = $('#Id').val();
    const data = {
        modelId: id || null,
        modelName: $('#ModelName').val(),
        modelCode: $('#ModelCode').val(),
        brandId: parseInt($('#BrandId').val()),
        classId: parseInt($('#ClassId').val()),
        description: $('#Description').val(),
        features: $('#Features').val(),
        price: parseFloat($('#Price').val()),
        dateofManufacturing: $('#DateofManufacturing').val()
    };

    const url = id ? `${apiBase}/${id}` : apiBase;
    const method = id ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(data)
    })
        .done(function () {
            $('#carModelModal').modal('hide');
            $('#carModelsTable').DataTable().ajax.reload();
        })
        .fail(function (error) {
            alert('Error saving car model: ' + (error.responseText || JSON.stringify(error)));
        });
}

function deleteCarModel(id) {
    if (confirm('Are you sure you want to delete this car model?')) {
        $.ajax({
            url: `${apiBase}/${id}`,
            type: 'DELETE'
        })
            .done(function () {
                $('#carModelsTable').DataTable().ajax.reload();
            })
            .fail(function (error) {
                alert('Error deleting car model: ' + error.responseText);
            });
    }
}

function openImageModal(modelId) {
    resetForm('#imageForm');
    $('#ModelId').val(modelId);

    // Make sure the modal is shown before initializing the table
    $('#imageModal').modal('show');

    // Initialize the table after the modal is shown
    setTimeout(function () {
        initImagesTable(modelId);
    }, 200);
}

function initImagesTable(modelId) {
    if ($.fn.DataTable.isDataTable('#imagesTable')) {
        $('#imagesTable').DataTable().destroy();
    }

    $('#imagesTable').DataTable({
        ajax: {
            url: `${imgApi}/model/${modelId}`,
            dataSrc: function (data) {
                // Return empty array if data is null
                return data || [];
            },
            error: function (xhr, error, thrown) {
                console.error('Error loading images:', error);
                return [];
            }
        },
        columns: [
            {
                data: 'imageUrl',
                render: function (data) {
                    if (!data) return 'No image';
                    // Prepend the API base URL to the image path
                    const fullImageUrl = data.startsWith('http') ? data : `${imgBaseUrl}${data}`;
                    return `<img src="${fullImageUrl}" alt="Car Image" style="max-height: 100px;" />`;
                }
            },
            {
                data: 'isDefault',
                render: function (data) {
                    return data ? 'Yes' : 'No';
                }
            },
            {
                data: null,
                render: function (data) {
                    const defaultButton = !data.isDefault ?
                        `<button class="btn btn-sm btn-success set-default-btn">Set Default</button>` : '';
                    return `
                        <div data-image-id="${data.imageId}" data-model-id="${data.modelId}">
                            ${defaultButton}
                            <button class="btn btn-sm btn-danger delete-image-btn">Delete</button>
                        </div>
                    `;
                }
            }
        ]
    });

    // Remove previous event handlers to prevent duplicates
    $('#imagesTable').off('click', '.delete-image-btn');
    $('#imagesTable').off('click', '.set-default-btn');

    // Add event listeners for image actions
    $('#imagesTable').on('click', '.delete-image-btn', function () {
        const imageId = $(this).parent().data('image-id');
        deleteImage(imageId);
    });

    $('#imagesTable').on('click', '.set-default-btn', function () {
        const container = $(this).parent();
        const imageId = container.data('image-id');
        const modelId = container.data('model-id');
        setDefaultImage(imageId, modelId);
    });
}

function uploadImage() {
    const formData = new FormData();

    // Add model ID
    formData.append('ModelId', $('#ModelId').val());

    // Add IsDefault value
    formData.append('IsDefault', $('#IsDefault').prop('checked'));

    // Add all selected files
    const fileInput = document.getElementById('Images');
    if (fileInput.files.length > 0) {
        for (let i = 0; i < fileInput.files.length; i++) {
            formData.append('Images', fileInput.files[i]);
        }
    } else {
        alert('Please select at least one image');
        return;
    }

    $.ajax({
        url: `${imgApi}/Upload`,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false
    })
        .done(function () {
            resetForm('#imageForm');
            const modelId = $('#ModelId').val();
            initImagesTable(modelId);
        })
        .fail(function (error) {
            alert('Error uploading images: ' + (error.responseText || JSON.stringify(error)));
        });
}

function deleteImage(id) {
    if (confirm('Are you sure you want to delete this image?')) {
        $.ajax({
            url: `${imgApi}/${id}`,
            type: 'DELETE'
        })
            .done(function () {
                const modelId = $('#ModelId').val();
                initImagesTable(modelId);
            })
            .fail(function (error) {
                alert('Error deleting image: ' + error.responseText);
            });
    }
}

function setDefaultImage(imageId, modelId) {
    $.ajax({
        url: `${imgApi}/SetDefaultImage/${imageId}/model/${modelId}`,
        type: 'PUT'
    })
        .done(function () {
            initImagesTable(modelId);
        })
        .fail(function (error) {
            alert('Error setting default image: ' + error.responseText);
        });
}

function viewCarModelDetails(id) {
    $.get(`${apiBase}/${id}`)
        .done(function (data) {
            // Store model ID in a hidden input field instead of displaying it
            $('#viewModelId').val(data.modelId);

            // Set basic car model details
            $('#viewModelName').text(data.modelName);
            $('#viewModelCode').text(data.modelCode);
            $('#viewBrandName').text(data.brandName);
            $('#viewClassName').text(data.className);
            $('#viewPrice').text(formatCurrency(data.price));

            // Format and set the manufacturing date
            if (data.dateofManufacturing) {
                const date = new Date(data.dateofManufacturing);
                if (!isNaN(date.getTime())) {
                    $('#viewDateofManufacturing').text(date.toLocaleDateString());
                } else {
                    $('#viewDateofManufacturing').text('N/A');
                }
            } else {
                $('#viewDateofManufacturing').text('N/A');
            }

            // Set description and features with fallbacks for empty values
            $('#viewDescription').html(data.description ? data.description : '<p class="text-muted">No description available</p>');
            $('#viewFeatures').html(data.features ? data.features : '<p class="text-muted">No features available</p>');

            // Fetch the default image for this car model
            $.get(`${imgApi}/model/${id}/default`)
                .done(function (imageData) {
                    if (imageData && imageData.imageUrl) {
                        const fullImageUrl = imageData.imageUrl.startsWith('http') ?
                            imageData.imageUrl : `${imgBaseUrl}${imageData.imageUrl}`;
                        $('#viewDefaultImage').html(`
                            <img src="${fullImageUrl}" alt="Default Car Image" 
                                 class="img-fluid" style="max-height: 250px;" />
                        `);
                    } else {
                        $('#viewDefaultImage').html('<p class="text-muted">No default image available</p>');
                    }
                })
                .fail(function () {
                    $('#viewDefaultImage').html('<p class="text-muted">No default image available</p>');
                });

            // Show the modal
            $('#viewDetailsModal').modal('show');
        })
        .fail(function (error) {
            alert('Error fetching car model details: ' + (error.responseText || JSON.stringify(error)));
        });
}

// Helper function to format currency
function formatCurrency(value) {
    if (value == null) return 'N/A';
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(value);
}

function initValidation() {
    $('#carModelForm').validate({
        rules: {
            ModelName: {
                required: true,
                maxlength: 50
            },
            ModelCode: {
                required: true,
                maxlength: 50
            },
            BrandId: 'required',
            ClassId: 'required',
            Price: {
                required: true,
                number: true,
                min: 0
            },
            DateofManufacturing: {
                date: true,
                max: function () {
                    return new Date().toISOString().split('T')[0]; // Today's date
                }
            }
        },
        messages: {
            ModelName: {
                required: "Model name is required",
                maxlength: "Model name cannot exceed 50 characters"
            },
            ModelCode: {
                required: "Model code is required",
                maxlength: "Model code cannot exceed 50 characters"
            },
            BrandId: "Brand is required",
            ClassId: "Class is required",
            Price: {
                required: "Price is required",
                number: "Price must be a valid number",
                min: "Price cannot be negative"
            },
            DateofManufacturing: {
                max: "Manufacturing date cannot be in the future"
            }
        },
        errorElement: 'span',
        errorClass: 'text-danger',
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        submitHandler: function (form) {
            saveCarModel();
            return false;
        }
    });

    $('#imageForm').validate({
        rules: {
            Images: {
                required: true,
                extension: "jpg|jpeg|png|gif"
            }
        },
        messages: {
            Images: {
                required: "Please select at least one image",
                extension: "Only .jpg, .jpeg, .png, and .gif files are allowed"
            }
        },
        errorElement: 'span',
        errorClass: 'text-danger',
        errorPlacement: function (error, element) {
            if (element.attr("name") == "Images") {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        },
        submitHandler: function (form) {
            uploadImage();
            return false;
        }
    });

    // Add additional validation methods
    $.validator.addMethod("extension", function (value, element, param) {
        param = typeof param === "string" ? param.replace(/,/g, "|") : "png|jpe?g|gif";
        if (element.files.length === 0) {
            return true; // Skip validation if no file selected (required rule will handle this)
        }

        let valid = true;
        for (let i = 0; i < element.files.length; i++) {
            const extension = (element.files[i].name.split('.').pop()).toLowerCase();
            if (!new RegExp("\.(" + param + ")$").test("." + extension)) {
                valid = false;
                break;
            }
        }
        return valid;
    }, "Please upload files with valid extensions.");
}

function resetForm(selector) {
    $(selector)[0].reset();
    const validator = $(selector).validate();
    if (validator) {
        validator.resetForm();
    }

    // Clear validation errors visually
    $(selector).find('.is-invalid').removeClass('is-invalid');
    $(selector).find('.is-valid').removeClass('is-valid');
}