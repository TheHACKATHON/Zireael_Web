using System.Collections.Generic;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Group
    {
        public Group()
        {
            IsVisible = true;
            Users = new List<User>();
            Messages = new List<Message>();
        }
        virtual public UserBase Creator { get; set; }
        public bool IsVisible { get; set; }
        public string Name { get; set; }
        virtual public ICollection<User> Users { get; set; }
        virtual public ICollection<Message> Messages { get; set; }
        public GroupType Type { get; set; }
        virtual public Message LastMessage { get; set; }
        public int Id { get; set; } 
    }
}
