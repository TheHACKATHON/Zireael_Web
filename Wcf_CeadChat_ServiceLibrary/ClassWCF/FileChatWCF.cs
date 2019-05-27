using System.Runtime.Serialization;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class FileChatWCF
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Lenght { get; set; }
        [DataMember]
        public string Hash { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int CountPackages { get; set; }
        [DataMember]
        public int CountReadyPackages { get; set; }
        
        [DataMember]
        public string FullPath { get; set; }
        public FileChatWCF(FileChat file)
        {
            Name = file.Name;
            Lenght = file.Lenght;
            Hash = file.Hash;
            Id = file.Id;
            FullPath = file.FullPath;
            CountPackages = file.CountPackages;
            CountReadyPackages = file.CountReadyPackages;
        }
        public FileChatWCF()
        {

        }
    }
}