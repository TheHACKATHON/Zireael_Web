using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Avatar
    {
        public int Id { get; set; }
        public byte[] SmallData { get; set; }
        public byte[] BigData { get; set; }
        public string Format { get; set; }
        public DateTime DateTime { get; set; }
        public Avatar()
        {

        }
        public Avatar(AvatarWCF avatar, UserBase user)
        {
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
        }
    }

}
