# MyUMCApp - United Methodist Church Members Application

A modern, cloud-native application for managing United Methodist Church membership and activities.

## Features

- Multi-language support (English, Shona, Ndebele)
- Theme switching (Light/Dark mode)
- Member management
- Church content management (Sermons, Blog, Announcements)
- Event management
- E-commerce store
- Mobile-responsive design

## Technology Stack

- Frontend: Blazor WebAssembly
- Backend: .NET 8 Microservices
- Database: Amazon RDS (MySQL)
- Cloud: AWS Free Tier Services
- Authentication: AWS Cognito
- API Management: Amazon API Gateway
- Storage: Amazon S3
- CDN: Amazon CloudFront
- Messaging: Amazon SNS/SQS

## Prerequisites

- .NET 8 SDK
- AWS Account (Free Tier)
- Node.js (Latest LTS)
- Git

## Project Structure

```
MyUMCApp/
├── src/
│   ├── MyUMCApp.Client/          # Blazor WebAssembly Frontend
│   ├── MyUMCApp.Identity.API/    # Authentication & Authorization
│   ├── MyUMCApp.Members.API/     # Member Management
│   ├── MyUMCApp.Content.API/     # Content Management
│   ├── MyUMCApp.Store.API/       # E-commerce
│   └── MyUMCApp.Shared/          # Shared Components
└── tests/
    ├── MyUMCApp.Client.Tests/
    ├── MyUMCApp.Identity.Tests/
    ├── MyUMCApp.Members.Tests/
    ├── MyUMCApp.Content.Tests/
    ├── MyUMCApp.Store.Tests/
    └── MyUMCApp.Integration.Tests/
```

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/MyUMCApp.git
cd MyUMCApp
```

2. Install dependencies:
```bash
dotnet restore
```

3. Run the application:
```bash
# Run the Blazor Client
cd src/MyUMCApp.Client
dotnet run

# Run the APIs (in separate terminals)
cd src/MyUMCApp.Identity.API
dotnet run

cd src/MyUMCApp.Members.API
dotnet run

cd src/MyUMCApp.Content.API
dotnet run

cd src/MyUMCApp.Store.API
dotnet run
```

4. Run tests:
```bash
dotnet test
```

## AWS Configuration

1. Create an AWS account and configure AWS CLI
2. Set up required AWS services:
   - Cognito User Pool
   - RDS MySQL instance
   - S3 buckets
   - API Gateway
   - Lambda functions
   - SNS topics
   - SQS queues

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 