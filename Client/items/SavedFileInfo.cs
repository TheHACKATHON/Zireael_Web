using System;
using System.Collections.Generic;
using AdditionsLibrary;
using Client.ServiceReference;

namespace Client
{
    [Serializable]
    public class SavedFileInfo
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdate { get; set; }
        public int Lenght { get; set; }
        public int MessageId { get; set; }

        public SavedFileInfo()
        {

        }

        public SavedFileInfo(string fullName, string name, DateTime lastUpdate, int lenght, int messageId)
        {
            FullName = fullName;
            Name = name;
            LastUpdate = lastUpdate;
            Lenght = lenght;
            MessageId = messageId;
        }

        public override bool Equals(object obj)
        {
            if (obj is SavedFileInfo savedFileInfo)
            {
                return savedFileInfo.FullName.Equals(FullName) &&
                    savedFileInfo.Lenght.Equals(Lenght) &&
                    savedFileInfo.LastUpdate.Equals(LastUpdate);
            }
            if (obj is MessageFileWCF messageFile)
            {
                return messageFile.Id.Equals(MessageId) &&
                    messageFile.File.Lenght.Equals(Lenght) &&
                    messageFile.File.Hash.Equals(GetHashCode());
            }
            return base.Equals(obj);
        }

        public new string GetHashCode() => HashCode.ComputeFromFile(FullName);
    }
}