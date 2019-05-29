using System;
using System.Collections.Generic;
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
                    return Json(new { message = "Пароль успешно изменен!", type = ErrorType.Error.ToString() });
                }
                else
                {
                    return Json(new { message = "Ошибка! Проверте правильность введенных данных.", type = ErrorType.Error.ToString() });
                }
            }
            else
            {
                return Json(new { message = "Пароли не совпадают!", type = ErrorType.Error.ToString() });
            }
        }
        public async Task<JsonResult> RegistrationSendCode(string email)
        {
            var token = Generator.String(_emailTokenLenght);
            if (await _client.SendCodeOnEmailAsync(email, token) != null)
            {
                Response.Cookies.Add(new HttpCookie(_nameEmailTokenCookie, token));
                return Json(new { message = $"Код отправлен на {email.ToLower()}!", type = ErrorType.Success.ToString() });
            }
            else
            {
                return Json(new { message = $"Ошибка! Код НЕ отправлен на {email.ToLower()}!", type = ErrorType.Error.ToString()});
            }
        }
        [HttpPost]
        public async Task<JsonResult> Registration(string login, string password, string repPassword, string email, string code)
        {
            if (Regex.IsMatch(password, @"^(?=.*[a-zа-я])(?=.*[A-ZА-Я]).{8,32}$"))
            {
                if (password == repPassword)
                {
                    if (Regex.IsMatch(login, @"^(?=.*[A-Za-z0-9]$)[A-Za-z][A-Za-z\d]{5,24}$"))
                    {
                        string emailFromDB = await _client.GetEmailByCookieAndCodeAsync(Request.Cookies[_nameEmailTokenCookie].Value, code);
                        if (emailFromDB != null)
                        {
                            if (await _client.RegistrationAsync(new UserWCF() { Email = emailFromDB, Login = login, PasswordHash = password }))
                            {
                                return Json(new { message = "Вы зарегистрированы УСПЕШНО", type = ErrorType.Success.ToString() });
                            }
                            else
                            {
                                return Json(new { message = "Уупс... Ошибка при регистрации...", type = ErrorType.Error.ToString() });
                            }
                        }
                        else
                        {
                            return Json(new { message = "Не верный код с почты", type = ErrorType.Warning.ToString() });
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
                            type = ErrorType.Warning.ToString()
                        });
                    }
                }
                else
                {
                    return Json(new { message = "Пароли не совпадают!", type = ErrorType.Warning.ToString() });
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
                    type = ErrorType.Warning.ToString()
                });
            }
        }
    }
}