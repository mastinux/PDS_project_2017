using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    public class DirectoryNode
    {
        private String _directoryName;
        private List<String> _fileNameNodes;
        private List<DirectoryNode> _directoryNodes;

        public String DirectoryName { get => _directoryName; }
        public List<String> FileNameNodes { get => _fileNameNodes; }
        public List<DirectoryNode> DirectoryNodes { get => _directoryNodes; }

        public DirectoryNode(String directoryName)
        {
            _directoryName = directoryName;

            _directoryNodes = new List<DirectoryNode>();
            _fileNameNodes = new List<string>();
        }

        public void AddChildDirectory(DirectoryNode directoryNode)
        {
            _directoryNodes.Add(directoryNode);
        }

        public void AddChildFile(String fileName)
        {
            _fileNameNodes.Add(fileName);
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

            foreach (var fn in directoryNode.FileNameNodes)
            {
                PrintIndentation(level + 1);
                Console.WriteLine(fn);
            }
        }

        private void PrintIndentation(int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write("\t");
        }
    }
}
