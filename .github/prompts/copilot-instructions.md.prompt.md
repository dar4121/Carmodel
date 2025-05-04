# GitHub Copilot Instructions

## Overview
This project involves creating an API for managing car models. The API should include the following endpoints and adhere to clean code principles. Fluent Validation should be used for validating controls, and unnecessary methods or functions should be removed. The implementation should include repositories, interfaces, and view models, and the API should be defined in the controller. Stored procedures will be used for database operations.

## API Endpoints
1. **GetAllCarModelList**  
   - Use the stored procedure `[dbo].[GetAllCarModelList]` to fetch all car models.

2. **InsertUpdateCarModel**  
   - Use the stored procedure `[dbo].[InsertUpdateCarModel]` to insert or update a car model.

3. **DeleteCarModel**  
   - Create an endpoint to delete a car model.

4. **UpdateSortOrder**  
   - Create an endpoint to update the sort order of car models.

5. **UploadCarModelImage**  
   - Create an endpoint to upload images for a car model.

6. **GetCarModelImages**  
   - Create an endpoint to fetch all images for a specific car model.

7. **DeleteCarModelImage**  
   - Create an endpoint to delete a specific car model image.

8. **SetDefaultImage**  
   - Create an endpoint to set a default image for a car model.

## Implementation Guidelines
1. **Validation**  
   - Use Fluent Validation to validate all input controls.

2. **Clean Code**  
   - Remove any unnecessary methods or functions.
   - Follow clean code principles for readability and maintainability.

3. **Architecture**  
   - Create a **Repository** for database operations.
   - Define an **Interface** for the repository.
   - Use **ViewModels** for data transfer between the API and the client.

4. **Controller**  
   - Implement all API endpoints in the controller.

5. **Database Operations**  
   - Use the stored procedures `[dbo].[GetAllCarModelList]` and `[dbo].[InsertUpdateCarModel]` for their respective operations.

## Example Prompt for GitHub Copilot
- "Create a repository interface for managing car models with methods for fetching, inserting, updating, and deleting car models."
- "Generate a controller with endpoints for managing car models, including validation using Fluent Validation."
- "Write a ViewModel for car models with properties for ID, Name, SortOrder, and Image details."
- "Implement a method in the repository to call the stored procedure `[dbo].[GetAllCarModelList]` and return the result."
- "Create a Fluent Validator for validating car model properties like Name and SortOrder."
- "Write a method in the repository to execute the stored procedure `[dbo].[InsertUpdateCarModel]` with parameters for car model details."

## Notes
- Ensure proper error handling and logging in all methods.
- Follow SOLID principles for better code structure.
- Use dependency injection for managing repository instances in the controller.