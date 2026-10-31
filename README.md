# Course Registration

A small ASP.NET Core and EF Core service implementing enrollment, a FIFO waitlist, drops, prerequisite checks, and waitlist promotion for course offerings.

## Projects

- `CourseRegistration.Api` – HTTP endpoints and application startup
- `CourseRegistration.Application` – service contracts, result/request DTOs, and registration rules
- `CourseRegistration.Domain` – learner, course offering, enrollment, and waitlist entities
- `CourseRegistration.Infrastructure` – EF Core SQLite persistence, repositories, and transactional unit of work
- `CourseRegistration.Tests` – xUnit behavior tests using SQLite in-memory databases

## Requirements

- .NET 10 SDK

## Build and test

From this directory:

```bash
dotnet restore CourseRegistration.sln
dotnet build CourseRegistration.sln
dotnet test CourseRegistration.sln
```

## Run the API

```bash
dotnet run --project CourseRegistration.Api
```

The API creates `course-registration.db` in the working directory on first run. Configure the `ConnectionStrings:CourseRegistration` setting to change the database path. The current schema is created with `EnsureCreated`; migrations are not yet included.

Endpoints:

- `POST /api/CourseRegistration/enroll` with `{ "courseOfferingId": 1, "learnerId": 1 }`
- `POST /api/CourseRegistration/drop` with `{ "courseOfferingId": 1, "learnerId": 1 }`

The starter API intentionally has no course or learner administration endpoints or seed data. Populate the `Learners` and `CourseOfferings` tables before calling these endpoints. See [DESIGN.md](DESIGN.md) for the persistence, invariants, policy, and limitations.

AI assistance and verification are recorded in [AI_USAGE.md](AI_USAGE.md).
