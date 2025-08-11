# Movies.VerticalSlice.Api Solution

A modular movie management system built with .NET 8 and C# 12,
featuring a secure ASP.NET Core Web API and a modern WPF client. T
he solution follows the Vertical Slice Architecture for clear separation of features and maintainability. 
Comprehensive automated tests ensure reliability across both API and UI layers.

## Features

- **ASP.NET Core Web API**
  - RESTful endpoints for managing movie data (CRUD operations)
  - Secure authentication and token management
  - Clean separation of concerns using MediatR and DTOs
  - Vertical slice feature organization for maintainability

- **WPF Client**
  - MVVM-based desktop application for consuming the API
  - Responsive UI for listing, adding, and managing movies
  - Secure login and token handling

- **Testing**
  - Unit and integration tests for API and services (xUnit)
  - Automated UI tests for WPF client (Telerik Test Studio, xUnit)

## Solution Structure

- `Movies.VerticalSlice.Api`  
  ASP.NET Core Web API project implementing movie management features.

- `Movies.VerticalSlice.Api.Wpf`  
  WPF client application using MVVM to interact with the API.

- `Movies.VerticalSlice.Api.Services`  
  Shared service layer for business logic and data access.

- `Movies.VerticalSlice.Api.Shared`  
  Shared DTOs and contracts between API and client.

- `Movies.VerticalSlice.Api.Wpf.Tests`  
  UI and ViewModel tests for the WPF client.

- `Movies.Api.VerticalSlice.Api.Tests`  
  Unit and integration tests for the API and services.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or later

### Build and Run

1. **Clone the repository**
