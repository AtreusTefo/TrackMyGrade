# **EA991 Ticketing Solution - Developer Report**

## **Executive Summary**

The **EA991 Ticketing Solution** is a multi-project .NET 6 web-based application designed for comprehensive ticket management across multiple ticket types (EMS, Roadside, COVID, and IHT). The solution follows a layered architecture with clear separation of concerns, utilizing Razor Pages for the UI, Entity Framework Core for data access, and robust business logic implementation.

---

## **1. Solution Architecture Overview**

### **1.1 Project Structure**
The solution consists of 7 main projects:

```
C:\Development\EA991\src\
├── Tickets (Main Web Application - .NET 6)
├── Tickets.Data (Data Access Layer - .NET 6)
├── Tickets.Common (Shared Utilities - .NET 6)
├── Tickets.Business (Business Logic Layer - .NET 6)
├── Tickets.Interfaces (Contract Definitions - .NET 6)
├── Tickets.Console (Command-Line Utility - .NET 6)
├── Tickets.Test (Unit & Integration Tests - .NET 6)
└── Tickets.RunnerChecker (Background Job Monitor - .NET 6)
```

### **1.2 Layered Architecture Pattern**

```
┌─────────────────────────────────────┐
│   Presentation Layer                │
│   (Tickets - Razor Pages Web App)   │
│   - Areas: Operations, Admin,       │
│     Reporting, Basenet              │
└──────────────┬──────────────────────┘
			   │
┌──────────────┴──────────────────────┐
│   Business Logic Layer              │
│   (Tickets.Business)                │
│   - Services & Validation Rules     │
└──────────────┬──────────────────────┘
			   │
┌──────────────┴──────────────────────┐
│   Data Access Layer                 │
│   (Tickets.Data)                    │
│   - Entity Framework Core           │
│   - Repositories & DbContext        │
└──────────────┬──────────────────────┘
			   │
┌──────────────┴──────────────────────┐
│   Supporting Layers                 │
│   - Tickets.Interfaces (Contracts)  │
│   - Tickets.Common (Utilities)      │
└─────────────────────────────────────┘
```

---

## **2. Core Technologies & Dependencies**

### **2.1 Framework & Runtime**
- **Target Framework**: .NET 6.0
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **Garbage Collection**: Server GC + Concurrent GC enabled

### **2.2 Key NuGet Packages**

| Package | Version | Purpose |
|---------|---------|---------|
| **AutoMapper.Extensions.Microsoft.DependencyInjection** | 12.0.1 | Object mapping/DTOs |
| **FluentValidation.AspNetCore** | 11.3.1 | Business validation |
| **Microsoft.EntityFrameworkCore.SqlServer** | 6.0.1 | Database ORM |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 6.0.1 | Authentication/Authorization |
| **Serilog.AspNetCore** | 9.0.0 | Structured logging |
| **Syncfusion.EJ2.AspNet.Core** | 30.2.4 | UI Components (licensed) |
| **Scriban** | 6.2.1 | Template engine |
| **FormHelper** | 5.0.0 | Form handling utilities |
| **jQuery-Validation.NuGet** | 1.15.1 | Client-side validation |

### **2.3 Frontend Technologies**
- **Bootstrap 5**: CSS Framework
- **jQuery**: DOM manipulation
- **jQuery Validation**: Client-side form validation
- **Syncfusion Components**: Advanced UI widgets (charts, grids, PDF viewer)
- **Select2**: Enhanced select dropdowns
- **Font Awesome**: Icon library

---

## **3. Database Design**

### **3.1 Database Connection**
- **Engine**: SQL Server
- **Host**: `BASENET-PRIME\MSSQLSERVER19`
- **Database**: `EA911Tickets`
- **Connection Pooling**: Enabled

### **3.2 Core Entities (DbSets)**

#### **Ticket Management**
- `Tickets` - Base ticket entity
- `EMSTicket` - Emergency Medical Services tickets (DTO/view-based)
- `RSATicket` - Roadside Assistance tickets (DTO/view-based)
- `TicketTracking` - Audit trail for ticket lifecycle
- `TicketTrackingRoadSide` - Roadside-specific tracking

