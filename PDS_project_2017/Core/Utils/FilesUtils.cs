using System;
using System.IO;
using System.Web;
using System.Windows.Forms;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.Core
{
    class FilesUtils
    {
        private static object _lock = new object();

        public static DirectoryNode BuildDirectoryNode(string path)
        {
            var rootDirectoryInfo = new DirectoryInfo(path);

            return CreateDirectoryNode(rootDirectoryInfo);
        }

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
        
        public static TreeView ListDirectory(string path)
        {
            TreeView treeView = new TreeView();

            var rootDirectoryInfo = new DirectoryInfo(path);

            treeView.Nodes.Add(CreateTreeNode(rootDirectoryInfo));

            PrintTreeView(treeView.Nodes[0], 0);

            return treeView;
        }

        private static TreeNode CreateTreeNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateTreeNode(directory));

            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(file.Name));

            return directoryNode;
        }

        private static void PrintTreeView(TreeNode treeNode, int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write("\t");

            Console.WriteLine(treeNode.Text);

            foreach (TreeNode node in treeNode.Nodes)
            {
                PrintTreeView(node, level + 1);
            }
        }

        public static FileStream CreateUniqueFile(string filePath)
        {
            int count = 1;

            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            string fileNameFormat = fileDirectory + "\\" + fileNameWithoutExtension + " ({0})" + fileExtension;

            // TODO understand why first progress bar is closed

            lock (_lock)
            {
                while (File.Exists(filePath))
                {
                    filePath = string.Format(fileNameFormat, count);

                    count++;
                }
            }

            return File.Open(filePath, FileMode.Create);
        }

        public static string CheckDirectoryExistance(string directoryPath)
        {
            int count = 1;

            string directoryNameFormat = directoryPath + " ({0})";

            while (Directory.Exists(directoryPath))
            {
                directoryPath = string.Format(directoryNameFormat, count);

                count++;
            }

            return directoryPath;
        }
    }
}
