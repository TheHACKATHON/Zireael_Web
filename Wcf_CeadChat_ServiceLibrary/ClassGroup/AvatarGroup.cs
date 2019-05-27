using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class AvatarGroup:Avatar
    {
        virtual public Group Group { get; set; }
        public AvatarGroup()
        {

        }
        public AvatarGroup(AvatarUserWCF avatar, Group group)
        {
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
            Group = group;
        }
    }
}
