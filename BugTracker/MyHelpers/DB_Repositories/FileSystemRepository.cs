using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class FileSystemRepository
    {
        public const string TextExtension = CONSTANTS.TextExtension;
        public const string JsonExtension = CONSTANTS.JsonExtension;
        public const string UploadFolder = CONSTANTS.UploadFolder;
        public const string BenchmarkFolder = CONSTANTS.BenchmarkFolder;
        public string ServerMapPath { get; private set; }
        public readonly HashSet<string> FileExtensionsToAvoid;
        private readonly HttpServerUtilityBase HttpServerUtilityBase;

        /// <summary>Meant for saving to <see cref="UploadFolder" /></summary>
        public FileSystemRepository(HttpServerUtilityBase httpServerUtilityBase, HashSet<string> fileExtensionsToAvoid, string folderToSaveTo = UploadFolder)
        {
            FileExtensionsToAvoid = fileExtensionsToAvoid == null || fileExtensionsToAvoid.Count == 0
                ? new HashSet<string> { ".exe", ".batch", ".bat", ".tmp", ".cmd" }
                : fileExtensionsToAvoid;
            HttpServerUtilityBase = httpServerUtilityBase;
            ServerMapPath = InitializeDirectory(folderToSaveTo) ?? throw new Exception();
        }

        /// <summary>Meant for saving json to <paramref name="folderToSaveTo"/></summary>
        public FileSystemRepository(HttpServerUtilityBase httpServerUtilityBase, string folderToSaveTo)
        {
            if (string.IsNullOrWhiteSpace(folderToSaveTo))
            {
                throw new ArgumentException("Invalid data", nameof(folderToSaveTo));
            }
            FileExtensionsToAvoid = new HashSet<string>();
            HttpServerUtilityBase = httpServerUtilityBase;
            ServerMapPath = InitializeDirectory(folderToSaveTo) ?? throw new Exception();
        }

        public string InitializeDirectory(string folderToSaveTo = UploadFolder)
        {
            if (string.IsNullOrWhiteSpace(folderToSaveTo))
            {
                throw new ArgumentException(nameof(folderToSaveTo));
            }

            string serverMapPath = HttpServerUtilityBase?.MapPath(folderToSaveTo) ?? throw new ArgumentException(nameof(folderToSaveTo));

            if (!Directory.Exists(serverMapPath))
            {
                Directory.CreateDirectory(serverMapPath);
            }
            return serverMapPath;
        }
        public void SetServerPath(string folderToSaveTo)
        {
            ServerMapPath = InitializeDirectory(folderToSaveTo);
        }

        public bool IsValidFileExtension(string fileExtension) => !FileExtensionsToAvoid.Contains(fileExtension.ToLower());

        /// <summary>returns <see cref="UploadFolder"/> + <paramref name="physicalPath"/>'s fileName</summary>
        /// <param name="physicalPath">Path needs to be within the <see cref="UploadFolder"/></param>
        /// <returns><see cref="UploadFolder"/> + <paramref name="physicalPath"/>'s fileName (without '~' character at the start)</returns>
        public string GetVirtualPathFromPhyiscalPath(string physicalPath, string folder = UploadFolder) => (folder + Path.GetFileName(physicalPath))?.TrimStart('~');
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
                fileUrl = GetVirtualPathFromPhyiscalPath(filePath, UploadFolder);
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

        public (bool hasSaved, string filePath, string resultMessage) SaveJsonFile<T>(string fileName, T @object) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Invalid Name", nameof(fileName));
            }
            else if (@object == null)
            {
                throw new ArgumentException($@"""{nameof(@object)}"" Can't Null", nameof(@object));
            }


            if (!fileName.EndsWith(JsonExtension))
            {
                fileName += JsonExtension;
            }

            string json;
            string filePath = ServerMapPath + fileName;
            try
            {
                json = JsonConvert.SerializeObject(@object, Formatting.Indented);

                if (string.IsNullOrWhiteSpace(json)) throw new Exception($"{nameof(@object)} failed to convert to a json string");
            }
            catch (ArgumentException e)
            {
                return (false, filePath, e.Message);
            }
            catch (Exception e)
            {
                return (false, filePath, e.Message);
            }

            File.WriteAllText(filePath, json, Encoding.UTF8);

            if (File.Exists(filePath))
            {
                return (true, filePath, $"{fileName} was saved successfully");
            }

            return (false, filePath, "Something bad happened");
        }

        public (T resultObject, bool hasLoaded, string resultMessage) LoadJsonFile<T>(string fileName) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Invalid FileName", nameof(fileName));
            }
            else if (!Directory.Exists(ServerMapPath))
            {
                throw new ArgumentException("Fatal Error: Directory not found", nameof(fileName));
            }
            else if (!File.Exists(ServerMapPath + fileName)  && !File.Exists(ServerMapPath + fileName + JsonExtension))
            {
                return (default(T), false, "File Doesn't Exist");
            }


            if (!fileName.EndsWith(JsonExtension))
            {
                fileName += JsonExtension;
            }

            string path = ServerMapPath + fileName;
            T obj;
            try
            {
                string json = File.ReadAllText(path);
                obj = JsonConvert.DeserializeObject<T>(json);

                if (obj is null) throw new Exception("Failed to convert from JSON");
            }
            catch (Exception e)
            {
                return (default(T), false, e.Message);
            }

            return (obj, true, $"{nameof(T)} object has loaded");
        }

        public (bool hasSaved, string filePath, string resultMessage) SaveTextFile(string fileName, string contents)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Invalid Name", nameof(fileName));
            }
            else if (contents == null)
            {
                throw new ArgumentException($@"""{nameof(contents)}"" Can't Null", nameof(contents));
            }


            if (!fileName.EndsWith(TextExtension))
            {
                fileName += TextExtension;
            }

            string filePath = ServerMapPath + fileName;
            try
            {
                File.WriteAllText(filePath, contents, Encoding.UTF8);
            }
            catch (ArgumentException e)
            {
                return (false, filePath, e.Message);
            }
            catch (Exception e)
            {
                return (false, filePath, e.Message);
            }


            if (File.Exists(filePath))
            {
                return (true, filePath, $"{fileName} was saved successfully");
            }

            return (false, filePath, "Something bad happened");
        }
    }
}