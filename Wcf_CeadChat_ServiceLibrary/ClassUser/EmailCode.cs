using System;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class EmailCode
    {
        public int Id { get; set; }
        public DateTime RequestTime { get; set; }//время отправки кода
        public string ConfirmationCode { get; set; }//код востановления
        public string Email { get; set; }//на какую почту отправлено
        public string Cookie { get; set; }//для какого пользователя
        public EmailCode()
        {
            RequestTime = DateTime.Now;
        }
    }
}
