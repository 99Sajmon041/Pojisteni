# Insurance Management System (ASP.NET Core MVC)

This is a web application for managing insurance and policyholders. The project is built using ASP.NET Core MVC, Entity Framework Core, and SQL Server. It includes role-based access for administrators and regular users.

## Features

- Manage policyholders (add, edit, delete)
- Manage insurance policies and types
- Record and view insurance claims
- Role-based access:
  - **Admin**: full access to insurance, policyholders, claims
  - **User**: view and select available insurance
- Login and registration system
- Basic responsive UI using Bootstrap

## Tech Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap
- Identity (for user authentication)

## Project Structure

- `Models/` – Data models (Insurance, Policyholder, User, Claim, etc.)
- `Controllers/` – MVC controllers (e.g., InsuranceController, AccountController)
- `Views/` – Razor views for each section
- `Data/` – ApplicationDbContext and DB initialization

## Roles & Permissions

| Role     | Access Level                      |
|----------|-----------------------------------|
| Admin    | Full CRUD on all data             |
| User     | View and select insurances only   |