#### **Contacts & Organizations**
- `Contacts` - Contact records with GUID primary key
- `ContactDetails` - Extended contact information
- `Employees` - Employee records
- `PolicyOrganizations` - Organization policy mappings
- `OrganizationDefaults` - Organization configuration

#### **Medical & Related**
- `Medicals` - Medical records
- `MedicalInsurances` - Insurance information
- `Categories` & `SubCategories` - Ticket classification
- `ContactInteractions` - Contact communication history

#### **Vehicle & Infrastructure**
- `Vehicles` - Vehicle information
- `TicketRoadSideCall` - Roadside service calls

#### **Reporting & DTOs**
- `OpenTicket` - Query-based view (no key)
- `SingleRSATicketDTO` - RSA ticket DTO (no key)

### **3.3 Entity Relationships**

The `Contact` entity demonstrates the relational model:
```csharp
public class Contact
{
	public Guid ContactId { get; set; }
	public string NationalIdentity { get; set; }
	public string Firstname { get; set; }
	public string Lastname { get; set; }
	public string Othername { get; set; }
	public Guid Employer { get; set; }
	public DateTime? Birthday { get; set; }
	public string Gender { get; set; }
	public bool? HasDisability { get; set; }

	// Navigation properties
	public virtual ICollection<ContactDetail> ContactDetails { get; set; }
	public virtual ICollection<Medical> Medicals { get; set; }
}
```

The `Ticket` base entity includes comprehensive tracking:
- Customer demographics (name, contact, gender, DOB)
- Medical information (diagnosis, member numbers, facility details)
- Operational timestamps (call time, dispatch, arrival)
- Medical aid membership validation

---

## **4. Application Architecture**

### **4.1 Startup Configuration (Program.cs)**

```csharp
// Key configurations:
1. Serilog Logger Setup
   - Console output (ANSI colored theme)
   - File rolling logs (daily rotation, 7-day retention)
   - Path: "Logs/ea-tickets-.log"

2. Syncfusion License Registration
   - Version 30.2.4 licensed components

3. Service Registration
   - Handled via ServicesConfiguration.Configure()
   - DI container setup via builder.Services

4. Pipeline Configuration
   - Custom middleware: HTTP request logging
   - Authentication enabled
   - CORS configured ("AllowAll" policy)
   - Razor Pages routing
```

### **4.2 Dependency Injection Pattern**

The application uses constructor injection extensively. Example from `ViewTicketModel`:
```csharp
public ViewTicketModel(
	ITicketRepository ticketRepository,
	ILogger<IndexModel> logger,
	IConfiguration configuration,
	IWebHostEnvironment hostingEnvironment,
	IMapper mapper,
	IMedicalInsuranceRepository medicalInsuranceRepository,
	IVehicleRepository vehicleRepository,
	ITicketRoadSideCallRepository ticketRoadSideCallRepository,
	IOrganizationDefaultRepository organizationDefaultRepository,
	ITicketTrackingRepository ticketTrackingRepository,
	IApplicationDbContext applicationDbContext,
	ITicketTrackingRoadSideRepository ticketTrackingRoadSideRepository,
	IMemoryCache cache,
	IUrlHelperServices urlHelperServices,
	IHttpContextAccessor httpContextAccessor)
```

### **4.3 Object Mapping (AutoMapper)**

The application uses AutoMapper for DTO <-> Entity conversions:

**Mapping Profiles** (e.g., `TicketProfile.cs`):
- `Ticket` ↔ `TicketListDTO`
- `Ticket` ↔ `TicketReportDTO`
- `Ticket` ↔ `TicketCovidDTO`
- `Ticket` ↔ `TicketIHTDTO`
- `Ticket` ↔ `TicketPrimaryDTO`
- `TicketRoadSideCall` ↔ `TicketRoadSideCallDTO`
- `TicketTracking` ↔ `TicketTrackingDTO`
- `TicketTrackingRoadSide` ↔ `TicketTrackingRoadSideDTO`

---

## **5. Presentation Layer (Razor Pages)**

### **5.1 Areas Structure**

The application is organized into functional areas:

#### **Operations Area**
- **Purpose**: Core ticket CRUD operations
- **Pages**:
  - `Tickets/ViewTicket.cshtml` (ViewTicket page model - 667 lines)
  - Create, Edit, Delete operations
  - Ticket tracking and audit logs
