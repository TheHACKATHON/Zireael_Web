using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    [KnownType(typeof(UserWCF))]
    public class UserBaseWCF
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public DateTime LastTimeOnline { get; set; }
        [DataMember]
        public bool IsOnline { get; set; }
        [DataMember]
        public bool IsBlocked { get; set; }

        public UserBaseWCF()
        {
            DisplayName = Login;
            LastTimeOnline = DateTime.Now;
        }
        public UserBaseWCF(UserBase userBase)
        {
            Id = userBase.Id;
            IsOnline = userBase.IsOnline;
            Login = userBase.Login;
            LastTimeOnline = userBase.LastTimeOnline;
            DisplayName = userBase.DisplayName;
        }

        public UserBaseWCF(User user)
        {
            Id = user.Id;
            Login = user.Login;
            IsOnline = user.IsOnline;
            LastTimeOnline = user.LastTimeOnline;
            if (user.DisplayName != null)
            {
                DisplayName = user.DisplayName;
            }
            
        }
        public UserBaseWCF(UserWCF user)
        {
            Id = user.Id;
            Login = user.Login;
            IsOnline = user.IsOnline;
            LastTimeOnline = user.LastTimeOnline;
            DisplayName = user.DisplayName;
        }
    }
}
