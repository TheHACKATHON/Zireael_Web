using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class Recovery
    {
        public int Id { get; set; }
        public DateTime RequestTime { get; set; }//время отправки кода
        public DateTime RecoveryTime { get; set; }//время приминения кода
        public string ConfirmationCode { get; set; }//код востановления
        public string NewInformation { get; set; }//новая почта
        public bool IsWorking { get; set; }//можно ли использовать код
        public int UserId { get; set; }//для какого пользователя
        public Recovery()
        {
            RequestTime = DateTime.Now;
        }
    }
}
