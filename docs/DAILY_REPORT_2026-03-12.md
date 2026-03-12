# Daily Report — 2026-03-12

**Project:** TrackMyGrade  
**Developer:** AtreusTefo  
**Date:** 12 March 2026

---

## ✅ What I Did Today

### 1. Project Setup & Build
- Confirmed both ports 5000 (API) and 4200 (Angular) were free before starting
- Located the MSBuild executable bundled with Visual Studio 18 Community
- Built the ASP.NET Framework backend using MSBuild in Debug configuration with NuGet package restore
- Attempted to install Angular frontend dependencies via `npm install` in the `StudentApp/` directory

### 2. Delete Modal Improvement (Angular)
- Replaced the native browser `confirm()` dialog in `StudentListComponent` with a fully custom Angular modal
- Implemented three new component methods:
  - `openDeleteModal()` — stores the target student and shows the modal
  - `closeDeleteModal()` — hides the modal and clears state
  - `confirmDelete()` — calls the delete service, tracks in-flight state, and closes on completion
- Added three new state fields: `showDeleteModal`, `studentToDelete`, `isDeleting`
- Updated the delete button in the template to call `openDeleteModal()` instead of `deleteStudent()`
- Added the modal markup to the template with:
  - Semi-transparent backdrop that closes on outside click
  - Student name displayed in the confirmation message
  - "This action cannot be undone." warning text
  - Cancel and Delete buttons both disabled while deletion is in progress
  - Delete button shows "Deleting..." during the API call
  - Proper accessibility attributes (`role="dialog"`, `aria-modal`, `aria-labelledby`)
- Added all supporting CSS styles:
  - Backdrop overlay with 50% opacity
  - Modal box with shadow, border-radius, and `fadeInModal` slide-in animation
  - Modal header, body, footer sections with borders
  - `btn-secondary` (grey) style for the Cancel button
  - `btn:disabled` opacity rule
  - Mobile-responsive layout — footer buttons stack vertically on screens ≤ 768px

### 3. Verified Frontend Build
- Ran `ng build --configuration development` to confirm no TypeScript or template errors
- Build completed successfully — all bundles generated cleanly

### 4. Git Setup & Push to GitHub
- Initialised a new local Git repository (`git init -b main`)
- Configured remote to `https://github.com/AtreusTefo/TrackMyGrade.git`
- Staged all source files — `.gitignore` automatically excluded build artifacts
- Committed all project files in a single commit
- Force-pushed to `origin/main` (commit SHA: `de6152e`)

---

## 📦 What Was Completed

| Area | Item | Status |
|---|---|---|
| Backend | MSBuild compile — zero errors | ✅ Done |
| Frontend | Delete modal replaced with Angular component | ✅ Done |
| Frontend | Angular dev build — zero errors | ✅ Done |
| Git | Local repo initialised on `main` branch | ✅ Done |
| Git | All source files committed (75 files) | ✅ Done |
| GitHub | Force-pushed to `AtreusTefo/TrackMyGrade` main | ✅ Done |

### Files Changed in the Delete Modal Improvement
| File | Change |
|---|---|
| `student-list.component.ts` | Added modal state fields and three modal methods; removed `confirm()` |
| `student-list.component.html` | Wired delete button to `openDeleteModal()`; added modal markup |
| `student-list.component.css` | Added modal styles, `btn-secondary`, `btn:disabled`, `fadeInModal` animation |

---

## ⚠️ Challenges Faced

### 1. `npm install` Interrupted
- **Problem:** `npm install` for the Angular frontend was cancelled mid-run, leaving a partially extracted `node_modules/` directory with EPERM (permission denied) cleanup warnings.
- **Impact:** The Angular dev server was not started during this session.
- **Resolution:** The build was verified using `npx ng build` which completed successfully. A clean `npm install` run is needed before using `ng serve`.

### 2. Git Repository Not Initialised
- **Problem:** The project folder had no `.git` directory — it had never been committed to version control locally, despite having a `.gitignore` already in place.
- **Resolution:** Initialised a fresh repo with `git init -b main` and force-pushed to the existing GitHub remote.

### 3. PowerShell stderr Reporting `git push` as Failed
- **Problem:** PowerShell treated `git push` output on stderr as a command failure even though the push was successful (git writes progress info to stderr by default).
- **Resolution:** Confirmed success by reading the actual output line `+ 4825d5d...de6152e main -> main (forced update)` and verifying with `git log --oneline` and `git status`.

---

## 📝 Notes & Next Steps

- Run a clean `npm install` then `npm start` to bring up the Angular dev server on `http://localhost:4200`
- Start the API with `.\TrackMyGradeAPI\start-api.ps1` or press **F5** in Visual Studio
- Consider adding future commits incrementally rather than force-pushing to preserve history
- The `Models/Teacher.cs` entity is referenced in services but was not found as a standalone file — verify it is defined inside another file or add it explicitly

---

*Report generated: 2026-03-12*
