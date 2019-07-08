using System.IO;
using System.Runtime.Serialization;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class AvatarGroupWCF : AvatarWCF
    {
        [DataMember]
        public GroupWCF Group { get; set; }
        public AvatarGroupWCF(AvatarGroup avatar)
        {
            if(!string.IsNullOrWhiteSpace(avatar.FilePath) 
                && !string.IsNullOrWhiteSpace(avatar.SmallFilePath) 
                && avatar.Group != null)
            {
                SmallData = File.ReadAllBytes(avatar.SmallFilePath);
                BigData = File.ReadAllBytes(avatar.FilePath);
                Format = new FileInfo(avatar.FilePath).Extension;
                DateTime = avatar.DateTime;
                Group = new GroupWCF(avatar.Group);
            }
        }
    }

}