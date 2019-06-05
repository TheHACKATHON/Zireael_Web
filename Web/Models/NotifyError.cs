using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class NotifyError
    {
        public string Code { get; } = NotifyType.Error;
        public string Error { get; set; }

        public NotifyError(string error)
        {
            Error = error;
        }
    }
}