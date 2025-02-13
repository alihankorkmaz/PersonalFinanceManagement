# Personal Finance Management Rest API

## Overview

This is a REST API designed for personal finance management. It includes features for user and admin registration, authentication, and management of user transactions.

## Features

- **User Authentication**: Register and login as a user or admin.
- **Admin Management**: Admins can manage users and their own profile.
- **User Self-Management**: Users can update their profiles and manage transactions.
- **JWT Authentication**: Secure endpoints using JSON Web Tokens.

## Technologies Used

- **Backend**: ASP.NET Core
- **Database**: Microsoft SQLite Server
- **Authentication**: JWT (JSON Web Tokens)

## Prerequisites

Before running this project, ensure you have the following installed:

- .NET 6 SDK or later
- SQL Lite Server
- Visual Studio or Visual Studio Code

## Installation

1. **Clone the Repository**
   ```bash
   [git clone https://github.com/your-username/your-repository-name.git]
   cd PersonalFinanceManagement
2. Set Up the Database

Update the appsettings.json file with your database connection string under ConnectionStrings:DefaultConnection.
Apply the migrations to create the database:
dotnet ef database update

Run the Application
Start the application locally:
dotnet run
The API will be accessible at https://localhost:7050/swagger/index.html (or the default port configured).
## Usage Examples

Below are some sample JSON payloads for testing the API via Swagger or Postman.

### Register as User
**Endpoint**: `POST /api/UserRegister/register`
```json
{
  "name": "testName",
  "email": "testmail@hotmail.com",
  "password": "testpassword"
}
```
### Register as Admin
**Endpoint**: `POST /api/AdminRegister/register`
```json
{
  "name": "testAdmin",
  "email": "testadminmail@msn.com",
  "password": "test123"
}
```
### Login as User or Admin // Use same username and password with registered, you will take token with role (admin,user)
**Endpoint**: `POST /api/Token/Login`
```json
{
  "email": "testmail@hotmail.com",
  "password": "testpassword"
} 
```
//just copy token and paste it in Authorize on the right-top to access feature requests (paste only token {token})

### ---*Transaction*----


### Add Transaction
**Endpoint**: `POST /api/Transaction`
```json
{
  "amount": 0,
  "date": "2025-01-26T20:21:25.815Z",
  "category": "string",
  "description": "string"
}
 ``` 
### Change Transaction
**Endpoint:** `PUT /api/Transaction`
```json
{
  "id": 0,
  "amount": 0,
  "date": "2025-01-26T20:25:34.610Z",
  "category": "string",
  "description": "string"
}
```
## Running Tests

This project includes unit tests to verify the API's functionality. The tests are written using MSTest and cover various scenarios, including:

- ✅ User registration with valid/invalid data
- ✅ Retrieving user information
- ✅ Updating user details
- ✅ Deleting users and verifying deletion



