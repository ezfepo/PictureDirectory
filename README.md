# PictureDirectory

A small Windows utility that sorts photos and videos into date-named folders.

Point it at a folder and it moves every photo/video directly inside into a `yyyy-MM-dd`
subfolder, based on the earliest date it can determine for that file:

- EXIF `DateTimeOriginal` metadata (for photos that have it)
- A date embedded in the filename, for WhatsApp-style names (`IMG-20230115-...`,
  `VID_20230115-...`, `VID-20230115-...`)
- File creation / last-write / last-access timestamps

The earliest of these wins, so a photo that still has valid EXIF data will be sorted
correctly even if its file timestamps were reset (e.g. after being copied between drives).

Supported extensions: `.jpg`, `.jpeg`, `.png`, `.mp4`, `.mov`, `.3gp`. It only looks at
files directly inside the selected folder — it does not recurse into subdirectories, and
it never overwrites a file that already exists at the destination.

## Requirements

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or later) to build and run

## Build & run

```sh
dotnet build PictureDirectory.slnx
dotnet run --project PictureDirectory/PictureDirectory.csproj
```

Or open `PictureDirectory.slnx` in Visual Studio 2022+ and press F5.

## Usage

1. Launch the app.
2. Click **Select** and choose the folder containing your photos/videos.
3. Click **Process**. Matching files are moved into `yyyy-MM-dd` subfolders of that same
   folder.

## License

See [LICENSE](LICENSE).
