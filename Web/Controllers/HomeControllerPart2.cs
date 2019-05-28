using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public partial class HomeController
    {
        [HttpPost]
        public async Task<ActionResult> SendCode(string loginOrEmail)
        {
            return PartialView("PartialSendCode");
        }
        
        [HttpPost]
        public async Task<JsonResult> ChangePassword(string loginOrEmail)
        {
            return Json(new {message = "Пароль успешно изменен!", type=ErrorType.OK.ToString() });
        }
    }
}