using System;
using System.Collections.Generic;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.Core
{
    public class DirectoryNode
    {
        private string _senderUserName;
        private string _name;
        private List<FileNode> _fileNodes;
        private List<DirectoryNode> _directoryNodes;

        public string SenderUserName
        {
            get => _senderUserName;
            set => _senderUserName = value;
        }

        public string DirectoryName
        {
            get => _name;
        }

        public List<FileNode> FileNodes
        {
            get => _fileNodes;
        }

        public List<DirectoryNode> DirectoryNodes
        {
            get => _directoryNodes;
        }

        public DirectoryNode(String name)
        {
            _name = name;

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
    }
}
