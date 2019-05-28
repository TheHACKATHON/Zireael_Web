using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public partial class HomeController
    {
        [HttpPost]
        public async Task<ActionResult> SendCode(string loginOrEmail)
        {
            return PartialView("PartialSendCode");
        }
    }
}