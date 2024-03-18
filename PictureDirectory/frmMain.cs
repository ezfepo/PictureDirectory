using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PictureDirectory
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            DialogResult result = fbdMain.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtDirectory.Text = fbdMain.SelectedPath;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            string[] oFiles = Directory.GetFiles(txtDirectory.Text);

            foreach (string oFile in oFiles)
            {
                try
                {
                    string oFileName = System.IO.Path.GetFileName(oFile);
                    string oExtension = System.IO.Path.GetExtension(oFile).ToLower();

                    switch (oExtension)
                    {
                        case ".3gp":
                        case ".mp4":
                        case ".mov":
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                            //FileAttributes oFileAttributes = File.GetAttributes(oFile);

                            DateTime oMinDateTime = DateTime.Today;

                            oMinDateTime = new DateTime(Math.Min(File.GetCreationTime(oFile).Ticks, oMinDateTime.Ticks));
                            oMinDateTime = new DateTime(Math.Min(File.GetLastAccessTime(oFile).Ticks, oMinDateTime.Ticks));
                            oMinDateTime = new DateTime(Math.Min(File.GetLastWriteTime(oFile).Ticks, oMinDateTime.Ticks));

                            IReadOnlyList<MetadataExtractor.Directory> oDirectories = MetadataExtractor.ImageMetadataReader.ReadMetadata(oFile);
                            MetadataExtractor.Formats.Exif.ExifSubIfdDirectory oExifSubIfdDirectory = oDirectories.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
                            if (oExifSubIfdDirectory != null)
                            {
                                object oDateTime = oExifSubIfdDirectory.GetObject(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeOriginal);
                                if (oDateTime != null)
                                {
                                    DateTime oTagDateTimeOriginal = DateTime.ParseExact(oDateTime.ToString().Substring(0, 10), "yyyy:MM:dd", System.Threading.Thread.CurrentThread.CurrentCulture);
                                    oMinDateTime = new DateTime(Math.Min(oTagDateTimeOriginal.Ticks, oMinDateTime.Ticks));
                                }
                            }

                            if (oFileName.StartsWith("IMG-") || oFileName.StartsWith("VID_") || oFileName.StartsWith("VID-"))
                            {
                                DateTime oDateTimeWhatsapp = DateTime.Today;
                                if (DateTime.TryParseExact(oFileName.Substring(4, 8), "yyyyMMdd", System.Threading.Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.None, out oDateTimeWhatsapp))
                                {
                                    oMinDateTime = new DateTime(Math.Min(oDateTimeWhatsapp.Ticks, oMinDateTime.Ticks));
                                }
                            }

                            //Console.WriteLine(oFile + " " + oCreationTime.ToShortDateString() + " " + oLastAccessTime.ToShortDateString() + " " + oLastWriteTime.ToShortDateString());

                            string oDirectory = oMinDateTime.ToString("yyyy-MM-dd");

                            oDirectory = Path.Combine(txtDirectory.Text, oDirectory);
                            if (!Directory.Exists(oDirectory)) { Directory.CreateDirectory(oDirectory); }

                            Console.WriteLine(oFile + " " + oDirectory);

                            string newFile = Path.Combine(oDirectory, oFileName);
                            if (!File.Exists(newFile)) { File.Move(oFile, newFile); }

                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //throw;
                }
            }
        }
    }
}