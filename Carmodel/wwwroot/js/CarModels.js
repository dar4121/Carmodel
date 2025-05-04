if (typeof window.apiBase === 'undefined') {
    window.apiBase = 'https://localhost:7003/api/CarModels';
    window.imgApi = 'https://localhost:7003/api/CarModelImages';
    window.imgBaseUrl = 'https://localhost:7003';
    window.brandApi = 'https://localhost:7003/api/CarModels/Brands';
    window.classApi = 'https://localhost:7003/api/CarModels/Classes';
    window.defaultCarImage = '/images/cars/default-car.jpg';
}


toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": true,
    "showDuration": "200",
    "hideDuration": "800",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

$(document).ready(function () {
    loadDropdownData();
    initCarModelsTable();
    initValidation();
    initDragAndDrop();


    $('#addNewModelBtn').on('click', function () {
        openCarModelModal();
    });


    $('.modal .close, .modal .btn-secondary').on('click', function () {
        $(this).closest('.modal').modal('hide');
    });
});

function loadDropdownData() {

    $.get(window.brandApi)
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


    $.get(window.classApi)
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

    if ($.fn.DataTable.isDataTable('#carModelsTable')) {
        $('#carModelsTable').DataTable().destroy();
    }

    $('#carModelsTable').DataTable({
        ajax: {
            url: window.apiBase,
            dataSrc: ''
        },
        rowReorder: {
            dataSrc: 'sortOrder',
            selector: 'td:not(:last-child)',
            handle: true
        },
        order: [[4, 'asc']], 
        columns: [
            {
                data: 'modelName',
                className: 'reorder-handle'
            },
            {
                data: 'modelCode',
                className: 'reorder-handle'
            },
            {
                data: 'brandName',
                className: 'reorder-handle'
            },
            {
                data: 'className',
                className: 'reorder-handle'
            },
            {
                data: 'sortOrder',
                className: 'reorder-handle'
            },
            {
                data: null,
                className: 'action-column',
                orderable: false,
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

    table.on('row-reorder', function (e, diff) {
        if (!diff || diff.length === 0) return;

       
        const allRows = table.rows().data().toArray();
        const updates = [];

      
        const positionToModelId = {};
        allRows.forEach((row, index) => {
            positionToModelId[index] = row.modelId;
        });

        
        const newOrder = [...allRows].map(row => row.modelId);

      
        diff.forEach(item => {
            const modelId = positionToModelId[item.oldPosition];
           
            newOrder.splice(item.oldPosition, 1);
            newOrder.splice(item.newPosition, 0, modelId);
        });

        
        for (let i = 0; i < newOrder.length; i++) {
            updates.push({
                modelId: newOrder[i],
                sortOrder: i + 1 
            });
        }

        if (updates.length > 0) {
         
            const loadingOverlay = $('<div class="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="sr-only">Updating order...</span></div></div>');
            $('body').append(loadingOverlay);

            $.ajax({
                url: `${window.apiBase}/UpdateSortOrder`,
                type: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(updates)
            })
                .done(function () {
                    toastr.success('Sort order updated successfully');
                  
                    table.ajax.reload(null, false); 
                })
                .fail(function (error) {
                    toastr.error('Error updating sort order: ' + error.responseText);
                    table.ajax.reload();
                })
                .always(function () {
                   
                    loadingOverlay.remove();
                });
        }
    });
}

function openCarModelModal(id = null) {
    resetForm('#carModelForm');


    loadDropdownData();

    if (id) {
        $.get(`${window.apiBase}/${id}`)
            .done(function (data) {

                $('#Id').val(data.modelId);
                $('#ModelName').val(data.modelName);
                $('#ModelCode').val(data.modelCode);
                $('#Description').val(data.description);
                $('#Features').val(data.features);
                $('#Price').val(data.price);


                if (data.dateofManufacturing) {
                    const date = new Date(data.dateofManufacturing);
                    if (!isNaN(date.getTime())) {
                        const formattedDate = date.toISOString().split('T')[0];
                        $('#DateofManufacturing').val(formattedDate);
                    }
                }


                setTimeout(function () {
                    $('#BrandId').val(data.brandId);
                    $('#ClassId').val(data.classId);
                }, 200);

                $('#carModelModal').modal('show');
                toastr.info('Editing car model: ' + data.modelName);
            })
            .fail(function (error) {
                toastr.error('Error fetching car model: ' + JSON.stringify(error));
            });
    } else {
        $('#carModelModal').modal('show');
        toastr.info('Creating new car model');
    }
}

function saveCarModel() {
    const form = $('#carModelForm');


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

    const url = id ? `${window.apiBase}/${id}` : window.apiBase;
    const method = id ? 'PUT' : 'POST';
    const actionText = id ? 'updated' : 'created';

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(data)
    })
        .done(function () {
            $('#carModelModal').modal('hide');
            toastr.success(`Car model ${actionText} successfully`);
            $('#carModelsTable').DataTable().ajax.reload();
        })
        .fail(function (error) {
            toastr.error('Error saving car model: ' + (error.responseText || JSON.stringify(error)));
        });
}

function deleteCarModel(id) {
    if (confirm('Are you sure you want to delete this car model?')) {
        $.ajax({
            url: `${window.apiBase}/${id}`,
            type: 'DELETE'
        })
            .done(function () {
                toastr.success('Car model deleted successfully');
                $('#carModelsTable').DataTable().ajax.reload();
            })
            .fail(function (error) {
                toastr.error('Error deleting car model: ' + error.responseText);
            });
    }
}

function openImageModal(modelId) {
    resetForm('#imageForm');
    $('#ModelId').val(modelId);


    $('#imageModal').modal('show');


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
            url: `${window.imgApi}/model/${modelId}`,
            dataSrc: function (data) {

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
                    if (!data) {

                        return `<img src="${window.imgBaseUrl}${window.defaultCarImage}" alt="Default Car Image" style="max-height: 100px;" />`;
                    }

                    const fullImageUrl = data.startsWith('http') ? data : `${window.imgBaseUrl}${data}`;
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


    $('#imagesTable').off('click', '.delete-image-btn');
    $('#imagesTable').off('click', '.set-default-btn');


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


    formData.append('ModelId', $('#ModelId').val());


    formData.append('IsDefault', $('#IsDefault').prop('checked'));


    const fileInput = document.getElementById('Images');
    if (fileInput.files.length > 0) {
        for (let i = 0; i < fileInput.files.length; i++) {
            formData.append('Images', fileInput.files[i]);
        }
    } else {
        toastr.warning('Please select at least one image');
        return;
    }

    $.ajax({
        url: `${window.imgApi}/Upload`,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false
    })
        .done(function () {
            resetForm('#imageForm');
            toastr.success('Images uploaded successfully');
            const modelId = $('#ModelId').val();
            initImagesTable(modelId);
        })
        .fail(function (error) {
            toastr.error('Error uploading images: ' + (error.responseText || JSON.stringify(error)));
        });
}

function deleteImage(id) {
    if (confirm('Are you sure you want to delete this image?')) {
        $.ajax({
            url: `${window.imgApi}/${id}`,
            type: 'DELETE'
        })
            .done(function () {
                toastr.success('Image deleted successfully');
                const modelId = $('#ModelId').val();
                initImagesTable(modelId);
            })
            .fail(function (error) {
                toastr.error('Error deleting image: ' + error.responseText);
            });
    }
}

function setDefaultImage(imageId, modelId) {
    $.ajax({
        url: `${window.imgApi}/SetDefaultImage/${imageId}/model/${modelId}`,
        type: 'PUT'
    })
        .done(function () {
            toastr.success('Default image set successfully');
            initImagesTable(modelId);
        })
        .fail(function (error) {
            toastr.error('Error setting default image: ' + error.responseText);
        });
}

function viewCarModelDetails(id) {
    $.get(`${window.apiBase}/${id}`)
        .done(function (data) {

            $('#viewModelId').val(data.modelId);


            $('#viewModelName').text(data.modelName);
            $('#viewModelCode').text(data.modelCode);
            $('#viewBrandName').text(data.brandName);
            $('#viewClassName').text(data.className);
            $('#viewPrice').text(formatCurrency(data.price));


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


            $('#viewDescription').html(data.description ? data.description : '<p class="text-muted">No description available</p>');
            $('#viewFeatures').html(data.features ? data.features : '<p class="text-muted">No features available</p>');


            $.get(`${window.imgApi}/model/${id}/default`)
                .done(function (imageData) {
                    if (imageData && imageData.imageUrl) {
                        const fullImageUrl = imageData.imageUrl.startsWith('http') ?
                            imageData.imageUrl : `${window.imgBaseUrl}${imageData.imageUrl}`;
                        $('#viewDefaultImage').html(`
                            <img src="${fullImageUrl}" alt="Default Car Image" 
                                 class="img-fluid" style="max-height: 250px;" />
                        `);
                    } else {

                        $('#viewDefaultImage').html(`
                            <img src="${window.imgBaseUrl}${window.defaultCarImage}" alt="Default Car Image" 
                                 class="img-fluid" style="max-height: 250px;" />
                        `);
                    }
                })
                .fail(function () {

                    $('#viewDefaultImage').html(`
                        <img src="${window.imgBaseUrl}${window.defaultCarImage}" alt="Default Car Image" 
                             class="img-fluid" style="max-height: 250px;" />
                    `);
                });


            $('#viewDetailsModal').modal('show');
        })
        .fail(function (error) {
            toastr.error('Error fetching car model details: ' + (error.responseText || JSON.stringify(error)));
        });
}


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
                required: true,
                max: function () {
                    return new Date().toISOString().split('T')[0];
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
                required: "Date of Manufacturing is required",
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


    $.validator.addMethod("extension", function (value, element, param) {
        param = typeof param === "string" ? param.replace(/,/g, "|") : "png|jpe?g|gif";
        if (element.files.length === 0) {
            return true;
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


    $(selector).find('.is-invalid').removeClass('is-invalid');
    $(selector).find('.is-valid').removeClass('is-valid');
}