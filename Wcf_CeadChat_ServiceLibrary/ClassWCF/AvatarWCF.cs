using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    [KnownType(typeof(AvatarUserWCF))]
    [KnownType(typeof(AvatarGroupWCF))]
    public class AvatarWCF
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public byte[] SmallData { get; set; }
        [DataMember]
        public byte[] BigData { get; set; }
        [DataMember]
        public string Format { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        public AvatarWCF()
        {

        }
        public AvatarWCF(Avatar avatar)
        {
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
        }
    }
}
