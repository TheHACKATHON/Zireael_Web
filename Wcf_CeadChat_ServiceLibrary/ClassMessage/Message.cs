using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Message
    {
        public int Id { get; set; }
        public bool IsVisible { get; set; }
        public string  Text  { get; set; }
        public DateTime DateTime { get;  set; }
        public bool IsRead { get;  set; }
        public bool IsChanged { get;  set; }
        virtual public Group Group { get; set; }
        virtual public UserBase Sender { get; set; }
        public Message()
        {
            DateTime = DateTime.Now;
            IsRead = false;
            IsVisible = true;
            IsChanged = false;
        }
        public Message(MessageWCF message, Group group, UserBase sender)
        {
            IsVisible = true;
            Id = message.Id;
            Text = message.Text;
            DateTime = message.DateTime;
            IsRead = message.IsRead;
            Group = group;
            Sender = sender;
            IsChanged = message.IsChanged;
        }
    }
}
