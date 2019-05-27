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
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
            Group = new GroupWCF(avatar.Group);
        }
    }

}