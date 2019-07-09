using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class UserBase
    {
        string login;
        public int Id { get; set; }
        public string Login { get=> login;
            set
            {
                login = value;
                if(string.IsNullOrEmpty(DisplayName))
                {
                    DisplayName = login;
                }
            } }
        public string DisplayName { get; set; }
        public DateTime LastTimeOnline { get; set; }
        public bool IsOnline { get; set; }
        public UserBase()
        {
            DisplayName = Login;
            LastTimeOnline = DateTime.Now;
        }

        public UserBase(User u)
        {
            Id = u.Id;
            Login = u.Login;
            IsOnline = u.IsOnline;
            LastTimeOnline = u.LastTimeOnline;
            DisplayName = u.DisplayName;
        }
        public UserBase(UserBaseWCF u)
        {
            Id = u.Id;
            IsOnline = u.IsOnline;
            Login = u.Login;
            LastTimeOnline = u.LastTimeOnline;
            if(u.DisplayName!= null)
            {
                DisplayName = u.DisplayName;
            }
        }
    }
}
