using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class GroupWCF
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ICollection<UserBaseWCF> Users { get; set; }
        [DataMember]
        public ICollection<MessageWCF> Messages { get; set; }
        [DataMember]
        public GroupType Type { get; set; }
        [DataMember]
        public MessageWCF LastMessage { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public UserBaseWCF Creator { get; set; }
        public GroupWCF()
        {
            Users = new List<UserBaseWCF>();
            Messages = new List<MessageWCF>();
        }
        public GroupWCF(Group group)
        {
            Users = new List<UserBaseWCF>();
            Messages = new List<MessageWCF>();
            Id = group.Id;
            Name = group.Name;
            Type = group.Type;

            if (group.Creator != null)
            {
                Creator = new UserBaseWCF(group.Creator);
            }
            if (group.LastMessage != null)
            {
                LastMessage = group.LastMessage is MessageFile file ? new MessageFileWCF(file) : new MessageWCF(group.LastMessage);
            }
            foreach (var item in group.Users)
            {
                Users.Add(new UserBaseWCF(item));
            }
            foreach (var item in group.Messages)
            {
                Messages.Add(new MessageWCF(item));
            }
        }
    }
}
