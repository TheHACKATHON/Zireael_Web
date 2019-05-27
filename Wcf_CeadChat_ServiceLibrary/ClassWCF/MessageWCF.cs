using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    [KnownType(typeof(MessageFileWCF))]
    public class MessageWCF
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public bool IsRead { get; set; }
        [DataMember]
        public bool IsChanged { get; set; }
        [DataMember]
        public int GroupId { get; set; }
        [DataMember]
        public UserBaseWCF Sender { get; set; }
        public MessageWCF()
        {
            DateTime = DateTime.Now;
            IsRead = false;
            IsChanged = false;
        }
        public MessageWCF(Message message)
        {
            Id = message.Id;
            Text = message.Text;
            DateTime = message.DateTime;
            IsRead = message.IsRead;
            GroupId = message.Group.Id;
            IsChanged = message.IsChanged;
            if (message.Sender != null)
            {
                Sender = new UserBaseWCF(message.Sender);
            }
            else
            {
                Sender = new UserBaseWCF { Login = "system" };
            }
        }
    }
}
