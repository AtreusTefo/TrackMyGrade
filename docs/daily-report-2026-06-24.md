# Daily Development Report
**Date:** 2026-06-24  
**Developer:** Developer.03  
**Project:** EA991 — Tickets System (`C:\Development\EA991\src\Tickets\`)

---

## What I Did Today

Investigated and resolved a PDF generation failure in the Tickets system. The feature in question — downloading a PDF report from a ticket view — was throwing an unhandled exception that prevented any PDF from being generated. The work involved debugging the Syncfusion HTML-to-PDF (Blink/Chrome-based) converter pipeline, identifying multiple layered root causes, and applying targeted fixes to both the C# handler code and the deployed binary assets.

---

## What Was Completed

### 1. Fixed `Syncfusion.Pdf.PdfException: Failed to launch Base!`
- Identified the exception occurring in `OnGetDownloadReport` and `OnGetDownloadRSAReport` inside `Areas\Operations\Pages\Tickets\ViewTicket.cshtml.cs`.
- The Syncfusion Blink converter uses an embedded Chromium (`chrome.exe`) process to render HTML to PDF. Chrome was failing to start due to missing process sandbox configuration.
- **Fix:** Added the following two command-line arguments to `BlinkConverterSettings` in both handler methods, before assigning the settings to the converter:
  ```csharp
  blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
  blinkConverterSettings.CommandLineArguments.Add("--disable-dev-shm-usage");
  ```
- This change was applied to both `OnGetDownloadReport` and `OnGetDownloadRSAReport` to keep them consistent.

### 2. Resolved mismatched BlinkBinaries resource files
- After applying the sandbox fix, the exception persisted. Deeper investigation revealed that the resource files inside `wwwroot\BlinkBinaries\` were from a **different (older) Chrome build** than the `chrome.exe` present in the same folder.
- The authoritative correct files were identified inside the Syncfusion 33.2.12 NuGet package at:  
  `~\.nuget\packages\syncfusion.htmltopdfconverter.net.windows\33.2.12\runtimes\win-x64\native\`
- All mismatched files were replaced by copying from the NuGet package, and a stale file (`snapshot_blob.bin`) that no longer ships with the package was deleted.
- `chrome.exe` was verified to launch successfully (exit code `0`) from the terminal after the fix.

### 3. Verified build integrity
- After all code changes, a full workspace build was confirmed successful with zero compilation errors.

---

## Challenges Faced & How They Were Resolved

### Challenge 1 — Exception persisted after Hot Reload
**Problem:** After adding `--no-sandbox` and `--disable-dev-shm-usage` to the code and hot-reloading the running application, the `Failed to launch Base!` exception continued to occur.  
**Resolution:** Recognised that Hot Reload only patches compiled method bodies — it does not affect static files on disk. The BlinkBinaries folder was a separate, disk-level problem that required a full debug session restart after the file replacements were complete.

### Challenge 2 — Identifying mismatched resource files
**Problem:** The error message `ProcessException: Failed to launch Base!` from `BlinkResult()` gives no indication of *why* Chrome is crashing — it is a generic launcher failure. The sandbox flags were the documented first step, but the crash persisted.  
**Resolution:** Ran `chrome.exe` directly from PowerShell with headless flags and confirmed it was exiting with code `0x80000003` (a hard crash / access violation before even reaching the DevTools handshake). Compared every file's byte size between `wwwroot\BlinkBinaries\` and the NuGet `win-x64\native\` folder, which revealed 5 mismatched files and 1 extra stale file. Chrome requires all its `.pak` and `.dat` resource files to exactly match the version of its executable.

### Challenge 3 — Second `replace_string_in_file` call caused structural damage
**Problem:** When applying the `CommandLineArguments` fix to the second handler (`OnGetDownloadRSAReport`), the search pattern was too broad and accidentally matched a region that included the `OnPostLoadAsync` method signature, stripping its parameter list.  
**Resolution:** The build output immediately surfaced cascade compiler errors (CS0548, CS1014, CS1022, etc.) pointing to line 504. The file was inspected and the missing `([FromBody] SyncFusionJsonObject requestData)` parameter was restored with a targeted replacement. A subsequent build confirmed zero errors.

---

## Files Changed

| File | Change |
|---|---|
| `Areas\Operations\Pages\Tickets\ViewTicket.cshtml.cs` | Added `--no-sandbox` and `--disable-dev-shm-usage` to `BlinkConverterSettings.CommandLineArguments` in `OnGetDownloadReport` and `OnGetDownloadRSAReport` |
| `wwwroot\BlinkBinaries\chrome_100_percent.pak` | Replaced with correct version from NuGet package |
| `wwwroot\BlinkBinaries\chrome_200_percent.pak` | Replaced with correct version from NuGet package |
| `wwwroot\BlinkBinaries\icudtl.dat` | Replaced with correct version from NuGet package |
| `wwwroot\BlinkBinaries\resources.pak` | Replaced with correct version from NuGet package |
| `wwwroot\BlinkBinaries\locales\en-US.pak` | Replaced with correct version from NuGet package |
| `wwwroot\BlinkBinaries\snapshot_blob.bin` | Deleted (stale — no longer shipped by Syncfusion 33.2.12) |

---

## Notes for Future Reference

- Whenever the `Syncfusion.HtmlToPdfConverter.Net.Windows` NuGet package is upgraded, the contents of `wwwroot\BlinkBinaries\` **must** be re-synced from the new package's `runtimes\win-x64\native\` folder. The resource files are version-locked to `chrome.exe` and any mismatch will cause a silent Chrome crash.
- The `--no-sandbox` and `--disable-dev-shm-usage` flags are required for all non-IIS hosted environments (Visual Studio dev server, self-hosted Kestrel) and should remain in place permanently.