- **Features**:
  - Advanced ticket viewing with PDF support
  - Ticket status tracking
  - Medical information management

#### **Administration Area**
- Configuration and system management
- User/role management
- System defaults

#### **Reporting Area**
- Dashboard and analytics
- Report generation
- Data visualization (Syncfusion charts)

#### **Basenet Area**
- Legacy integration (placeholder structure)

### **5.2 Shared Views & Partials**

- `_HeaderPartial.cshtml` - Navigation header
- Layout pages for consistent UI
- Validation error displays
- Form helpers for complex fields

### **5.3 Razor Page Model Pattern**

**ViewTicket.cshtml.cs** demonstrates the pattern:
```csharp
[Authorize]
[IgnoreAntiforgeryToken(Order = 1001)]
public class ViewTicketModel : PageModel
{
	// Handler methods: OnGet(), OnPost(), OnPostAsync()
	// Page properties: @Model.Ticket, @Model.ErrorMessage
	// TempData for cross-request messaging

	public void OnGet()
	{
		// Access HttpContext, User claims, cookies
		var user = _httpContextAccessor.HttpContext?.User;
		// ...
	}
}
```

---

## **6. Business Logic Layer**

### **6.1 Repository Pattern**

The business layer implements the Repository pattern for data abstraction:

**Example**: `ContactDetailsRepository.cs`
```csharp
public class ContactDetailsRepository : BaseRepository<ContactDetail>, 
	IContactDetailRepository
{
	public ContactDetailsRepository(IApplicationDbContext context) 
		: base(context) { }
}
```

### **6.2 Key Repositories**
- `ITicketRepository` - Ticket CRUD
- `IContactRepository` - Contact management
- `IContactDetailRepository` - Contact details
- `IMedicalInsuranceRepository` - Insurance data
- `IVehicleRepository` - Vehicle management
- `ITicketRoadSideCallRepository` - Roadside calls
- `IOrganizationDefaultRepository` - Org defaults
- `ITicketTrackingRepository` - Audit/tracking
- `ITicketTrackingRoadSideRepository` - Roadside tracking

### **6.3 Validation Framework**

**FluentValidation** is used for declarative validation:
```csharp
private readonly IValidator<Ticket> _validator;
// Used in business logic and page models
```

---

## **7. Data Access Layer**

### **7.1 Entity Framework Core Configuration**

**ApplicationDbContext** (145 lines):
- Extends `IdentityDbContext<IdentityUser>` for built-in authentication
- Implements `IApplicationDbContext` interface
- Configures 19 DbSets
- Handles stored procedure result mapping (EMSTicket, RSATicket)

**Database Facade Access**:
```csharp
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
	public virtual DbSet<Ticket> Tickets { get; set; }
	public virtual DbSet<Employee> Employees { get; set; }
	// ... 17 more DbSets

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder); // Important for Identity
		// Configure keyless entities for stored procedures
		// Configure enum conversions
	}
}
```

### **7.2 Stored Procedures & Views**

Two DTOs are configured as keyless entities for stored procedure results:
- `EMSTicket` - Emergency medical services data
- `RSATicket` - Roadside assistance data

These entities handle enum-to-int conversions for Status, Priority, and Severity.

### **7.3 Migrations**

Located in `Tickets.Data/Migrations/`, the migration system manages schema changes with Entity Framework Core tools.

### **7.4 Identity Integration**

- Uses `IdentityDbContext` for user/role management
- User Secrets ID: `aspnet-Tickets-FDEA5985-F71D-471A-BFAA-13DD3D455092`
- Authentication configured in startup

---

## **8. Supporting Infrastructure**

### **8.1 Logging (Serilog)**

**Configuration**:
```
Console Output: Colorized ANSI theme with timestamp, level, message
File Output: Daily rolling logs
Location: Logs/ea-tickets-.log
Retention: 7 days
Format: [HH:mm:ss] [LEVEL] Message {Exception}
```

### **8.2 Configuration Management**

