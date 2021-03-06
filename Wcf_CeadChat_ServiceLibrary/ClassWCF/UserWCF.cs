﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public class UserWCF:UserBaseWCF
    {
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string PasswordHash { get; set; }
        //[DataMember]
        //public ICollection<UserBaseWCF> Friends { get; set; }
        [DataMember]
        public ICollection<GroupWCF> Groups { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public string Session { get; set; }
        public UserWCF() : base()
        {
            DateCreated = DateTime.Now;
            // Friends = new List<UserBaseWCF>();
            Groups = new List<GroupWCF>();
        }
        public UserWCF(User user, string session = null)
        {
            Groups = new List<GroupWCF>();
            Id = user.Id;
            Login = user.Login;
            Token = user.Token;
            if (user.DisplayName != null)
            {
                DisplayName = user.DisplayName;
            }
            else
            {
                DisplayName = Login;
            }
            Email = user.Email;
            PasswordHash = user.PasswordHash;
            Session = session;
            DateCreated = user.DateCreated;
            IsOnline = user.IsOnline;
            //Friends = new List<UserBaseWCF>();
            foreach (var item in user.Groups.Where(g=> g.IsVisible))
            {
                Groups.Add(new GroupWCF(item));
            }
            //foreach (var item in user.Friends)
            //{
            //    Friends.Add(new UserBaseWCF(item));
            //}
        }

    }
}
