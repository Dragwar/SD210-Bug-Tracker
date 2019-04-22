using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class FileSystemRepository
    {
        public const string UploadFolder = @"~/UserUploads/";
        public readonly string ServerMapPath;
        public readonly HashSet<string> FileExtensionsToAvoid;

        public FileSystemRepository(HttpServerUtilityBase httpServerUtilityBase, HashSet<string> fileExtensionsToAvoid)
        {
            FileExtensionsToAvoid = fileExtensionsToAvoid == null || fileExtensionsToAvoid.Count == 0
                ? new HashSet<string> { ".exe", ".batch", ".bat", ".tmp", ".cmd" }
                : fileExtensionsToAvoid;

            ServerMapPath = InitializeDirectory(httpServerUtilityBase) ?? throw new Exception();
        }

        public string InitializeDirectory(HttpServerUtilityBase httpServerUtilityBase)
        {
            string serverMapPath = httpServerUtilityBase?.MapPath(UploadFolder) ?? throw new ArgumentNullException(nameof(httpServerUtilityBase));

            if (!Directory.Exists(serverMapPath))
            {
                Directory.CreateDirectory(serverMapPath);
            }
            return serverMapPath;
        }

        public bool IsValidFileExtension(string fileExtension) => !FileExtensionsToAvoid.Contains(fileExtension.ToLower());

        /// <summary>returns <see cref="UploadFolder"/> + <paramref name="physicalPath"/>'s fileName</summary>
        /// <param name="physicalPath">Path needs to be within the <see cref="UploadFolder"/></param>
        /// <returns><see cref="UploadFolder"/> + <paramref name="physicalPath"/>'s fileName (without '~' character at the start)</returns>
        public string GetVirtualPathFromPhyiscalPath(string physicalPath) => (UploadFolder + Path.GetFileName(physicalPath))?.TrimStart('~');
        public (bool hasSaved, string filePath, string fileUrl, string resultMessage) SaveFile(HttpPostedFileBase file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            string filePath;
            string fileUrl;
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);
                if (!IsValidFileExtension(fileExtension))
                {
                    throw new ArgumentException($"File Extension isn't allowed ({fileExtension})");
                }
                filePath = ServerMapPath + file.FileName;
                fileUrl = GetVirtualPathFromPhyiscalPath(filePath);
            }
            catch (ArgumentException e)
            {
                return (false, null, null, e.Message);
            }
            catch (Exception e)
            {
                return (false, null, null, e.Message);
            }

            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(fileUrl))
            {
                return (false, null, null, $"({nameof(filePath)} || {nameof(fileUrl)}) was invalid/null");
            }

            file.SaveAs(filePath);

            if (File.Exists(filePath))
            {
                return (true, filePath, fileUrl, $"{file.FileName} was saved successfully");
            }

            return (false, filePath, fileUrl, $"{file.FileName} failed to save at {filePath}");
        }
    }
}