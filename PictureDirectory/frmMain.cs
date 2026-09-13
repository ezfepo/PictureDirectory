using System.Globalization;
using MetadataExtractor.Formats.Exif;
using MetadataDirectory = MetadataExtractor.Directory;

namespace PictureDirectory;

public partial class frmMain : Form
{
    private static readonly string[] SupportedExtensions =
    [
        ".3gp", ".mp4", ".mov", ".png", ".jpg", ".jpeg"
    ];

    public frmMain()
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }

    private void btnSelect_Click(object sender, EventArgs e)
    {
        if (fbdMain.ShowDialog() == DialogResult.OK)
        {
            txtDirectory.Text = fbdMain.SelectedPath;
        }
    }

    private async void btnProcess_Click(object sender, EventArgs e)
    {
        string directory = txtDirectory.Text;

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            MessageBox.Show(this, "Please select a valid directory first.", "PictureDirectory",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnProcess.Enabled = false;
        btnSelect.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            int processed = await Task.Run(() => OrganizeFiles(directory));
            MessageBox.Show(this, $"Processed {processed} file(s).", "PictureDirectory",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnProcess.Enabled = true;
            btnSelect.Enabled = true;
        }
    }

    private static int OrganizeFiles(string directory)
    {
        int processedCount = 0;

        foreach (string filePath in Directory.GetFiles(directory))
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!SupportedExtensions.Contains(extension))
            {
                continue;
            }

            try
            {
                if (MoveFileToDateDirectory(directory, filePath))
                {
                    processedCount++;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to process '{filePath}': {ex.Message}");
            }
        }

        return processedCount;
    }

    private static bool MoveFileToDateDirectory(string rootDirectory, string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        DateTime earliestDate = GetEarliestDate(filePath, fileName);

        string targetDirectory = Path.Combine(rootDirectory, earliestDate.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(targetDirectory);

        string destinationPath = Path.Combine(targetDirectory, fileName);
        if (File.Exists(destinationPath))
        {
            return false;
        }

        File.Move(filePath, destinationPath);
        return true;
    }

    private static DateTime GetEarliestDate(string filePath, string fileName)
    {
        DateTime earliest = DateTime.Today;

        earliest = Min(earliest, File.GetCreationTime(filePath));
        earliest = Min(earliest, File.GetLastAccessTime(filePath));
        earliest = Min(earliest, File.GetLastWriteTime(filePath));

        DateTime? exifDate = TryGetExifDate(filePath);
        if (exifDate.HasValue)
        {
            earliest = Min(earliest, exifDate.Value);
        }

        DateTime? whatsappDate = TryGetWhatsappDate(fileName);
        if (whatsappDate.HasValue)
        {
            earliest = Min(earliest, whatsappDate.Value);
        }

        return earliest;
    }

    private static DateTime? TryGetExifDate(string filePath)
    {
        try
        {
            IReadOnlyList<MetadataDirectory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);
            ExifSubIfdDirectory? exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            object? dateTimeValue = exifSubIfd?.GetObject(ExifDirectoryBase.TagDateTimeOriginal);

            if (dateTimeValue is null)
            {
                return null;
            }

            string dateText = dateTimeValue.ToString() ?? string.Empty;
            if (dateText.Length < 10)
            {
                return null;
            }

            if (DateTime.TryParseExact(dateText[..10], "yyyy:MM:dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsed))
            {
                return parsed;
            }
        }
        catch (Exception ex) when (ex is MetadataExtractor.ImageProcessingException or IOException)
        {
            // No readable metadata (e.g. video files, corrupt/unsupported images) - fall back to file timestamps.
        }

        return null;
    }

    private static DateTime? TryGetWhatsappDate(string fileName)
    {
        if (!fileName.StartsWith("IMG-", StringComparison.Ordinal) &&
            !fileName.StartsWith("VID_", StringComparison.Ordinal) &&
            !fileName.StartsWith("VID-", StringComparison.Ordinal))
        {
            return null;
        }

        if (fileName.Length < 12)
        {
            return null;
        }

        if (DateTime.TryParseExact(fileName.Substring(4, 8), "yyyyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
