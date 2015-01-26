using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;

namespace MSBuild.Umbraco.Tasks
{
    public class ManifestUpdate : Task
    {
        private const string VERSION_NO_REGEX = @"^(\d).(\d).(\d)$";

        [Required]
        public string ManifestFile { get; set; }

        [Required]
        public string WorkingDirectory { get; set; }

        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string PackageLicenseName { get; set; }
        public string PackageLicenseUrl { get; set; }

        public string PackageUrl { get; set; }

        public string MinimumRequiredUmbracoVersion { get; set; }

        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }

        public string Readme { get; set; }

        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            try
            {
                // Load manifest
                var doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(ManifestFile);

                // Update package info
                XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_NAME_XPATH, PackageName);
                XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_VERSION_XPATH, PackageVersion);
                XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_URL, PackageUrl);
                XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_LICENSE_XPATH, PackageLicenseName);
                XmlHelper.UpdateAttribute(ref doc, Constants.PACKAGE_LICENSE_XPATH, "url", PackageLicenseUrl);

                // Update requirements
                if (!string.IsNullOrEmpty(MinimumRequiredUmbracoVersion) && Regex.IsMatch(MinimumRequiredUmbracoVersion, VERSION_NO_REGEX))
                {
                    var match = Regex.Match(MinimumRequiredUmbracoVersion, VERSION_NO_REGEX);

                    XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_REQUIREMENTS_XPATH + "/major", match.Groups[1].Value);
                    XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_REQUIREMENTS_XPATH + "/minor", match.Groups[2].Value);
                    XmlHelper.UpdateNode(ref doc, Constants.PACKAGE_REQUIREMENTS_XPATH + "/patch", match.Groups[3].Value);
                }

                // Update author info
                XmlHelper.UpdateNode(ref doc, Constants.AUTHOR_NAME_XPATH, AuthorName);
                XmlHelper.UpdateNode(ref doc, Constants.AUTHOR_URL_XPATH, AuthorUrl);

                // Update readme info
                XmlHelper.UpdateCDataNode(ref doc, Constants.README_XPATH, Readme);

                // Append files
                if(Files != null && Files.Length > 0)
                {
                    Log.LogMessage(Files.Length.ToString() + " files to add to Manifest");

                    var filesNode = doc.SelectSingleNode("//files");

                    foreach (var file in Files)
                    {
                        if(File.Exists(file.ItemSpec))
                        {
                            var fileInfo = new FileInfo(file.ItemSpec);
                            if (fileInfo.FullName.StartsWith(WorkingDirectory))
                            {
                                var relativeFilePath = fileInfo.FullName.Substring(WorkingDirectory.Length + 1).Replace(@"\", "/");
                                var relativeDirPath = "/" + relativeFilePath.Substring(0, relativeFilePath.LastIndexOf("/")).Trim('/');
                                var guid = Guid.NewGuid().ToString() + fileInfo.Extension;

                                var fileNode = XmlHelper.CreateNode(ref doc, "file", "");
                                var guidNode = XmlHelper.CreateNode(ref doc, "guid", guid);
                                var orgPathNode = XmlHelper.CreateNode(ref doc, "orgPath", relativeDirPath);
                                var orgNameNode = XmlHelper.CreateNode(ref doc, "orgName", fileInfo.Name);

                                fileNode.AppendChild(guidNode);
                                fileNode.AppendChild(orgPathNode);
                                fileNode.AppendChild(orgNameNode);

                                filesNode.AppendChild(fileNode);

                                fileInfo.MoveTo(fileInfo.DirectoryName + @"\" + guid);
                            }
                        }
                    }
                }

                // Save manifest
                doc.Save(ManifestFile);

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
            
        }
    }
}