**appsettings.json**:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "...",
	"EA991Connection": "...",
	"EA991ConnectionRemote": "...",
	"EA991ConnectionRemoteDev": "..."
  },
  "BasenetInfo": {
	"EmailAddress": "john@ivaluenet.com",
	"DefaultLogoFile": "defaultlogo.png"
  }
}
```

### **8.3 Utility Projects**

#### **Tickets.Common**
- Shared constants and enums
- Utility/helper classes
- Cross-project reusable code
- Logging helpers

#### **Tickets.Console**
- Command-line interface for administrative tasks
- Batch processing operations
- Data migration tools
- Integration with business/data layers

#### **Tickets.RunnerChecker**
- Background job monitoring
- Health check diagnostics
- System status reporting
- Logging for job execution

---

## **9. Testing**

### **9.1 Test Project Structure**

**Tickets.Test**:
- Unit tests for business logic
- Integration tests for workflows
- Data access testing
- Validation testing

### **9.2 Test Coverage Areas**
- Business logic rules validation
- Data repository operations
- Entity relationships
- Service layer functionality

---

## **10. Security & Authentication**

### **10.1 Authorization**
- **Pattern**: `[Authorize]` attribute on pages
- **Framework**: ASP.NET Core Identity with Entity Framework
- **Database**: Integrated Identity tables in EA911Tickets

### **10.2 CSRF Protection**
- Anti-forgery tokens configured
- `[IgnoreAntiforgeryToken]` used selectively (e.g., ViewTicket page)

### **10.3 CORS**
- Policy: "AllowAll" (configured in startup)
- Should be restricted in production

---

## **11. Performance Considerations**

### **11.1 Garbage Collection**
```xml
<ServerGarbageCollection>true</ServerGarbageCollection>
<ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
```
- Server GC for better throughput
- Concurrent GC for responsiveness

### **11.2 Caching**
- **IMemoryCache** used in ViewTicket model
- ASP.NET Core's in-memory cache provider

### **11.3 Connection Pooling**
- SQL Server connection pooling enabled
- Multiple connection string configurations for different environments

---

## **12. Deployment & Hosting**

### **12.1 Docker Support**
- `DockerDefaultTargetOS`: Windows
- Dockerfile context configured
- Container support in project file

### **12.2 Branding**
- Application Icon: `EA991 CRM Circle.ico`
- Package Icon: `EA991 CRM (3).png`
- Indicates EA991 CRM product branding

### **12.3 Static Content**
- **wwwroot** directory structure:
  - `css/site.css` - Global styles
  - `js/site.js` - Global scripts
  - `lib/` - Library dependencies (Bootstrap, jQuery, Syncfusion)
  - `templates/` - Email templates (HTML, CSS, logo)

---

## **13. File Organization**

### **13.1 Razor Pages Structure**
```
Areas/
├── Operations/Pages/Tickets/
│   ├── ViewTicket.cshtml
│   ├── ViewTicket.cshtml.cs
│   ├── Index.cshtml
│   └── [other CRUD pages]
├── Administration/Data/
├── Reporting/
└── Basenet/
```

### **13.2 Helper Functions**
```
Helpers/
├── MappingProfiles/
│   └── TicketProfile.cs (117 lines)
├── ReferenceData/
└── [other utilities]
```

### **13.3 Models & DTOs**
```
Models/
├── Reports/
├── MultiPatients/
└── [ViewModels]

Data/Entities/
├── DTOs/
│   ├── TicketListDTO
│   ├── TicketReportDTO
│   ├── TicketCovidDTO
│   └── [other DTOs]
├── Reports/
│   └── RSATicket
│   └── EMSTicket
└── CRM/
```

---

## **14. Key Workflows**

### **14.1 Ticket Lifecycle**

```
Create Ticket
	↓
[Validation via FluentValidation]
	↓
[AutoMapper: DTO → Entity]
	↓
[Save via ITicketRepository]
	↓
[Entity Framework Core → SQL Server]
	↓
[TicketTracking Record Created]
	↓
View/Edit Ticket
	↓
[Optional: Generate PDF Report]
	↓
Close/Archive
```

### **14.2 Razor Page Handler Flow**

```
HTTP Request
	↓
Routing to Area/Page
	↓
PageModel Constructor (DI)
	↓
OnGet() / OnPost() Handler
	↓
