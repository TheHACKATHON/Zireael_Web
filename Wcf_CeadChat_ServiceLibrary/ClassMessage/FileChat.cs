using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class FileChat
    {
        public string Name { get; set; }
        public int Lenght { get; set; }
        public string Hash { get; set; }
        public int Id { get; set; }
        public string FullPath { get; set; }
        public int CountPackages { get; set; }
        public int CountReadyPackages { get; set; }
        public List<Package> Packages { get; set; }
        public FileChat(FileChatWCF chatWCF):base()
        {
            Name = chatWCF.Name;
            Lenght = chatWCF.Lenght;
            Hash = chatWCF.Hash;
            FullPath = chatWCF.FullPath;
            CountPackages = chatWCF.CountPackages;
            CountReadyPackages = chatWCF.CountReadyPackages;
        }

        public FileChat()
        {
            Packages = new List<Package>();
        }
    }
}
