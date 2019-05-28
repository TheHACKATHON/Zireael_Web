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
        public async Task<ActionResult> SendCodeForChangePassword(string loginOrEmail)
        {
            if(await _client.SendCodeForRestorePasswordAsync(loginOrEmail))
            {
                ViewBag.Email = loginOrEmail;
                return PartialView("PartialSendCode");
            }
            else
            {
                throw new ArgumentException();
            }
        }
        
        [HttpPost]
        public async Task<JsonResult> ChangePassword(string loginOrEmail, string pass, string repPass, string code)
        {
            if (pass == repPass)
            {
                if (_client.RestorePassword(loginOrEmail, code, pass))
                {
                    return Json(new { message = "Пароль успешно изменен!", type = ErrorType.OK.ToString() });
                }
                else
                {
                    return Json(new { message = "Ошибка! Проверте правильность введенных данных.", type = ErrorType.ERROR.ToString() });
                }
            }
            else
            {
                return Json(new { message = "Пароли не совпадают!", type = ErrorType.ERROR.ToString() });
            }
        }
    }
}