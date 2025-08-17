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

# Bảng nội dung <div id= "bang-noi-dung"/>

- [Ngôn ngữ](#)
- [Nhãn](#)
- [Bảng nội dung](#bang-noi-dung)
- [Giới thiệu](#gioi-thieu)
- [Cho mình 1 ⭐ nhé](#cho-minh-sao-nhe)
- [Định Nghĩa](#dinh-nghia)
  - [Lợi ích](#lợi-ích)
  - [Nhược điểm](#nhược-điểm)
- [Tính năng :rocket:](#tinh-nang)
- [Nhá hàng cho các tính năng :fire:](#nha-hang-cho-cac-tinh-nang)
  - [Api](#api)
  - [Truy vết](#truy-vet)
  - [Lưu trử file media bằng Minio](#minio-storage)
  - [Tự động dịch message](#message-translation)
- [Sơ lượt về Cấu trúc :mag_right:](#so-luot-ve-cau-truc)
- [Bắt đầu thôi nào](#bắt-đầu-thôi-nào)
  - [Cách để chạy ứng dụng](#cách-để-chạy-ứng-dụng)
  - [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
    - [Authorize](#authorize)
    - [Thêm một quyền mới vào ứng dụng](#thêm-một-quyền-mới-vào-ứng-dụng)
    - [Bộ lọc](#bộ-lọc)
    - [Phân trang](#phân-trang)
- [Công nghệ](#công-nghệ)
- [Hỗ trợ](#hỗ-trợ)
- [Lời cảm ơn](#lời-cảm-ơn)
- [Cấp phép](#cấp-phép)
<div id="gioi-thieu" />

# Giới thiệu 

Template này được thiết kế dành cho các bạn backend làm việc với ASP.NET Core. Nó cung cấp một cách hiệu quả để xây dựng các ứng dụng enterprise một cách đơn giản bằng cách tận dụng lợi thế từ kiến trúc Clean Architecture và .NET Core framework.

Với template này, tất cả đã được thiết lập sẵn :smiley:.
<div id='cho-minh-sao-nhe'/>

# Cho mình 1 ⭐ nhé 

Nếu bạn thấy template này hữu ích và học được điều gì đó từ nó, hãy cân nhắc cho mình một :star:.

Sự hỗ trợ của bạn là động lực giúp mình mang đến những tính năng mới và cải tiến tốt hơn trong các phiên bản sắp tới.
<div id ="dinh-nghia"/>

# Định Nghĩa

Kiến trúc Sạch (Clean Architecture) là một phương pháp thiết kế phần mềm do Robert C. Martin (Uncle Bob) giới thiệu, nhấn mạnh vào thuật ngữ "Tách biệt các thành phần",các tầng ngoài cùng sẽ phụ thuộc vào các tầng bên trong như hình minh họa. Tầng core sẽ không phụ thuộc vào các framework bên ngoài, cơ sở dữ liệu hay giao diện người dùng, từ đó giúp hệ thống dễ bảo trì, kiểm thử và phát triển theo thời gian.

![Alt text](Screenshots/clean-architecture.png "Cấu trúc chung của Clean Architecture")

### Lợi ích

- **Các thành phần tách biệt**: Mỗi một tầng chịu trách nhiệm cho một khía cạnh của ứng dụng, giúp mã dễ hiểu và bảo trì.
- **Dễ dàng kiểm thử**: Các business logic được tách biệt khỏi framework và UI, việc kiểm thử đơn vị trở nên đơn giản và đáng tin cậy hơn.
- **Linh hoạt và Thích nghi**: Khi thay đổi framework, cơ sở dữ liệu hoặc các hệ thống bên ngoài ít ảnh hưởng đến logic của phần core.
- **Tái sử dụng**: Các Business rules có thể được tái sử dụng trong các ứng dụng hoặc hệ thống khác mà không phải thay đổi quá nhiều code.
- **Khả năng mở rộng**: Cấu trúc rõ ràng hỗ trợ việc phát triển và thêm tính năng mới mà không cần tái cơ cấu lại.
- **Không phụ thuộc vào framework**: Không bị phụ thuộc nhiều vào framework, rất dễ dàng để thanh đổi công nghệ mới.

### Nhược điểm

- **_Phức tạp_**: Cấu trúc các tầng có thể tăng tính phức tạp, đặc biệt đối với các dự án nhỏ nơi các kiến trúc đơn giản hơn có thể phù hợp hơn
- **_Chi phí khởi đầu cao_**: Thiết lập Kiến Trúc Sạch yêu cầu thêm nỗ lực để tổ chức các tầng và tuân theo các nguyên tắc thiết kế nghiêm ngặt.
- **_Khó khăn khi học tập_**: Các developer không quen thuộc với nguyên tắc này có thể mất thời gian để hiểu rõ cấu trúc và lợi ích của nó.
- **_Nguy cơ về cấu trúc quá phức tạp_**: Đối với các ứng dụng nhỏ, các tầng bổ sung có thể không cần thiết và dẫn đến sự phức tạp hóa.
- **_Hiệu năng bị suy giảm_**: Sự trích dẫn và trừa tượng(interface) giữa các tầng có thể giảm hiệu năng, tuy nhiên thường là không đáng kể.
<div id='tinh-nang'/>

# Tính năng :rocket:

Có gì đặc biệt khiến cho template này trở nên khác biệt so với những template khác có trên Github?

### Tính năng cần thiết cho mọi dự án:

- Đăng nhập :closed_lock_with_key:
- Refresh token :arrows_counterclockwise:
- Đổi mật khẩu :repeat:
- Quên mật khẩu :unlock:
- Xem và cập nhật profile người dùng :man_with_gua_pi_mao:
- User CRUD :family:
- Role CRUD 🛡️

### Một số tính năng hữu ích khác:

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
<div id= 'nha-hang-cho-cac-tinh-nang'/>

# Nhá hàng cho các tính năng :fire:

### API

![User Apis](/Screenshots/user-api.png)

![Role Apis](/Screenshots/role-api.png)
<div id='truy-vet'/>

### Truy Vết

![Tracing](/Screenshots/trace.png)
<div id='minio-storage'/>

### Lưu trử file media bằng Minio

![AWS s3 feature](Screenshots/AWS_S3_Feature.png)
<div id='message-translation'/>

### Tự động dịch message

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
<div id='so-luot-ve-cau-truc'/>

# Sơ lượt về Cấu trúc :mag_right:

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

# Bắt đầu thôi nào

## Cách để chạy ứng dụng

Các thứ cần để chạy ứng dụng:

- [Net 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/)

Bước thứ 1 :point_up: :

Tạo 1 file tên appsettings.Development.json ở ngoài cùng của tầng Api, Sao chép nội dung của appsettings.example.json vào file mới tạo và sau đó điều chỉnh lại các cấu hình theo cách của bạn.

Chỉnh sửa connection string của PostgreSQL (Bởi vì template này đang sử dụng PostgreSQL).

```json
"DatabaseSettings": {
    "DatabaseConnection": "Host=localhost;Username=[your_username];Password=[your_password];Database=example"
},
```

Cập nhật migration lên database

```
cd src/Infrastructure

dotnet ef database update
```

Bước tiếp theo nha :point_right::

```
cd Dockers/MinioS3

```

Đổi tên username và password ở file .env nếu cần thiết, lát nữa các bạn sẽ dùng nó để đăng nhập vào web manager đó.

```
MINIO_ROOT_USER=the_template_storage
MINIO_ROOT_PASSWORD=storage@the_template1

```

Dùng lệnh sau đây để chạy Amazon S3 service

```
docker-compose up -d

```

Truy cập http://localhost:9001 và đăng nhập

![S3 login](/Screenshots/S3-login.png)

Tạo ra cặp key

![S3 keys](/Screenshots/create-key-s3.PNG)

Chỉnh lại setting ở your appsettings.json

```json
"S3AwsSettings": {
      "ServiceUrl": "http://localhost:9000",
      "AccessKey": "",
      "SecretKey": "",
      "BucketName": "the-template-project",
      "PublicUrl": "http://localhost:9000",
      "PreSignedUrlExpirationInMinutes": 1440,
      "Protocol": 1
    },
```

Bước cuối nha

```
cd src/Api
dotnet run

```

vào swagger ui ở http://localhost:8080/docs

Tài khoản admin mặc định là <ins>username:</ins> <b>chloe.kim</b>, <ins>password</ins>: <b>Admin@123</b>

Xong rồi đó :tada: :tada: :tada: :clap:

## Hướng dẫn sử dụng

### Authorize

Để phân quyền cho nó sử dụng RequireAuth vào minimal api,
tham số permissions là kiểu string, các quyền được phân tách bởi dấu phẩy.

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

**_Tạo ra role kèm theo permission_**

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

### Thêm một quyền mới vào ứng dụng

Vào thư mục Constants trong Infrastructure mở file Credential.cs và chú ý tới permissions

```csharp
public static readonly List<Dictionary<string, List<string>>> permissions =
    [
        Permission.CreatebasicPermissions(PermissionResource.User),
        Permission.CreatebasicPermissions(PermissionResource.Role),
    ];
```

Chú ý rằng, key là quyền chính còn value là danh sách quyền liên quan của nó

Permission được gộp từ hành động và tên entity.
VD:

```
create:user
```

Đây là nơi để tạo ra các PermissionAction từ lớp ActionPermission và PermissionResource.

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

Tạo ra permission mới sau đó thêm nó vào list tên là permissions, tắt và chạy lại ứng dụng.
<div id='filtering'/>

### Bộ lọc

Để thực hiện tính năng filter, Chúng ta sẽ sử dụng cú pháp LHS Brackets.

LHS là cách để sử dụng các phương thức trong dấu ngoặc vuông cho key

VD:

```
GET api/v1/users?filter[dayOfBirth][$gt]="1990-10-01"
```

Ví dụ này nói rằng hãy lấy ra cho tôi tất cả những người có ngày sinh sau ngày 01 tháng 10 năm 1990

Tất cả các phương thức:

| Operator      | Description                                |
| ------------- | ------------------------------------------ |
| $eq           | So sánh bằng                               |
| $eqi          | So sánh bằng (Không phân biệt hoa thường)  |
| $ne           | Không bằng                                 |
| $nei          | Không bằng (Không phân biệt hoa thường)    |
| $in           | Lọc ra các kết quả Có trong mảng này       |
| $notin        | Lọc ra các kết quả không Có trong mảng này |
| $lt           | Bé hơn                                     |
| $lte          | Bé hơn bằng                                |
| $gt           | Lớn hơn                                    |
| $gte          | Lớn hơn hoặc bằng                          |
| $between      | Kết quả nằm giữa 2 phần tử trong mảng      |
| $notcontains  | không chứa                                 |
| $notcontainsi | không chưa (Không phân biệt hoa thường)    |
| $contains     | chứa                                       |
| $containsi    | chứa (Không phân biệt hoa thường)          |
| $startswith   | phần đầu khớp với                          |
| $endswith     | phần cuối khớp với                         |

Vài VD:

```
GET /api/v1/user?filter[gender][$in][0]=1&filter[gender][$in][1]=2
```

```
GET /api/v1/user?filter[gender][$between][0]=1&filter[gender][$between][1]=2
```

```
GET /api/v1/user?filter[firstName][$contains]=abc
```

Phương thúc $and và $or:

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
GET /api/users/filter[$or][0][$and][0][claims][claimValue][$eq]=admin&filter[$or][1][lastName][$eq]=Tran
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

Các bạn có thể tìm hiểu thêm ỏ một số link sau đây

[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#filtering)\
[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#complex-filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#complex-filtering)\
[https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#deep-filtering](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication#deep-filtering)

Mình thiết kế input đầu vào dựa trên [Strapi filter](https://docs.strapi.io/dev-docs/api/rest/filters-locale-publication)

Mình đã nhúng sẳn filter tự động vào tất cả các hàm lấy danh sách chỉ cần gọi

```csharp
unitOfWork.DynamicReadOnlyRepository<User>()
```
<div id='pagination'/>

### Phân trang

Offset and cursor pagination được tích hợp sẳn trong template.

Để sử dựng offset pagination thêm dòng sau vào code

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

Để sử dụng cursor pagination thêm dòng sau vào code

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

# Công nghệ

- .NET 8
- EntityFramework core 8
- AutoMapper
- FluentValidation
- Mediator
- XUnit, FluentAssertion, Respawn
- OpenTelemetry
- PostgresSQL
- Redis
- ElasticSearch
- Serilog
- Docker
- GitHub Workflow

# Hỗ trợ

Nếu như có bất kì vấn đề nào thì cho mình biết qua [phần issue ](https://github.com/minhsangdotcom/clean-architecture/issues) nhé.

# Lời cảm ơn

:heart: Cảm ơn mọi người rất nhiều :heart: :pray:.

- [Clean architecture by Jayson Taylor](https://github.com/jasontaylordev/CleanArchitecture)

- [Clean architecture by amantinband](https://github.com/amantinband/clean-architecture)
- [Clean architecture by Ardalis](https://github.com/ardalis/CleanArchitecture)
- [Specification pattern](https://github.com/ardalis/Specification)
- [REPR Pattern](https://github.com/ardalis/ApiEndpoints)
- [Clean testing by Jayson Taylor](https://github.com/jasontaylordev/CleanArchitecture/tree/main/tests)
<div id="license"/>

# Cấp phép

Dự án này sử dụng [MIT license](LICENSE)
