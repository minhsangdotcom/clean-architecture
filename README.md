# Clean Architecture The Template

[English](README.md) | [Vietnamese](README-VIETNAMESE.md)

#

![Visual Studio Code](https://img.shields.io/badge/Visual%20Studio%20Code-0078d7.svg?logo=visual-studio-code&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![GitHub Release](https://img.shields.io/github/v/release/minhsangdotcom/clean-architecture?color=orange)
![GitHub Org's stars](https://img.shields.io/github/stars/minhsangdotcom%2Fclean-architecture?color=pink)
![GitHub forks](https://img.shields.io/github/forks/minhsangdotcom/clean-architecture?color=%23f61d9c)
[![NuGet Version](https://img.shields.io/nuget/v/minhsangdotcom.TheTemplate.SharedKernel?label=SharedKernel&color=red)](https://www.nuget.org/packages/minhsangdotcom.TheTemplate.SharedKernel)
[![NuGet Version](https://img.shields.io/nuget/v/TranMinhSang.DynamicQueryExtension.EntityFrameworkCore?label=DynamicQueryExtension&color=red)](https://www.nuget.org/packages/TranMinhSang.DynamicQueryExtension.EntityFrameworkCore)
[![NuGet Version](https://img.shields.io/nuget/vpre/minhsangdotcom.TheTemplate.SpecificationPattern?style=flat&label=SpecificationPattern&color=red)](https://www.nuget.org/packages/minhsangdotcom.TheTemplate.SpecificationPattern/)
[![NuGet Version](https://img.shields.io/nuget/vpre/minhsangdotcom.TheTemplate.ElasticsearchFluentConfig?style=flat&label=ElasticsearchFluentConfig&color=red)](https://www.nuget.org/packages/minhsangdotcom.TheTemplate.ElasticsearchFluentConfig/1.0.1-alpha)

# Table of Contents

- [1. Language](#1-languages)
- [2. Badges](#2-badge)
- [3. Table of Contents](#3-table-of-contents)
- [2. Introduction](#2-introduction)
- [3. Give a Star! ⭐](#3-give-a-star)
- [4. What is Clean Architecture?](#4-what-is-clean-architecture)
  - [4.0.1. Pros](#401-pros)
  - [4.0.2. Cons](#402-cons)
- [5. Features :rocket:](#5-features)
- [6. Demo :fire:](#6-demo)
  - [6.0.1. Apis](#601-api)
  - [6.0.2. Tracing](#602-tracing)
  - [6.0.3. AWS S3 Cloud](#603-aws-s3-by-minio)
  - [6.0.4. Message](#604-automatic-translatable-message)
- [7. Structure Overview :mag_right:](#7-structure-overview)
- [8. Getting started](#8-getting-started)
  - [8.1. Run .NET Core Clean Architecture Project](#81-run-net-core-clean-architecture-project)
  - [8.2. Basic Usage](#82-basic-usage)
    - [8.2.1. Authorize](#821-authorize)
    - [8.2.2. Create role with permissions:](#822-create-role-with-permissions)
    - [8.2.3. How to add new permissions in your app](#823-how-to-add-new-permissions-in-your-app)
    - [8.2.4. Filtering](#824-filtering)
    - [8.2.5. Pagination](#825-pagination)
- [9. Technology](#9-technology)
- [10. Support](#10-support)
- [11. Credits](#11-credits)
- [12. Licence](#12-licence)

# 2. Introduction

Clean Architecture template is designed for backend developer working with ASP.NET Core. It provides you an efficient way to build enterprise applications effortlessly by leveraging advantages of clean architecture structure and .NET Core framework.

With this template, everything is already set up :smiley:.

# 3. Give a Star

If you find this template helpful and learn something from it, please consider giving it a :star:.

# 4. What is Clean Architecture?

Clean Architecture is a software design approach introduced by Robert C. Martin (Uncle Bob) that emphasizes the separation of concerns by organizing code into concentric layers. The core idea is to keep business logic independent from external frameworks, databases, and user interfaces, promoting a system that's easier to maintain, test, and evolve over time.

![Alt text](Screenshots/clean-architecture.png "clean architecture common structure")

### 4.0.1 Pros

- **Separation of Concerns**: Each layer is responsible for a specific aspect of the application, making the code easier to understand and maintain.
- **Testability**: Since business logic is decoupled from frameworks and UI, unit testing becomes simpler and more reliable.
- **Flexibility and Adaptability**: Changes to the framework, database, or external systems have minimal impact on the core logic.
- **Reusability**: Business rules can be reused across different applications or systems with minimal changes.
- **Scalability**: The clear structure supports growth and the addition of new features without significant refactoring.
- **Framework Independence**: Avoids being locked into a specific framework, making it easier to migrate to newer technologies.

### 4.0.2 Cons

- **Complexity**: The layered structure can add complexity, especially for smaller projects where simpler architectures might suffice.
- **Initial Overhead**: Setting up Clean Architecture requires additional effort to organize layers and follow strict design principles.
- **Learning Curve**: Developers unfamiliar with the principles may take time to grasp the structure and its benefits.
- **Over-Engineering Risk**: For small-scale applications, the additional layers might be unnecessary and lead to over-complication.
- **Performance Overhead**: The abstraction and indirection between layers can introduce slight performance trade-offs, though typically negligible.

# 5. Features

What makes this Clean Architecture template stand out from the rest on Github?

### Most common features:

- Login :closed_lock_with_key:
- Refresh token :arrows_counterclockwise:
- Changing user password :repeat:
- Password reset :unlock:
- Retrieving and Updating user profile :man_with_gua_pi_mao:
- User CRUD :family:
- Role CRUD 🛡️

### Other awesome features:

1. [DDD (Domain Driven Design)](/src/Domain/Aggregates/) :brain:
1. [CQRS & Mediator](/src/Application/Features/) :twisted_rightwards_arrows:
1. [Cross-cutting concern](/src/Application/Common/Behaviors/) :scissors:
1. [Mail Sender](/src/Infrastructure/Services/Mail/) :mailbox:
1. [Cached Repository](/src/Infrastructure/UnitOfWorks/CachedRepositories/) :computer:
1. [Queue](/src/Infrastructure/Services/Queue/) [Example at feature/TicketSale](https://github.com/minhsangdotcom/clean-architecture/tree/feature/TicketSale) :walking:
1. [Logging](/src/Api/Extensions/SerialogExtension.cs) :pencil:
1. [Tracing](/src/Api/Extensions/OpenTelemetryExtensions.cs) :chart_with_upwards_trend:
1. [Automatical translatable messages](https://github.com/minhsangdotcom/the-template_shared-kernel) :globe_with_meridians:
1. [S3 AWS](/src/Infrastructure/Services/Aws/) :cloud:

# 6. Demo

### 6.0.1. API

![User Apis](/Screenshots/user-api.png)

![Role Apis](/Screenshots/role-api.png)

### 6.0.2. Tracing

![Tracing](/Screenshots/trace.png)

### 6.0.3. AWS S3 by Minio

![AWS s3 feature](Screenshots/AWS_S3_Feature.png)

### 6.0.4. Automatic Translatable Message

```json
{
  "type": "BadRequestError",
  "title": "Error has occured with password",
  "status": 400,
  "instance": "POST /api/v1/Users/Login",
  "ErrorDetail": {
    "message": "user_password_incorrect",
    "en": "Password of user is incorrect",
    "vi": "Mật khẩu của Người dùng không đúng"
  },
  "requestId": "0HNC1ERHD53E2:00000001",
  "traceId": "fa7b365b49f1b554a9cfabd978d858c8",
  "spanId": "8623dbe038a6dede"
}
```

# 7. Structure Overview

```
/Domain
  ├── /Aggregates/           # Domain aggregates (entities with business rules)
  └── /Common/               # Shared domain logic and base types
       ├── AggregateRoot.cs       # Base class for aggregate roots
       ├── BaseEntity.cs          # Base class for entities
       └── UlidToStringConverter.cs  # Value converter for ULIDs
```

```
/Application
  ├── /Common
  │     ├── /Auth/                   # custom authorization & policies in .NET Core
  │     ├── /Behaviors/              # MediatR pipeline behaviors (CQRS cross‑cutting)
  │     ├── /DomainEventHandlers/    # handlers for raising/domain events
  │     ├── /Errors/                 # error types for Result‑pattern responses
  │     ├── /Exceptions/             # domain/application exception definitions
  │     ├── /Extensions/             # helper methods (pagination, LHS parsing, etc.)
  │     ├── /Interfaces/             # application‑level contracts & abstractions
  │     ├── /QueryStringProcessing/  # validation logic for query‑string params
  │     └── /Security/               # security attributes (e.g. [Authorize], roles)
  ├── /Features/                     # CQRS + MediatR pattern modules
  │     ├── AuditLogs/               # commands & queries for audit‑trail
  │     ├── Common/                  # shared feature utilities
  │     ├── Permissions/             # manage app permissions
  │     ├── QueueLogs/               # logging for background/queued jobs
  │     ├── Regions/                 # region‑related commands & queries
  │     ├── Roles/                   # role management (CRUD, assignments)
  │     └── Users/                   # user‑centric commands & queries
  └── DependencyInjection.cs         # Registration of all Application services into DI

```

```
/Infrastructure
  ├── /Constants/                    # application-wide constants & credential definitions
  │     └── Credential.cs            # strongly-typed credentials (keys, secrets, etc.)
  │
  ├── /Data/                         # EF Core data layer: context, migrations, seeding, configs
  │     ├── /Configurations/         # IEntityTypeConfiguration<> implementations
  │     ├── /Interceptors/           # DbCommand/SaveChanges interceptors (logging, auditing)
  │     ├── /Migrations/             # EF Core migration files
  │     ├── /Seeds/                  # seed-data providers for initial data
  │     ├── DatabaseSettings.cs      # POCO for database connection/settings
  │     ├── DbInitializer.cs         # ensures DB is created & seeded on startup
  │     ├── DesignTimeDbContextFactory.cs  # design-time factory for `dotnet ef` commands
  │     ├── RegionDataSeeding.cs           # specific seed logic for Regions table
  │     ├── TheDbContext.cs                # your `DbContext` implementation
  │     └── ValidateDatabaseSetting.cs     # runtime validation of DB settings
  │
  ├── /Services/                     # external/infrastructure services & integrations
  │     ├── /Aws/                    # AWS SDK wrappers (S3, SNS, etc.)
  │     ├── /Cache/                  # caching implementations (Redis, MemoryCache)
  │     ├── /ElasticSearch/          # Elasticsearch client & indexing/search logic
  │     ├── /Hangfire/               # background-job scheduler configuration
  │     ├── /Identity/               # identity provider integrations (JWT, OAuth)
  │     ├── /Mail/                   # SMTP, SendGrid, or other mail-sending services
  │     ├── /Queue/                  # Request queueing with Redis
  │     ├── /Token/                  # token-related services and helpers
  │     ├── ActionAccessorService.cs # grabs current `HttpContext` action info
  │     └── CurrentUserService.cs    # resolves authenticated user details
  │
  ├── /UnitOfWorks/                  # Unit-of-Work & repository abstractions
  │     ├── /CachedRepositories/     # repositories with built-in caching layers
  │     ├── /Repositories/           # concrete repository implementations
  │     ├── RepositoryExtension.cs   # extension methods for IRepository<T>
  │     └── UnitOfWork.cs            # coordinates multiple repository commits
  │
  └── DependencyInjection.cs         # registration of all Infrastructure services into DI
```

```
/Api
  ├── /common/                         # shared helpers, configurations for API layer
  │
  ├── /Converters/                     # JSON/string converters for date types
  │     ├── DateTimeConverter.cs           # custom converter for System.DateTime
  │     └── DateTimeOffsetConverter.cs     # custom converter for System.DateTimeOffset
  │
  ├── /Endpoints/                      # minimal‑API endpoint definitions
  │
  ├── /Extensions/                     # extension methods (IServiceCollection, HttpContext, etc.)
  │
  ├── /Middlewares/                    # custom middleware (error handling, logging, auth, etc.)
  │
  ├── /Resources/                      # static resource files
  │     └── /Translations/               # localization .resx files
  │           ├── Message.en.resx           # English resource strings
  │           └── Message.vi.resx           # Vietnamese resource strings
  │
  ├── /Settings/                       # POCOs bound to appsettings.json sections
  │     ├── OpenApiSettings.cs             # swagger/OpenAPI configuration
  │     ├── OpenTelemetrySettings.cs       # OTEL exporter/tracing settings
  │     └── SerilogSettings.cs             # Serilog sink & logging configuration
  │
  └── /wwwroot/                        # publicly served static content
        └── /Templates/                   # email/html templates, static assets
```

```
            +-----------------------------------------------+
            |                   Api                         |
            +-----------------------------------------------+
             |                     |                    |
             |                     |                    |
             ↓                     |                    |
        +------------------+       |                    |
        |  Infrastructure  |       |                    |
        +------------------+       |                    |
                        |          |                    |
                        ↓          ↓                    ↓
                    +--------------------+    +---------------------+
                    |   Application      | -> | Contracts           |
                    +--------------------+    +---------------------+
                             |
                             ↓
            +---------------------------+
            |          Domain           |
            +---------------------------+

```

# 8. Getting started

## 8.1. Run .NET Core Clean Architecture Project

The following prerequisites are required to build and run the solution:

- [Net 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/)

The first step :point_up: :

Create a appsettings.Development.json file at root of Api layer and just copy the content of appsettings.example.json to the file then Modify configurations in your case.

Modify PostgreSQL connection string (this template is using PostgreSQL currently).

```json
"DatabaseSettings": {
    "DatabaseConnection": "Host=localhost;Username=[your_username];Password=[your_password];Database=example"
},
```

Update migrations to your own database.

```
cd src/Infrastructure

dotnet ef database update
```

The next step :point_right::

```
cd Dockers/MinioS3

```

change mino username and password at .env if needed and you're gonna use it for logging in Web UI Manager

```
MINIO_ROOT_USER=the_template_storage
MINIO_ROOT_PASSWORD=storage@the_template1

```

To Run Amazon S3 service for media file storage.

```
docker-compose up -d

```

Access Minio S3 Web UI at http://localhost:9001 and login

![S3 login](/Screenshots/S3-login.png)

Create a pairs of key like

![S3 keys](/Screenshots/create-key-s3.PNG)

input the keys at your appsettings.json

```json
"S3AwsSettings": {
      "ServiceUrl": "http://localhost:9000",
      "AccessKey": "***",
      "SecretKey": "***",
      "BucketName": "the-template-project",
      "PublicUrl": "http://localhost:9000",
      "PreSignedUrlExpirationInMinutes": 1440,
      "Protocol": 1
    },
```

The final step

```
cd src/Api
dotnet run
```

http://localhost:8080/docs is swagger UI path

The default admin account <ins>username:</ins> <b>chloe.kim</b>, <ins>password</ins>: <b>Admin@123</b>

Congrats! you are all set up :tada: :tada: :tada: :clap:

## 8.2. Basic Usage

### 8.2.1. Authorize

To Achieve this, let's add RequireAuth on minimal api, permissions parameter is string and Each permission is separated by comma like "create:user,update:user".

```csharp
app.MapPost(Router.UserRoute.Users, HandleAsync)
    .WithOpenApi(operation => new OpenApiOperation(operation)
    {
        Summary = "Create user 🧑",
        Description = "Creates a new user and returns the created user details.",
        Tags = [new OpenApiTag() { Name = Router.UserRoute.Tags }],
    })
    .WithRequestValidation<CreateUserCommand>()
    .RequireAuth(
        permissions: Permission.Generate(PermissionAction.Create, PermissionResource.User)
    )
    .DisableAntiforgery();
```

### 8.2.2. Create role with permissions:

Json payload is like

```json
{
  "description": "this is super admin role",
  "name": "superAdmin",
  "roleClaims": [
    {
      "claimType": "permission",
      "claimValue": "create:customer"
    },
    {
      "claimType": "permission",
      "claimValue": "update:customer"
    }
  ]
}
```

### 8.2.3. How to add new permissions in your app

To get this, let's navigate to constants folder in Infrastructure layer, then open Credential.cs file and pay your attention on permissions list

```csharp
public static readonly List<Dictionary<string, List<string>>> permissions =
    [
        Permission.CreatebasicPermissions(PermissionResource.User),
        Permission.CreatebasicPermissions(PermissionResource.Role),
    ];
```

Notice that, the key is **primary permission** and value is **list of relative permissions**

Permission combines from action and entity name.
For example:

```
create:user
```

Let's take a look at PermissionAction and PermissionResource class

```csharp
public class PermissionAction
{
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Detail = nameof(Detail);
    public const string List = nameof(List);
    public const string Test = nameof(Test);
    public const string Testing = nameof(Testing);
}

public class PermissionResource
{
    public const string User = nameof(User);
    public const string Role = nameof(Role);
}
```

Define your new one at permissions list then stop and start application again

### 8.2.4. Filtering

To do filter in this template, we use LHS Brackets.

LHS is the way to encode operators is the use of square brackets [] on the key name.

For example

```
GET api/v1/users?filter[dayOfBirth][$gt]="1990-10-01"
```

This example indicates filtering out users whose birthdays are after 1990/10/01

All support operations:

| Operator      | Description                         |
| ------------- | ----------------------------------- |
| $eq           | Equal                               |
| $eqi          | Equal (case-insensitive)            |
| $ne           | Not equal                           |
| $nei          | Not equal (case-insensitive)        |
| $in           | Included in an array                |
| $notin        | Not included in an array            |
| $lt           | Less than                           |
| $lte          | Less than or equal to               |
| $gt           | Greater than                        |
| $gte          | Greater than or equal to            |
| $between      | Is between                          |
| $notcontains  | Does not contain                    |
| $notcontainsi | Does not contain (case-insensitive) |
| $contains     | Contains                            |
| $containsi    | Contains (case-insensitive)         |
| $startswith   | Starts with                         |
| $endswith     | Ends with                           |

Some Examples:

```
GET /api/v1/users?filter[gender][$in][0]=1&filter[gender][$in][1]=2
```

```
GET /api/v1/users?filter[gender][$between][0]=1&filter[gender][$between][1]=2
```

```
GET /api/v1/users?filter[firstName][$contains]=abc
```

$and and $or operator:

```
GET /api/v1/users/filter[$and][0][firstName][$containsi]="sa"&filter[$and][1][lastName][$eq]="Tran"
```

```JSON
{
  "filter": {
    "$and": {
      "firstName": "sa",
      "lastName": "Tran"
    }
  }
}
```

```
GET /api/v1/users/filter[$or][0][$and][0][claims][claimValue][$eq]=admin&filter[$or][1][lastName][$eq]=Tran
```

```JSON
{
    "filter": {
        "$or": {
            "$and":{
                "claims": {
                    "claimValue": "admin"
                }
            },
            "lastName": "Tran"
        }
    }
}
```

For more examples and get better understand, you can visit

[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#filtering)\
[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#complex-filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#complex-filtering)\
[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#deep-filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#deep-filtering)

'Cause I designed filter input based on [Strapi filter](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication)

To Apply dynamic filter, you just call any list method at

```csharp
unitOfWork.DynamicReadOnlyRepository<User>()
```

### 8.2.5. Pagination

This template supports offset pagination and cursor pagination.

To Enable offset pagination just add this line

```csharp
var response = await unitOfWork
    .DynamicReadOnlyRepository<User>(true)
    .PagedListAsync(
        new ListUserSpecification(),
        query,
        ListUserMapping.Selector(),
        cancellationToken: cancellationToken
    );
```

To Enable cursor pagination just add this line

```csharp
var response = await unitOfWork
    .DynamicReadOnlyRepository<User>(true)
    .CursorPagedListAsync(
        new ListUserSpecification(),
        query,
        ListUserMapping.Selector(),
        cancellationToken: cancellationToken
    );
```

```json
{
  "results": {
    "data": [
      {
        "firstName": "sang",
        "lastName": "minh",
        "username": "sang.minh123",
        "email": "sang.minh123@gmail.com",
        "phoneNumber": "0925123320",
        "dayOfBirth": "1990-01-09T17:00:00Z",
        "gender": 2,
        "address": "abcdef,Xã Phước Vĩnh An,Huyện Củ Chi,Thành phố Hồ Chí Minh",
        "avatar": null,
        "status": 1,
        "createdBy": "01JD936AXSDNMQ713P5XMVRQDV",
        "updatedBy": "01JD936AXSDNMQ713P5XMVRQDV",
        "updatedAt": "2025-04-16T14:26:01Z",
        "id": "01JRZFDA1F7ZV4P7CFS5WSHW8A",
        "createdAt": "2025-04-16T14:17:54Z"
      }
    ],
    "paging": {
      "pageSize": 1,
      "totalPage": 3,
      "hasNextPage": true,
      "hasPreviousPage": false,
      "before": null,
      "after": "q+blUlBQci5KTSxJTXEsUbJSUDIyMDLVNTDRNTQLMTK0MjS3MjXRMzG3tDAx1DYwtzIwUNIB6/FMASk2MPQKinJzcTR0M48KMwkwd3YLNg0P9gi3cFTi5aoFAA=="
    }
  },
  "status": 200,
  "message": "Success"
}
```

# 9. Technology

- .NET 8
- EntityFramework core 8
- PostgresSQL
- FluentValidation
- Mediator
- XUnit, Shouldly, Respawn
- OpenTelemetry
- Serilog
- Redis
- ElasticSearch
- Docker
- GitHub Workflow

# 10. Support

If you are having problems, please let me know at [issue section](https://github.com/minhsangdotcom/clean-architecture/issues).

# 11. Credits

- [Clean architecture by Jayson Taylor](https://github.com/jasontaylordev/CleanArchitecture)

- [Clean architecture by amantinband](https://github.com/amantinband/clean-architecture)
- [Clean architecture by Ardalis](https://github.com/ardalis/CleanArchitecture)
- [Specification pattern](https://github.com/ardalis/Specification)
- [REPR Pattern](https://github.com/ardalis/ApiEndpoints)
- [Clean testing by Jayson Taylor](https://github.com/jasontaylordev/CleanArchitecture/tree/main/tests)

# 12. License

This project is licensed with the [MIT license](LICENSE).
