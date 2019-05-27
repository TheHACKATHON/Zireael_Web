using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Friends
    {
        public int Id { get; set; }
        virtual public User Sender { get; set; }
        virtual public User User2 { get; set; }
    }
}
