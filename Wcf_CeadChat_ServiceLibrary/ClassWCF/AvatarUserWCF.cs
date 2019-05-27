using System.Runtime.Serialization;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class AvatarUserWCF:AvatarWCF
    {
        [DataMember]
        public UserBaseWCF User { get; set; }
        public AvatarUserWCF(AvatarUser avatar)
        {
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
            User = new UserBaseWCF(avatar.User);
        }
    }
}