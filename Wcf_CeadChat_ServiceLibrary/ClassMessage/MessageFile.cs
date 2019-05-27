using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class MessageFile:Message
    {
        virtual public FileChat File { get; set; }
        public FileType Type { get; set; }
        public MessageFile(MessageFileWCF message, Group group, UserBase sender)
        {
            IsVisible = true;
            Id = message.Id;
            Text = message.Text;
            DateTime = message.DateTime;
            IsRead = message.IsRead;
            Group = group;
            Sender = sender;
            Type = message.Type;
            File = new FileChat(message.File);
        }

        public MessageFile()
        {
            DateTime = DateTime.Now;
            IsRead = false;
            IsVisible = true;
        }
    }
}
