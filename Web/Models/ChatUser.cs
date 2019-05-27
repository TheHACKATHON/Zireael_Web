using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.ServiceReference1;

namespace Web.Models
{
    public class ChatUser : UserBaseWCF
    {
        public string ConnectionId { get; set; }
    }
}