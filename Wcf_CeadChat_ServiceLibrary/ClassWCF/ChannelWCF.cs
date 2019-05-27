using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    class ChannelWCF:GroupWCF
    {
        [DataMember]
        public List<UserBase> Admins { get; set; }
        [DataMember]
        public string Login { get; set; }

        public ChannelWCF()
        {
            Type = GroupType.Channel;
        }
    }
}
