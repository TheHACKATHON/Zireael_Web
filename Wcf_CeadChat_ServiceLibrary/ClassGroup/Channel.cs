using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    class Channel:Group
    {
        virtual public List<UserBase> Admins { get; set; }
        public string Login { get; set; }

        public Channel()
        {
            Type = GroupType.Channel;
        }
    }
}
