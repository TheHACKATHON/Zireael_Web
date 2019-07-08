using System.IO;
using System.Runtime.Serialization;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class AvatarUserWCF : AvatarWCF
    {
        [DataMember]
        public UserBaseWCF User { get; set; }
        public AvatarUserWCF(AvatarUser avatar)
        {
            if (!string.IsNullOrWhiteSpace(avatar.FilePath)
                && !string.IsNullOrWhiteSpace(avatar.SmallFilePath)
                && avatar.User != null)
            {
                SmallData = File.ReadAllBytes(avatar.SmallFilePath);
                BigData = File.ReadAllBytes(avatar.FilePath);
                Format = new FileInfo(avatar.FilePath).Extension;
                DateTime = avatar.DateTime;
                User = new UserBaseWCF(avatar.User);
            }
        }
    }
}