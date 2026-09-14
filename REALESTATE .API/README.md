RealEstate.API

A secure and scalable ASP.NET Core Web API for managing residential apartments in Alexandria, Egypt.

The system allows property owners to publish apartments, buyers to browse and save approved properties, and administrators to review and manage property listings.

---

🚀 Project Overview

RealEstate.API is a backend-focused real estate platform built with ASP.NET Core Web API.

The project was designed to demonstrate real-world backend development concepts including:

- RESTful API development
- JWT Authentication
- Role-Based Authorization
- Entity Framework Core
- MySQL Database
- Service Layer Architecture
- CRUD Operations
- Business Rules
- Pagination, Filtering and Sorting
- Image Upload and Management
- Global Exception Handling
- Unit Testing
- Integration Testing
- API Documentation with Swagger

---

👥 User Roles

The system supports three roles:

Admin

Administrators can:

- View pending properties
- Approve properties
- Reject properties with a reason
- View all properties
- Filter and sort properties
- View system statistics
- View users
- Delete properties
- Manage users

Owner

Property owners can:

- Register and login
- Create apartment listings
- View their own properties
- Update their properties
- Resubmit rejected properties
- Mark approved properties as sold
- Delete their properties
- Add property images
- Upload property images
- Delete their property images

Buyer

Buyers can:

- Register and login
- Browse approved apartments
- Search and filter properties
- Sort properties
- View property images
- Add properties to favorites
- View their favorite properties
- Remove properties from favorites

---

🏠 Property Workflow

New properties follow an approval workflow:

Owner creates property
        ↓
     Pending
        ↓
   ┌────┴────┐
   ↓         ↓
Approved   Rejected
   ↓         ↓
Published  Owner can resubmit
   ↓
  Sold

Only approved properties are publicly visible to buyers.

---

🔐 Authentication & Authorization

The API uses JWT (JSON Web Tokens) for authentication.

JWT tokens contain:

- User ID
- Email
- Role

Role-based authorization is implemented using ASP.NET Core authorization attributes.

Examples:

[Authorize(Roles = "Admin")]

[Authorize(Roles = "Owner")]

[Authorize(Roles = "Buyer")]

Passwords are securely hashed using BCrypt.

---

🛠️ Technologies Used

Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- JWT Authentication
- BCrypt

Database

- MySQL
- Entity Framework Core Migrations
- Pomelo Entity Framework Core Provider

API Documentation

- Swagger / OpenAPI

Testing

- xUnit
- ASP.NET Core Integration Testing
- Entity Framework Core InMemory Database

Development

- Visual Studio
- Git
- GitHub

---

🏗️ Architecture

The project follows a layered backend architecture:

Client
  │
  ▼
Controller Layer
  │
  ▼
Service Layer
  │
  ▼
Entity Framework Core
  │
  ▼
MySQL Database

Controller Layer

Responsible for:

- Receiving HTTP requests
- Authentication and authorization
- Validating request access
- Returning HTTP responses

Service Layer

Contains the application's business logic.

Examples:

- PropertyService
- FavoriteService
- PropertyImageService

This separation keeps controllers focused on HTTP concerns and makes the business logic easier to test and maintain.

Data Layer

"AppDbContext" manages communication between the application and the database using Entity Framework Core.

---

📦 Main Entities

The application contains the following main entities:

Owner

Stores user information and roles.

Id
Name
Email
Password
Role

Property

Stores apartment information.

Id
Title
Description
Price
Address
Neighborhood
Bedrooms
Bathrooms
Area
Status
RejectionReason
OwnerId

PropertyImage

Stores property image information.

Id
ImageUrl
PropertyId

Favorite

Connects buyers with their favorite properties.

Id
BuyerId
PropertyId

---

🔎 Property Search

The public property endpoint supports:

- Pagination
- Neighborhood filtering
- Minimum price
- Maximum price
- Number of bedrooms
- Number of bathrooms
- Minimum area
- Maximum area
- Sorting by price
- Sorting by area
- Newest properties

Example:

GET /api/Properties?pageNumber=1&pageSize=10&neighborhood=Smouha&minPrice=1000000&maxPrice=3000000&sortBy=price_asc

Only properties with the following status are returned publicly:

Approved

---

📄 Pagination

API responses use a reusable pagination DTO:

PageResultDto<T>

It provides:

- Items
- TotalCount
- PageNumber
- PageSize
- TotalPages

This allows the API to efficiently handle large numbers of properties.

---

🖼️ Property Images

Each property can have up to 10 images.

The API supports:

- Adding image URLs
- Uploading image files
- Retrieving property images
- Deleting images

Image operations are protected by authorization and ownership rules.

---

🛡️ Business Rules

The API implements several business rules, including:

