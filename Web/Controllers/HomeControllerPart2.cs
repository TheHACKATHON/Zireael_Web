using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public partial class HomeController
    {
        private int _emailTokenLenght = 64;
        private string _nameEmailTokenCookie = "emailToken";
        [HttpPost]
        public async Task<ActionResult> SendCodeForChangePassword(string loginOrEmail)
        {
            if (await _client.SendCodeForRestorePasswordAsync(loginOrEmail))
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
                if (await _client.RestorePasswordAsync(loginOrEmail, code, pass))
                {
                    return Json(new { message = "Пароль успешно изменен!", type = NotifyType.Error.ToString() });
                }
                else
                {
                    return Json(new { message = "Ошибка! Проверте правильность введенных данных.", type = NotifyType.Error.ToString() });
                }
            }
            else
            {
                return Json(new { message = "Пароли не совпадают!", type = NotifyType.Error.ToString() });
            }
        }
        public async Task<JsonResult> RegistrationSendCode(string email)
        {
            var token = Generator.String(_emailTokenLenght);
            var code = await _client.SendCodeOnEmailAsync(email, token);
            if (code != null)
            {
                if (code != "")
                {
                    Response.Cookies.Add(new HttpCookie(_nameEmailTokenCookie, token));
                    return Json(new { message = $"Код отправлен на {email.ToLower()}!", type = NotifyType.Success.ToString() });
                }
                else
                {
                    return Json(new { message = $"Аккаунт с почтой {email.ToLower()} уже создан!", type = NotifyType.Warning.ToString() });
                }
            }
            else
            {
                return Json(new { message = $"Ошибка! Код НЕ отправлен на {email.ToLower()}!", type = NotifyType.Error.ToString() });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Registration(string login, string password, string repPassword, string email, string code)
        {
            if (Regex.IsMatch(login, @"^(?=.*[A-Za-z0-9]$)[A-Za-z][A-Za-z\d]{5,24}$"))
            {
                if (!await _client.LoginExistAsync(login))
                {
                    if (!await _client.EmailExistAsync(email))
                    {
                        if (Regex.IsMatch(password, @"^(?=.*[a-zа-я])(?=.*[A-ZА-Я]).{8,32}$"))
                        {
                            if (password == repPassword)
                            {

                                var cook = Request.Cookies[_nameEmailTokenCookie]?.Value;
                                if (cook != null)
                                {
                                    string emailFromDB = await _client.GetEmailByCookieAndCodeAsync(cook, code);
                                    if (emailFromDB != null)
                                    {
                                        if (await _client.RegistrationAsync(new UserWCF() { Email = emailFromDB, Login = login, PasswordHash = password }))
                                        {
                                            var html = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialLogin", null);
                                            return Json(new { message = "Вы зарегистрированы УСПЕШНО", type = NotifyType.Success.ToString(), html });
                                        }
                                        else
                                        {
                                            return Json(new { message = "Уупс... Ошибка при регистрации...", type = NotifyType.Error.ToString() });
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { message = "Не верный код с почты", type = NotifyType.Warning.ToString() });
                                    }
                                }
                                else
                                {
                                    return Json(new { message = "Отправьте код на ВАШУ почту!", type = NotifyType.Warning.ToString() });
                                }
                            }
                            else
                            {
                                return Json(new { message = "Пароли не совпадают!", type = NotifyType.Warning.ToString() });
                            }
                        }
                        else
                        {
                            return Json(new
                            {
                                message = "Требования для пароля:\n" +
                                "минимум 8 символов\n" +
                                "максимум 32 символа\n" +
                                "буква в нижнем регистре\n" +
                                "буква в верхнем регистре",
                                type = NotifyType.Warning.ToString()
                            });
                        }
                    }
                    else
                    {
                        return Json(new { message = "Пользователь с таким email уже зарегистрирован!", type = NotifyType.Warning.ToString() });
                    }
                }
                else
                {
                    return Json(new { message = "Логин занят", type = NotifyType.Warning.ToString() });
                }
            }
            else
            {
                return Json(new
                {
                    message = "Требования для логина:\n" +
                    "может состоять только из латинских символов и цифр\n" +
                    "минимум 6 символов\n" +
                    "максимум 24 символа",
                    type = NotifyType.Warning.ToString()
                });
            }
        }
    }

    public static class RazorViewToStringFormat
    {
        /// <summary>  
        /// Render razorview to string   
        /// </summary>  
        /// <param name="controller"></param>  
        /// <param name="viewName"></param>  
        /// <param name="model"></param>  
        /// <returns></returns>  
        public static string RenderRazorViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
            // checking the view inside the controller  
            if (viewResult.View != null)
            {
                using (var sw = new StringWriter())
                {
                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                    return sw.GetStringBuilder().ToString();
                }
            }
            else
                return "View cannot be found.";
        }
    }
}