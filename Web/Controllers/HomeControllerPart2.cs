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
        private string _patternLogin = @"^(?=.*[A-Za-z0-9]$)[A-Za-z][A-Za-z\d]{5,24}$";
        private string _patternPassword = @"^(?=.*[a-zа-я])(?=.*[A-ZА-Я]).{8,32}$";
        private string _fatalError = "Мы потеряли связь с космосом, пытаемся восстановить квантовый соединитель. Попробуйте позже";

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
                    return Json(new { message = "Пароль успешно изменен!", type = NotifyType.Error });
                }
                else
                {
                    return Json(new { message = "Ошибка! Проверте правильность введенных данных.", type = NotifyType.Error });
                }
            }
            else
            {
                return Json(new { message = "Пароли не совпадают!", type = NotifyType.Error });
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
                    return Json(new { message = $"Код отправлен на {email.ToLower()}!", type = NotifyType.Success });
                }
                else
                {
                    return Json(new { message = $"Аккаунт с почтой {email.ToLower()} уже создан!", type = NotifyType.Warning });
                }
            }
            else
            {
                return Json(new { message = $"Ошибка! Код НЕ отправлен на {email.ToLower()}!", type = NotifyType.Error });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Registration(string login, string password, string repPassword, string email, string code)
        {
            if (Regex.IsMatch(login, _patternLogin))
            {
                if (!await _client.LoginExistAsync(login))
                {
                    if (!await _client.EmailExistAsync(email))
                    {
                        if (Regex.IsMatch(password, _patternPassword))
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
                                            return Json(new { message = "Вы зарегистрированы УСПЕШНО", type = NotifyType.Success, html });
                                        }
                                        else
                                        {
                                            return Json(new { message = _fatalError, type = NotifyType.Error });
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { message = "Не верный код с почты", type = NotifyType.Warning });
                                    }
                                }
                                else
                                {
                                    return Json(new { message = "Отправьте код на ВАШУ почту!", type = NotifyType.Warning });
                                }
                            }
                            else
                            {
                                return Json(new { message = "Пароли не совпадают!", type = NotifyType.Warning });
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
                                type = NotifyType.Warning
                            });
                        }
                    }
                    else
                    {
                        return Json(new { message = "Пользователь с таким email уже зарегистрирован!", type = NotifyType.Warning });
                    }
                }
                else
                {
                    return Json(new { message = "Логин занят", type = NotifyType.Warning });
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
                    type = NotifyType.Warning
                });
            }
        }
    }
}