[Access Repositories, Services]
	↓
[Validation & Business Logic]
	↓
[Mapping & Data Persistence]
	↓
View Rendering / Redirect
	↓
HTTP Response
```

---

## **15. Configuration Files**

| File | Purpose |
|------|---------|
| `Tickets.csproj` | Project configuration, dependencies |
| `Program.cs` | Application startup, logging, DI setup |
| `appsettings.json` | Connection strings, app settings |
| `launchSettings.json` | Debug profiles, development settings |

---

## **16. Area-Specific Details**

### **Operations Area**
- **ViewTicket Page** (667 lines):
  - Uses extensive DI (12+ injected services)
  - Authorization required
  - Handles multiple ticket types
  - PDF viewing capability
  - HTTP context manipulation
  - Memory cache usage

### **Reporting Area**
- Dashboard and analytics
- Syncfusion EJ2 visualizations
- Data export capabilities

### **Administration Area**
- System configuration
- User management (if implemented)

---

## **17. Dependencies & Integration Points**

### **Project Dependencies**
```
Tickets (main)
├── Tickets.Data
├── Tickets.Business
├── Tickets.Interfaces
└── Tickets.Common

Tickets.Business
├── Tickets.Data
├── Tickets.Interfaces
└── Tickets.Common

Tickets.Data
├── Tickets.Interfaces
└── Tickets.Common
```

### **External Integrations**
- SQL Server database
- Syncfusion cloud/licensing
- Email services (configured)
- PDF conversion (Syncfusion)

---

## **18. Development Workflow**

### **Build**
```powershell
dotnet build
```

### **Run**
```powershell
dotnet run
# Access: https://localhost:5001
```

### **Database Migrations**
```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

### **Testing**
```powershell
dotnet test
```

---

## **19. Code Quality & Standards**

### **Best Practices Implemented**
1. **SOLID Principles**: Repository pattern, DI, interface-based design
2. **Separation of Concerns**: Layered architecture
3. **Code Reusability**: AutoMapper for DTOs, shared utilities
4. **Error Handling**: Serilog structured logging
5. **Validation**: FluentValidation framework
6. **Security**: ASP.NET Core Identity, authorization attributes
7. **Type Safety**: Nullable reference types enabled

### **Areas for Improvement**
1. **CORS Policy**: Currently "AllowAll" - should restrict in production
2. **Connection String Security**: Passwords in appsettings (use secrets in production)
3. **Async/Await**: Mostly synchronous - consider async patterns
4. **Error Handling**: Expand try-catch blocks for better exception handling
5. **Unit Tests**: Need more comprehensive test coverage

---

## **20. Summary Statistics**

| Metric | Value |
|--------|-------|
| **Target Framework** | .NET 6 |
| **Project Count** | 7 |
| **DbSet Entities** | 19 |
| **NuGet Dependencies** | 17+ |
| **Areas** | 4 (Operations, Admin, Reporting, Basenet) |
| **Authentication** | ASP.NET Core Identity |
| **ORM** | Entity Framework Core 6.0.1 |
| **UI Framework** | ASP.NET Razor Pages |
| **CSS Framework** | Bootstrap 5 |
| **JavaScript Libraries** | jQuery, Syncfusion EJ2 |
| **Logging** | Serilog |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |

---

## **Conclusion**

The **EA991 Ticketing Solution** is a well-architected, enterprise-ready .NET 6 application following clean code principles and industry best practices. The layered architecture ensures maintainability, the robust DI pattern promotes testability, and the comprehensive data model supports complex ticket management workflows across multiple ticket types (EMS, Roadside, COVID, IHT).

**Key Strengths:**
- Clear separation of concerns
- Comprehensive data model
- Strong authentication/authorization
- Extensive integration capabilities
- Modern .NET 6 stack

**Recommended Next Steps:**
1. Restrict CORS policy for production
2. Move connection strings to secure configuration
3. Increase async/await usage
4. Expand unit test coverage
5. Document API contracts if exposed
6. Implement comprehensive error handling

---

**Report Generated**: 2024
**Environment**: Visual Studio Community 2026 (18.6.2)
**Root Path**: C:\Development\EA991\src\Tickets\
