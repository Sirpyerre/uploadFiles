
# FileUploadApi

A learning project built with **.NET 8**, following **Clean Architecture** principles.  
The API allows users to register, upload and manage photos/documents, with file storage simulated using **AWS S3 (LocalStack)** and persistence with **SQL Server** running in Docker.

---

## Features

- REST API built with **ASP.NET Core**
- **Entity Framework Core** with SQL Server
- **Repository + Service layers** for clean separation of concerns
- **File upload support** for user photos and documents
- **S3 integration** (via LocalStack for local development)
- **Health check endpoint** (`/health`)
- Dockerized environment with SQL Server and LocalStack

---

## Project Structure

```

FileUploadApi
├── FileUploadApi.API            # ASP.NET Core Web API (controllers, DI setup, Program.cs)
├── FileUploadApi.Application    # Business logic (services, interfaces, DTOs)
├── FileUploadApi.Domain         # Core domain (entities, aggregates)
├── FileUploadApi.Infrastructure # Data access, EF Core DbContext, repository implementations
└── docker-compose.yml           # Local development environment (SQL Server, LocalStack)

````

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (tested on macOS with Apple Silicon/M4)
- [AWS CLI](https://docs.aws.amazon.com/cli/) (optional, for interacting with LocalStack)

---

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/your-username/FileUploadApi.git
cd FileUploadApi
````

### 2. Start infrastructure (SQL Server + LocalStack)

```bash
docker compose up -d
```

### 3. Apply EF Core migrations

```bash
dotnet ef database update --project FileUploadApi.Infrastructure --startup-project FileUploadApi.API
```

### 4. Add a bucket to LocalStack 

Using AWS CLI and LocalStack, run the following command in your terminal:

```bash
aws --endpoint-url=http://localhost:4566 s3 mb s3://user-photos
```
This will create a bucket named `user-photos` locally. You should see the output:

```
make_bucket: user-photos
```

### 5. Run the API

```bash
dotnet run --project FileUploadApi.API
```

The API will be available at: [https://localhost:5001](https://localhost:5001)
Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)

---

## Endpoints

### Health Check

```http
GET /health
```

### Upload User Photo

```http
POST /api/userphotos/{userId}
Content-Type: multipart/form-data
```

* Uploads a photo for the given user.
* If a photo already exists, the previous one is deleted from S3 and updated in the database.

---

## Configuration

Configuration is stored in `appsettings.json`:

```json
"Storage": {
  "BucketNamePhotos": "user-photos",
  "BucketNameDocument": "user-documents",
  "Region": "us-east-1",
  "EndpointUrl": "http://localhost:4566",
  "AccessKey": "test",
  "SecretKey": "test"
}
```

## Development Notes

* Uses **Clean Architecture** principles:

    * `Domain` contains entities and core logic
    * `Application` defines interfaces and services
    * `Infrastructure` implements repositories and database access
    * `API` is the entry point (controllers, DI, middleware)
* LocalStack simulates AWS S3 for uploads
* SQL Server container is forced to `amd64` for Apple Silicon compatibility (slower, but works)


## Next Steps (for learning)

* Add **JWT Authentication**
* Create endpoints for **user registration & login**
* Extend to **upload/download documents**
* Implement **unit tests** with xUnit + Moq
* Deploy to **Azure** or **AWS**


## License

MIT License.
This project is for learning purposes.
