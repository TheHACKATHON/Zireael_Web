using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class MessageFileWCF : MessageWCF
    {
        [DataMember]
        public FileChatWCF File { get; set; }
        [DataMember]
        public FileType Type { get; set; }
        public MessageFileWCF()
        {

        }
        public MessageFileWCF(MessageFile message)
        {
            Id = message.Id;
            Text = message.Text;
            DateTime = message.DateTime;
            IsRead = message.IsRead;
            GroupId = message.Group.Id;
            Sender = new UserBaseWCF(message.Sender);
            Type = message.Type;
            File = new FileChatWCF(message.File);
        }
    }
}
