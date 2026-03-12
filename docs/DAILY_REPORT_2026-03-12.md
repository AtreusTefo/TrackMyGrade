# Daily Development Report — 2026-03-12

**Project:** TrackMyGrade  
**Repository:** https://github.com/AtreusTefo/TrackMyGrade  
**Branch:** main

---

## ✅ What I Did Today

Worked across both the **ASP.NET Web API backend** (`TrackMyGradeAPI`) and the **Angular 18 frontend** (`StudentApp`), focusing on two major integration areas: AutoMapper and DataTables.

---

## 📦 What Was Completed

### 1. AutoMapper Integration — Backend (`TrackMyGradeAPI`)

Improved how AutoMapper is configured, initialised, and consumed throughout the API.

| File | Change |
|------|--------|
| `TrackMyGradeAPI.csproj` | Added `<RuntimeIdentifiers>win</RuntimeIdentifiers>` to unblock MSBuild |
| `Mapping/AutoMapperConfig.cs` | Moved double-checked lock inside `Initialize()` for true thread safety; switched to `cfg.AddMaps()` for profile auto-discovery; added `config.CompileMappings()` to pre-compile all mapping expressions at startup |
| `Mapping/MappingProfile.cs` | Added explicit `.ForSourceMember(src => src.Password, opt => opt.DoNotValidate())` on the `Teacher → TeacherResponseDto` map to prevent accidental password exposure |
| `Infrastructure/SimpleDependencyResolver.cs` | Added `using AutoMapper`; promoted `IMapper` from a per-call local variable to a `readonly` field on `SimpleDependencyScope`, resolved once per request scope |

**Before vs After — `AutoMapperConfig.Initialize()`:**
```csharp
// Before — lock only in property getter, profiles hardcoded, no pre-compilation
public static void Initialize()
{
    var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
    config.AssertConfigurationIsValid();
    _mapper = config.CreateMapper();
}

// After — self-contained lock, auto-discovery, pre-compiled expressions
public static void Initialize()
{
    if (_mapper != null) return;
    lock (_lock)
    {
        if (_mapper != null) return;
        var config = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(MappingProfile).Assembly));
        config.AssertConfigurationIsValid();
        config.CompileMappings();
        _mapper = config.CreateMapper();
    }
}
```

---

### 2. DataTables Integration — Frontend (`StudentApp`)

Improved how DataTables is typed, initialised, and configured in the student list component.

| File | Change |
|------|--------|
| `student-list.component.ts` | Imported `Api` type alongside `DataTable`; replaced `any` + ESLint suppression with `Api<any> \| null`; injected `ChangeDetectorRef`; replaced `setTimeout` with `cdr.detectChanges()`; changed `targets: 3` to `targets: -1`; added `lengthMenu: [5, 10, 25, 50]` |
| `student-list.component.css` | Added `text-align: left` to `.students-table td` to override DataTables' built-in right-alignment for numeric columns |

**Before vs After — DataTable initialisation:**
```typescript
// Before — any type, ESLint suppression, fragile setTimeout, magic column index
// eslint-disable-next-line @typescript-eslint/no-explicit-any
private dtInstance: any = null;

next: (data) => {
  this.students = data;
  this.isLoading = false;
  setTimeout(() => this.initDataTable());   // relies on implicit zone.js timing
}

columnDefs: [{ orderable: false, searchable: false, targets: 3 }]  // breaks if columns shift

// After — proper type, explicit DOM flush, maintainable column ref, user-controlled page size
private dtInstance: Api<any> | null = null;

next: (data) => {
  this.students = data;
  this.isLoading = false;
  this.cdr.detectChanges();   // guarantees *ngIf DOM is flushed before DataTables runs
  this.initDataTable();
}

columnDefs: [{ orderable: false, searchable: false, targets: -1 }],  // always = last column
lengthMenu: [5, 10, 25, 50]
```

---

## 🧱 Challenges Faced and How They Were Resolved

### Challenge 1 — MSBuild failing with `RuntimeIdentifiers` error

**Error:**
```
error : Your project file doesn't list 'win' as a "RuntimeIdentifier".
```

**Cause:** The ELMAH NuGet package ships platform-specific assets inside a `runtimes/` folder. MSBuild requires the project to declare which runtime it targets before NuGet can resolve those assets.

**Resolution:** Added `<RuntimeIdentifiers>win</RuntimeIdentifiers>` to the main `PropertyGroup` in `TrackMyGradeAPI.csproj` and re-ran `dotnet restore`. Build went from 127 errors to zero.

---

### Challenge 2 — `AutoMapperConfig.Initialize()` was not thread-safe when called directly

**Cause:** The double-checked locking pattern only existed inside the `Mapper` property getter. Calling `Initialize()` directly from `Startup.cs` bypassed the lock entirely, meaning two threads could theoretically both enter `Initialize()` at once.

**Resolution:** Moved the `if (_mapper != null) return` early-exit check and the `lock (_lock) { ... }` block into `Initialize()` itself, making the method self-defending regardless of the call site.

---

### Challenge 3 — `Password` field could leak silently in future

**Cause:** `CreateMap<Teacher, TeacherResponseDto>()` had no explicit instruction about the `Password` source property. AutoMapper skipped it correctly *today* because `TeacherResponseDto` has no `Password` field — but if someone added one in the future, AutoMapper would silently start mapping it.

**Resolution:** Added `.ForSourceMember(src => src.Password, opt => opt.DoNotValidate())` to document the intent explicitly and prevent any future accidental exposure.

---

### Challenge 4 — DataTables right-aligned the ID column

**Cause:** DataTables v2 automatically detects column data types. The ID column contains integers, so DataTables tagged those cells with `dt-type-numeric` and applied its built-in `text-align: right` rule. The component's `.students-table td` rule had no `text-align` set, so the DataTables global stylesheet won unchallenged.

**Resolution:** Added `text-align: left` to `.students-table td` in the component CSS. Angular's scoped attribute selectors give component rules higher specificity than third-party global rules, overriding DataTables cleanly without `!important`.

---

### Challenge 5 — `setTimeout` for DataTable initialisation was fragile

**Cause:** After setting `isLoading = false`, the table element (`#studentsTable`) is added to the DOM by Angular's `*ngIf`. The old code used `setTimeout(() => this.initDataTable())` to defer execution, relying on zone.js to flush Angular's change detection before the timer fired. This is implicit and timing-dependent.

**Resolution:** Injected `ChangeDetectorRef` and replaced the `setTimeout` with `this.cdr.detectChanges()` followed by an immediate `this.initDataTable()` call. `detectChanges()` synchronously forces Angular to evaluate the `*ngIf` binding and add the table to the DOM, making the contract explicit and order-guaranteed.

---

*Report generated: 2026-03-12*
