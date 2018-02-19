using System;
using System.IO;
using System.Windows.Forms;

namespace PDS_project_2017.Core
{
    class FilesUtils
    {
        public static DirectoryNode ListDirectoryNode(string path)
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
                directoryNode.AddChildFile(file.Name);

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
    }
}
