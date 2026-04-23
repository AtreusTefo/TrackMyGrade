# FIX_ERRORS — Error Log & Resolutions

> **Project:** TrackMyGrade  
> **Stack:** Angular 18 (frontend) · ASP.NET Web API / EF6 (backend) · SQL Server  
> **Last updated:** 2026-03-13

This document records every error encountered during development, its root cause, and how it was resolved. Entries are ordered chronologically.

---

## Table of Contents

1. [TS2792 — Cannot find module '@angular/router'](#1-ts2792--cannot-find-module-angularrouter)
2. [TS2792 (Cascade) — All imports failing including local relative paths](#2-ts2792-cascade--all-imports-failing-including-local-relative-paths)

---

## 1. TS2792 — Cannot find module '@angular/router'

**Date:** 2026-03-13  
**File affected:** `StudentApp/src/app/components/student-detail/student-detail.component.ts`

### Error Message

```
TS2792: Cannot find module '@angular/router'. Did you mean to set the
'moduleResolution' option to 'nodenext', or to add aliases to the 'paths' option?
```

### Root Cause

`tsconfig.json` had `"moduleResolution": "node"`. Angular 18 packages use the `exports`
field in their `package.json` instead of a `main` entry point. The legacy `"node"`
resolution strategy does not read the `exports` map, so TypeScript could not locate
the type declarations inside `@angular/router` (and other Angular packages).

```json
// BEFORE — incompatible with Angular 18
"moduleResolution": "node"
```

### Fix

Changed `moduleResolution` to `"bundler"` in `StudentApp/tsconfig.json`. The
`"bundler"` strategy (introduced in TypeScript 5.0) mirrors how modern bundlers such
as esbuild (used by Angular CLI 18) resolve modules: it reads the `exports` field and
correctly resolves all Angular package entry points.

```json
// AFTER — correct for Angular 18 + TypeScript 5.5
"moduleResolution": "bundler"
```

**Verification:** Running the workspace TypeScript compiler directly produced zero errors:

```powershell
node .\node_modules\typescript\bin\tsc --noEmit -p .\tsconfig.json
# (no output = no errors)
```

---

## 2. TS2792 (Cascade) — All imports failing including local relative paths

**Date:** 2026-03-13  
**File affected:** `StudentApp/src/app/components/student-detail/student-detail.component.ts`

### Error Messages

```
TS2792: Cannot find module '@angular/router'.      Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '@angular/core'.        Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '@angular/common'.      Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '../../services/student.service'. Did you mean to set 'moduleResolution'...
TS2792: Cannot find module '../../services/error.util'.      Did you mean to set 'moduleResolution'...
TS2792: Cannot find module '../../models'.                   Did you mean to set 'moduleResolution'...
```

### Root Cause

After applying the `"bundler"` fix from error #1, the same errors persisted in Visual
Studio's Error List — and now even local relative imports (`../../services/...`,
`../../models`) were reported as unresolvable. Relative imports are unaffected by
`moduleResolution`, so their failure was the key diagnostic clue.

**Visual Studio ships its own TypeScript language service** (separate from the
TypeScript version installed in `node_modules`). When the VS-bundled TypeScript is
older than or out of sync with the workspace TypeScript, it may not recognise newer
`moduleResolution` values such as `"bundler"` and falls back to broken behaviour,
generating false-positive errors across all imports.

Confirmed by comparing results:

| Compiler | Command | Result |
|---|---|---|
| VS built-in language service | (IntelliSense) | ❌ 6 TS2792 errors |
| Workspace TypeScript 5.5.4 | `tsc --noEmit` | ✅ 0 errors |

### Fix

The `tsconfig.json` is correct. The errors are **Visual Studio IntelliSense false
positives** caused by a stale language-service cache. Two remedies:

#### Option A — Clear the Visual Studio cache (recommended first step)

1. **Close Visual Studio completely.**
2. Delete the `.vs` folder at the solution root:

```powershell
Remove-Item -Recurse -Force "C:\<path-to-solution>\.vs"
```

3. Reopen the solution. VS rebuilds its IntelliSense index using the current `tsconfig.json`.

#### Option B — Configure VS to use the workspace TypeScript version

1. In Visual Studio go to **Tools → Options → TypeScript → General**.
2. Set **TypeScript version** to **"Use TypeScript installed in workspace"**.
3. Restart Visual Studio.

This tells the VS language service to use the TypeScript from `node_modules`
(5.5.4 in this project) instead of its own bundled copy, eliminating the version
mismatch permanently.

### Notes

- `@angular/router` and all other Angular 18 packages are correctly listed in
  `package.json` under `dependencies` and are present in `node_modules`.
- No `npm install` changes were required.
- The actual Angular CLI build (`ng build` / `ng serve`) was never broken; only
  the VS IntelliSense was affected.

---

*End of FIX_ERRORS.md*
