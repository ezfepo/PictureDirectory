# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this is

PictureDirectory is a small Windows desktop utility (WinForms, .NET 8) with a single
window: pick a folder, click Process, and every photo/video directly inside that folder
gets moved into a `yyyy-MM-dd` subfolder based on the earliest date it can determine for
that file (EXIF metadata, WhatsApp-style filename, or file timestamps — whichever is
earliest). It does not recurse into subdirectories.

## Structure

- `PictureDirectory.slnx` — solution file (modern XML-based `.slnx` format, not `.sln`).
- `PictureDirectory/PictureDirectory.csproj` — SDK-style project, `net8.0-windows`, WinForms.
- `PictureDirectory/Program.cs` — entry point.
- `PictureDirectory/frmMain.cs` / `frmMain.Designer.cs` / `frmMain.resx` — the single form
  and all application logic (file scanning, date resolution, moving files).
- `PictureDirectory/app.ico` — application icon (also used as the .exe icon via
  `ApplicationIcon` in the csproj).

There are no other projects, no tests, and no CI configuration.

## Build & run

```sh
dotnet build PictureDirectory.slnx
dotnet run --project PictureDirectory/PictureDirectory.csproj
```

Requires the .NET 8 SDK (or later) with Windows Desktop workload, since this is a WinForms
app and only runs on Windows.

## Date resolution logic (frmMain.cs)

For each supported file (`.3gp .mp4 .mov .png .jpg .jpeg`) directly in the selected folder,
the target date is the **earliest** of:
1. File creation / last-access / last-write timestamps.
2. EXIF `DateTimeOriginal`, if the file has readable EXIF metadata (via `MetadataExtractor`).
3. A date parsed from the filename if it starts with `IMG-`, `VID_`, or `VID-` (WhatsApp
   media naming convention), e.g. `IMG-20230115-WA0001.jpg` → 2023-01-15.

Files are moved into `<selected folder>/<yyyy-MM-dd>/<original filename>`. If a file with
the same name already exists at the destination, the source file is left in place rather
than overwritten.

Keep this "earliest of all signals" behavior when touching this logic — it's intentional,
not an oversight, since any of the three signals can be wrong in isolation (e.g. a file
copied between drives loses its original creation time, but EXIF or the WhatsApp filename
survives).

## Conventions

- Nullable reference types and implicit usings are enabled — write idiomatic modern C#
  (file-scoped namespaces, target-typed `new`, collection expressions) rather than the
  older verbose style still visible in `frmMain.Designer.cs` (designer-generated, don't
  hand-edit its layout code beyond what's needed).
- Long-running work (`btnProcess_Click`) runs off the UI thread via `Task.Run` — preserve
  that pattern for anything that touches the filesystem so the form doesn't freeze.
- No automated tests exist. If you add non-trivial logic (especially to date resolution),
  consider whether it's worth extracting into a testable, non-UI class first.
