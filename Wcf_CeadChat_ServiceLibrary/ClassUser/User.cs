using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class User : UserBase
    {
        string token;
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        //public ICollection<User> Friends { get; set; }
        virtual public ICollection<Group> Groups { get; set; }
        public DateTime DateCreated { get; set; }
        public string Token { get => token; set
            {
                token = value;
                TokenDate = DateTime.Now;
            }}
        public DateTime TokenDate { get; set; }
        [NotMapped]
        public string SessionId { get; set; }
        public int AuthTry { get; set; }
        public DateTime LastTry { get; set; }
        public User() : base()
        {
            DateCreated = DateTime.Now;
            TokenDate = DateTime.Now;
            LastTry = DateTime.Now;
            //Friends = new List<User>();
            Groups = new List<Group>();
        }
        public User(UserWCF user)
        {
            Id = user.Id;
            Login = user.Login;
            Token = user.Token;
            DisplayName = user.DisplayName;
            LastTry = DateTime.Now;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
            DateCreated = user.DateCreated;
        }
    }
}
