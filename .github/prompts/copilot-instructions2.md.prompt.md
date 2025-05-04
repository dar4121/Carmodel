# Car Models Frontend Project Prompt  

## Project Overview  
Create a frontend project for managing car models and their images using the provided API endpoints. The project should be clean, modular, and developer-friendly. Use **AJAX**, **jQuery**, and **DataTables** for dynamic interactions without page reloads. Implement proper form validation using jQuery plugins.  

## Project Structure  
```
/Carmodel  
│  
├── /wwwroot  
│   ├── /css  
│   ├── /js  
│       ├── carModels.js  
│   ├── /lib (for third-party libraries like jQuery, DataTables)  
│  
├── /Views  
│   ├── /Shared  
│       ├── _Layout.cshtml  
│   ├── /CarModels  
│       ├── Index.cshtml  
│       ├── CreateEdit.cshtml  
│  
├── /Controllers  
│   ├── CarModelsController.cs  
│  
```

## Features to Implement  

### 1. Car Models and Images Management  
- **Index Page**:  
  - Display all car models in a DataTable.  
  - Use AJAX to fetch data from `GET /api/CarModels`.  
  - Add buttons for Edit, Delete, Update Sort Order, and Manage Images.  

- **Create/Edit Form**:  
  - Use a modal form for creating or editing car models.  
  - Submit data via AJAX to `POST /api/CarModels` or `PUT /api/CarModels/{id}`.  
  - Validate form fields using jQuery validation.  

- **Delete Car Model**:  
  - Use AJAX to call `DELETE /api/CarModels/{id}`.  
  - Show a confirmation dialog before deletion.  

- **Update Sort Order**:  
  - Allow drag-and-drop sorting in the DataTable.  
  - Submit the updated order via AJAX to `PUT /api/CarModels/UpdateSortOrder`.  

- **Manage Images**:  
  - Include an expandable section or modal within the car models page for managing images.  
  - Display all images for a car model in a DataTable.  
  - Fetch data using `GET /api/CarModelImages/model/{modelId}`.  
  - Add buttons for Upload, Delete, and Set Default Image.  

- **Upload Image**:  
  - Use a modal form for uploading images.  
  - Submit data via AJAX to `POST /api/CarModelImages/Upload`.  
  - Include file upload and validation.  

- **Delete Image**:  
  - Use AJAX to call `DELETE /api/CarModelImages/{imageId}`.  
  - Show a confirmation dialog before deletion.  

- **Set Default Image**:  
  - Use AJAX to call `PUT /api/CarModelImages/SetDefaultImage/{imageId}/model/{modelId}`.  

## Technical Requirements  
1. **AJAX and jQuery**:  
   - Use `$.ajax` for all API calls.  
   - Handle success and error responses appropriately.  

2. **DataTables**:  
   - Use [DataTables](https://datatables.net/) for displaying tabular data.  
   - Enable features like search, pagination, and sorting.  

3. **Form Validation**:  
   - Use jQuery validation plugin for client-side validation.  

4. **_Layout.cshtml**:  
   - Include necessary CSS and JS files for jQuery, DataTables, and validation plugins.  

5. **HttpClient**:  
   - Use `builder.Services.AddHttpClient()` in the backend for API calls.  

6. **Clean Code**:  
   - Write modular and reusable JavaScript functions.  
   - Avoid unnecessary methods or code duplication.  

## Example Code Snippets  

### _Layout.cshtml  
```html  
<head>  
    <link rel="stylesheet" href="/lib/datatables/datatables.min.css" />  
    <script src="/lib/jquery/jquery.min.js"></script>  
    <script src="/lib/datatables/datatables.min.js"></script>  
    <script src="/lib/jquery-validation/jquery.validate.min.js"></script>  
</head>  
```

### carModels.js  
```javascript  
$(document).ready(function () {  
    // Initialize DataTable  
    const table = $('#carModelsTable').DataTable({  
        ajax: {  
            url: '/api/CarModels',  
            dataSrc: ''  
        },  
        columns: [  
            { data: 'id' },  
            { data: 'name' },  
            { data: 'manufacturer' },  
            {  
                data: null,  
                render: function (data) {  
                    return `<button class="btn-edit" data-id="${data.id}">Edit</button>  
                            <button class="btn-delete" data-id="${data.id}">Delete</button>  
                            <button class="btn-manage-images" data-id="${data.id}">Manage Images</button>`;  
                }  
            }  
        ]  
    });  

    // Handle Create/Edit Form Submission  
    $('#carModelForm').on('submit', function (e) {  
        e.preventDefault();  
        const formData = $(this).serialize();  
        const url = $(this).data('id')  
            ? `/api/CarModels/${$(this).data('id')}`  
            : '/api/CarModels';  
        const method = $(this).data('id') ? 'PUT' : 'POST';  

        $.ajax({  
            url: url,  
            method: method,  
            data: formData,  
            success: function () {  
                $('#carModelModal').modal('hide');  
                table.ajax.reload();  
            },  
            error: function (err) {  
                alert('Error: ' + err.responseText);  
            }  
        });  
    });  

    // Handle Manage Images  
    $(document).on('click', '.btn-manage-images', function () {  
        const modelId = $(this).data('id');  
        loadImages(modelId);  
        $('#manageImagesModal').modal('show');  
    });  

    function loadImages(modelId) {  
        $.ajax({  
            url: `/api/CarModelImages/model/${modelId}`,  
            method: 'GET',  
            success: function (data) {  
                // Populate DataTable with images  
            },  
            error: function () {  
                alert('Failed to load images.');  
            }  
        });  
    }  
});  
```
