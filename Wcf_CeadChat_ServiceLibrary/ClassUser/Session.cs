using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Session
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public string SessionId { get; set; }
        public string ConnectionId { get; set; }
        public DateTime Date { get; set; }
        public Session()
        {
            Date = DateTime.Now;
        }

        public Session(User user, string sess)
        {
            User = user;
            SessionId = sess;
            Date = DateTime.Now;
        }
    }
}
