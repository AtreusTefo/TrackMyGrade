# AGENTS.md - TrackMyGrade Master Instructions

## Project Context
- **Name:** TrackMyGrade
- **Backend:** ASP.NET Framework 4.8 (C#) using OWIN Self-Host, EF6, and SQL Server LocalDB.
- **Frontend:** Angular 18 SPA.
- **Primary IDEs:** VS Code 2026, Visual Studio Community 2026.
- **Main Goal:** A full-stack system for Admins, Teachers, and Students to manage and view academic performance and assessments.

## AI Behavior Guidelines
- **No Emojis:** Do NOT use emojis in any documentation, comments, or commit messages. Keep text professional and plain-text based.
- **For Claude:** Focus on clean architecture, strict type safety, and the DTO/Service/Controller pattern.
- **For Gemini/GPT:** Be extremely concise. Avoid conversational filler.
- **General:** If logic is ambiguous, ask for clarification. Check the `docs/` folder before suggesting changes.

## Documentation Standards
- **Style:** Professional, technical, and objective. 
- **Format:** Use standard Markdown (headings, tables, lists).
- **Prohibition:** Strictly zero emojis allowed in `.md` files or code documentation.
- **Organization:** All documentation files MUST be created in their rightful folders under `docs/`:
  - `docs/architecture/` - System design, data flow, architectural patterns
  - `docs/implementation/` - Implementation guides, code summaries, completion reports
  - `docs/project/` - Project requirements, planning, deliverables, scope documentation
  - `docs/guides/` - Quick start guides, testing guides, how-to documentation
  - `docs/api-postman/` - API testing, Postman workflows, integration guides
  - `docs/error-fixes/` - Bug fixes, error resolutions, issue tracking
  - `docs/daily-reports/` - Daily progress reports and status updates
- **Never** leave documentation files in the `docs/` root directory.

## Coding Standards & Patterns
- **Backend (C#):**
  - Use **FluentValidation** for models.
  - Map entities to **DTOs** using AutoMapper.
  - Logic belongs in the **Service Layer**, not Controllers.
- **Frontend (Angular 18):**
  - Use **CanActivateFn** for route guards.
  - Component location: `StudentApp/src/app/components/`.
- **Naming:** PascalCase for C#; camelCase for JSON and TypeScript.

## Project Structure Reference
- **API Logic:** `TrackMyGradeAPI/Application/Services/`
- **API Controllers:** `TrackMyGradeAPI/Presentation/Controllers/`
- **Frontend Components:** `StudentApp/src/app/components/`
- **Documentation:** `docs/` (Refer to `DOCUMENTATION_INDEX.md`).

## Environment Commands
- **Build API:** `cd TrackMyGradeAPI && msbuild TrackMyGradeAPI.csproj`
- **Run API:** `cd TrackMyGradeAPI && .\bin\TrackMyGradeAPI.exe`
- **Build Angular:** `cd StudentApp && npm run build`
- **Run Angular:** `cd StudentApp && npm start`

## Critical Rules
1. **Headers:** Student endpoints REQUIRE `X-TeacherId` or `X-StudentToken` headers.
2. **Database:** EF6 `ApplicationDbContext.Initialize()` handles schema on startup.
3. **Logging:** Use **ELMAH** patterns for exception handling.