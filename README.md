# PictureDirectory

A small Windows desktop utility that sorts a folder full of photos and videos into
date-named subfolders — useful for cleaning up a camera roll or WhatsApp media dump
that's been exported as one big pile of files.

Point it at a folder and it moves every photo/video directly inside into a `yyyy-MM-dd`
subfolder, based on the earliest date it can determine for that file:

- File creation / last-write / last-access timestamps
- EXIF `DateTimeOriginal` metadata (for photos that have it)
- A date embedded in the filename, for WhatsApp-style names (`IMG-20230115-...`,
  `VID_20230115-...`, `VID-20230115-...`)

The **earliest** of these wins, since any single signal can be wrong on its own — for
example, a photo copied between drives loses its original timestamps, but its EXIF data
or WhatsApp-style filename still reveals the real date.

Supported extensions: `.jpg`, `.jpeg`, `.png`, `.mp4`, `.mov`, `.3gp`. It only looks at
files directly inside the selected folder — it does **not** recurse into subdirectories,
and it never overwrites a file that already exists at the destination (the source file is
left in place instead).

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

Processing runs on a background thread, so the window stays responsive on large folders.

## Notes

- This is a personal utility, kept intentionally small — there's no test suite or CI.
- Files are **moved**, not copied. Consider backing up the folder first if you're trying
  it out on files you can't afford to lose.

## License

MIT — see [LICENSE](LICENSE).
