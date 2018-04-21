using System;
using System.IO;
using System.Web;
using System.Windows.Forms;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.Core
{
    class FilesUtils
    {
        private static readonly object FileLock = new object();
        private static readonly object DirectoryLock = new object();

        // this method builds the directory description
        public static DirectoryNode BuildDirectoryNode(string path)
        {
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);

            return CreateDirectoryNode(rootDirectoryInfo);
        }

        // recursive method
        private static DirectoryNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            DirectoryNode directoryNode = new DirectoryNode(directoryInfo.Name);

            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.AddChildDirectory(CreateDirectoryNode(directory));

            foreach (var file in directoryInfo.GetFiles())
            {
                FileNode fileNode = new FileNode();
                fileNode.Name = file.Name;
                fileNode.Dimension = file.Length;
                fileNode.MimeType = MimeMapping.GetMimeMapping(file.Name);

                directoryNode.AddChildFile(fileNode);
            }

            return directoryNode;
        }
        
        public static FileStream CreateUniqueFile(string filePath)
        {
            int count = 1;

            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            string fileNameFormat = fileDirectory + "\\" + fileNameWithoutExtension + " ({0})" + fileExtension;

            lock (FileLock)
            {
                while (File.Exists(filePath))
                {
                    filePath = string.Format(fileNameFormat, count);

                    count++;
                }

                return File.Open(filePath, FileMode.Create);
            }
        }

        public static string CreateUniqueDirectory(string directoryPath)
        {
            int count = 1;

            string directoryNameFormat = directoryPath + " ({0})";

            lock (DirectoryLock)
            {
                while (Directory.Exists(directoryPath))
                {
                    directoryPath = string.Format(directoryNameFormat, count);

                    count++;
                }

                Directory.CreateDirectory(directoryPath);

                return directoryPath;
            }
        }
    }
}
