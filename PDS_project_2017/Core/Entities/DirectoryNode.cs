using System;
using System.Collections.Generic;
using PDS_project_2017.Core.Entities;

namespace PDS_project_2017.Core
{
    public class DirectoryNode
    {
        public string SenderUserName { get; set; }

        public string DirectoryName { get; }

        public List<FileNode> FileNodes { get; }

        public List<DirectoryNode> DirectoryNodes { get; }

        public DirectoryNode(String name)
        {
            DirectoryName = name;

            DirectoryNodes = new List<DirectoryNode>();
            FileNodes = new List<FileNode>();
        }

        public void AddChildDirectory(DirectoryNode directoryNode)
        {
            DirectoryNodes.Add(directoryNode);
        }

        public void AddChildFile(FileNode fileNode)
        {
            FileNodes.Add(fileNode);
        }
    }
}
