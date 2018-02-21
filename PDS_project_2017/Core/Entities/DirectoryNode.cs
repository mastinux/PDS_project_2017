using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.Core
{
    public class DirectoryNode
    {
        private String _directoryName;
        private List<FileNode> _fileNodes;
        private List<DirectoryNode> _directoryNodes;

        public String DirectoryName { get => _directoryName; }
        public List<FileNode> FileNodes { get => _fileNodes; }
        public List<DirectoryNode> DirectoryNodes { get => _directoryNodes; }

        public DirectoryNode(String directoryName)
        {
            _directoryName = directoryName;

            _directoryNodes = new List<DirectoryNode>();
            _fileNodes = new List<FileNode>();
        }

        public void AddChildDirectory(DirectoryNode directoryNode)
        {
            _directoryNodes.Add(directoryNode);
        }

        public void AddChildFile(FileNode fileNode)
        {
            _fileNodes.Add(fileNode);
        }

        public void Print()
        {
            PrintDirectoryNode(this, 0);
        }

        private void PrintDirectoryNode(DirectoryNode directoryNode, int level)
        {
            PrintIndentation(level);
            Console.WriteLine("> " + directoryNode.DirectoryName);

            foreach (var dn in directoryNode.DirectoryNodes)
            {
                PrintDirectoryNode(dn, level + 1);
            }

            foreach (var fn in directoryNode.FileNodes)
            {
                PrintIndentation(level + 1);
                Console.WriteLine(fn.Name + " " + fn.Dimension);
            }
        }

        private void PrintIndentation(int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write("\t");
        }
    }
}
