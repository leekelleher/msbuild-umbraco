using System;
using System.Globalization;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Umbraco.Tasks
{
    public class Package : Task
    {
        [Required]
        public string ManifestFile { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public string WorkingDirectory { get; set; }

        [Required]
        public string OutputDirectory { get; set; }

        protected string _packageFileName;
        protected string PackageFileName
        {
            get
            {
                if(string.IsNullOrEmpty(_packageFileName))
                {
                    var doc = new XmlDocument();
                    doc.Load(ManifestFile);

                    var packageNameNode = doc.SelectSingleNode(Constants.PACKAGE_NAME_XPATH);
                    var packageVersionNode = doc.SelectSingleNode(Constants.PACKAGE_VERSION_XPATH);

                    _packageFileName = Path.Combine(OutputDirectory, packageNameNode.InnerText.Replace(" ", "_") + "_" + packageVersionNode.InnerText + ".zip");
                }

                return _packageFileName;
            }
        }

        public override bool Execute()
        {
            return PackageFiles();
        }

        protected bool PackageFiles()
        {
            var flag = false;
            var crc = new Crc32();

            ZipOutputStream stream = null;

            try
            {
                using (stream = new ZipOutputStream(File.Create(PackageFileName)))
                {
                    stream.SetLevel(9);

                    var buffer = new byte[0x8000];
                    foreach (var item in Files)
                    {
                        var itemSpec = item.ItemSpec;
                        var info = new FileInfo(itemSpec);

                        if (!info.Exists)
                        {
                            Log.LogWarning("File not found", new object[] { info.FullName });
                        }
                        else
                        {
                            itemSpec = ZipEntry.CleanName(info.Name, true);
                            var entry = new ZipEntry(itemSpec);
                            entry.DateTime = info.LastWriteTime;
                            entry.Size = info.Length;

                            using (var stream2 = info.OpenRead())
                            {
                                crc.Reset();
                                var length = stream2.Length;
                                while (length > 0L)
                                {
                                    var len = stream2.Read(buffer, 0, buffer.Length);
                                    crc.Update(buffer, 0, len);
                                    length -= len;
                                }
                                entry.Crc = crc.Value;
                                stream.PutNextEntry(entry);
                                length = stream2.Length;
                                stream2.Seek(0L, SeekOrigin.Begin);
                                while (length > 0L)
                                {
                                    var count = stream2.Read(buffer, 0, buffer.Length);
                                    stream.Write(buffer, 0, count);
                                    length -= count;
                                }
                            }

                            Log.LogMessage("File added to package", new object[] { itemSpec });
                        }
                    }
                    stream.Finish();
                }

                Log.LogMessage("Package created", new object[] { PackageFileName });
                flag = true;
            }
            catch (Exception exception)
            {
                Log.LogErrorFromException(exception);
                flag = false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return flag;
        }
    }
}