- Only owners can create properties.
- The owner ID is taken from the authenticated JWT token.
- Owners cannot create properties for another user.
- New properties start as "Pending".
- Pending properties cannot be edited.
- Rejected properties can be resubmitted.
- Only administrators can approve or reject properties.
- Only approved properties are publicly visible.
- Only the property owner can manage their property.
- Buyers can only favorite approved properties.
- A buyer cannot favorite the same property more than once.
- Owners cannot mark another owner's property as sold.
- Users cannot access another user's favorites.
- Property images are limited to 10 per property.

---

⚠️ Global Exception Handling

The project includes global exception handling middleware.

Instead of duplicating exception-handling logic across controllers, unexpected exceptions are handled centrally.

This improves:

- Maintainability
- Consistency
- API reliability
- Error handling

---

🧪 Testing

The project includes both unit tests and integration tests.

Unit Tests

Business logic is tested independently using xUnit.

Examples include:

- Property service tests
- Favorite service tests

Integration Tests

Integration tests verify the complete HTTP pipeline including:

- Controllers
- Authentication
- Authorization
- Services
- Database operations
- Business rules

The integration test suite covers scenarios such as:

- JWT authentication
- Role authorization
- Property creation
- Property updates
- Property deletion
- Property approval
- Property rejection
- Property resubmission
- Sold properties
- Favorites
- Duplicate favorites
- Property image management
- Ownership restrictions

Test Result

Total Tests: 61
Passed:      61
Failed:       0

✅ 61/61 tests passed

---

📚 API Documentation

The project uses Swagger / OpenAPI to document and manually test the API endpoints.

Swagger allows developers to:

- Explore available endpoints
- View request parameters
- Send API requests
- Test authentication
- Test authorization
- Inspect API responses

---

⚙️ Configuration

Sensitive configuration values such as:

- Database connection strings
- JWT secret keys
- Administrator credentials

are kept outside the public source code.

Configuration files containing secrets are excluded from Git using ".gitignore".

For local development, configure the required settings in your local configuration files.

Example structure:

{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY"
  },
  "Admin": {
    "Email": "YOUR_ADMIN_EMAIL",
    "Password": "YOUR_ADMIN_PASSWORD"
  }
}

Do not commit real credentials or secret keys to GitHub.

---

▶️ How to Run the Project

1. Clone the repository

git clone https://github.com/EmanHamed24/REALESTATE.API

2. Open the solution

Open the solution in Visual Studio.

3. Configure the database

Create a MySQL database and configure the connection string in your local configuration.

4. Apply migrations

Run:

Update-Database

from the Visual Studio Package Manager Console.

5. Configure JWT

Add your local JWT secret key.

6. Configure Admin

Add the administrator email and password in your local configuration.

7. Run the application

Start the project from Visual Studio.

8. Open Swagger

Use the Swagger URL displayed by the application.

---

🔑 API Authentication

To access protected endpoints:

1. Login using the appropriate account.
2. Copy the returned JWT token.
3. Open Swagger's Authorize button.
4. Enter the token.
5. Test the protected endpoints according to the user's role.

---

📌 Main API Endpoints

Authentication

POST /api/Owners/register
POST /api/Owners/login
POST /api/Owners/change-password

Public Properties

GET /api/Properties
GET /api/Properties/{id}
GET /api/Properties/{id}/images

Owner Properties

GET    /api/Properties/my-properties
POST   /api/Properties
PUT    /api/Properties/{id}
PUT    /api/Properties/{id}/sold
DELETE /api/Properties/{id}
PUT    /api/Properties/resubmit/{id}

Property Images

POST   /api/Properties/{id}/images
POST   /api/Properties/{id}/images/upload
DELETE /api/Properties/{propertyId}/images/{imageId}

Favorites

POST   /api/Properties/{propertyId}/favorite
GET    /api/Properties/my-favorites
DELETE /api/Properties/{propertyId}/favorite

Admin

GET    /api/Properties/pending
GET    /api/Properties/admin/properties
GET    /api/Properties/admin-statistics
PUT    /api/Properties/approve/{id}
PUT    /api/Properties/reject/{id}

---

🔒 Security Features

The project implements several security practices:

- JWT authentication
- Role-based authorization
- BCrypt password hashing
- Ownership validation
- Protected admin endpoints
- Protected owner endpoints
- Protected buyer endpoints
- Input validation using Data Annotations
- Sensitive configuration excluded from Git
- Global exception handling
- Integration tests for authorization rules

---

📈 Future Improvements

Possible future improvements include:

- Redis caching
- Docker containerization
- CI/CD pipeline
- Refresh tokens
- Email notifications
- Advanced search
- Structured logging
- More comprehensive API versioning
- Deployment to a cloud platform

---

🎯 Project Goals

This project was built to demonstrate practical backend development skills with the .NET ecosystem, with a focus on:

- Clean backend architecture
- Secure authentication
- Authorization
- Database design
- Business logic
- Automated testing
- API development
- Maintainable code

---

👩‍💻 Author

Eman Hamed

Electrical Engineering — Computer & Systems

Aspiring .NET Backend Developer

---