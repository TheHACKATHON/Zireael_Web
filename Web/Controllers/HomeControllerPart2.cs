using Newtonsoft.Json;
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
        private string _patternEmail = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        private string _fatalError = "Мы потеряли связь с космосом, пытаемся восстановить квантовый соединитель. Попробуйте позже";
        private string _loginMessage = "Требования для логина:\n" +
                    "только латинские символы и цифры\n" +
                    "минимум 6 символов\n" +
                    "максимум 24 символа";
        private string _passwordMessage = "Требования для пароля:\n" +
                                "минимум 8 символов\n" +
                                "максимум 32 символа\n" +
                                "буква в нижнем регистре\n" +
                                "буква в верхнем регистре";
        #region Registration
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
                return PartialView("PartialPassword");
            }
        }

        [HttpPost]
        public async Task<JsonResult> RestorePassword(string loginOrEmail, string pass, string repPass, string code)
        {
            if (pass == repPass)
            {
                if (Regex.IsMatch(pass, _patternPassword))
                {
                    if (await _client.RestorePasswordAsync(loginOrEmail, code, pass))
                    {
                        return Json(new { message = "Пароль успешно изменен!", type = NotifyType.Success });
                    }
                    else
                    {
                        return Json(new { message = "Ошибка! Проверте правильность введенных данных.", type = NotifyType.Warning });
                    }
                }
                else
                {
                    return Json(new { message = _passwordMessage, type = NotifyType.Warning });
                }
            }
            else
            {
                return Json(new { message = "Пароли не совпадают!", type = NotifyType.Warning });
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
                                message = _passwordMessage,
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
                    message = _loginMessage,
                    type = NotifyType.Warning
                });
            }
        }
        #endregion
        [HttpPost]
        public async Task<JsonResult> MenuContacts()
        {
            var friends = await _client.GetFriendsAsync();
            if (friends != null)
            {
                var view = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialMenuContacts", friends);
                return Json(new { Code = NotifyType.Success, view, title = "Контакты" });
            }
            return Json(new NotifyError(_fatalError));
        }
        [HttpPost]
        public async Task<JsonResult> MenuCreateGroup()
        {
            var friends = await _client.GetFriendsAsync();
            if (friends != null)
            {
                var view = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialMenuCreateGroup", friends);
                return Json(new { Code = NotifyType.Success, view, title = "Создание группы" });
            }
            return Json(new NotifyError(_fatalError));
        }
        [HttpPost]
        public async Task<JsonResult> MenuSettings()
        {
            var account = await _client.GetMyProfileAsync();
            if (account != null)
            {
                var view = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialMenuSettings", account);
                return Json(new { Code = NotifyType.Success, view, title = "Настройки" });
            }
            return Json(new NotifyError(_fatalError));
        }

        [HttpPost]
        public async Task<JsonResult> AddContact(string login)
        {
            var friend = await _client.AddFriendAsync(login);
            if (friend)
            {
                return Json(new { Code = NotifyType.Success, Message = $"{login} успешно добавлен!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Пользователь \"{login}\" НЕ добавлен..." });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteContacts(string idArr)
        {
            var id = JsonConvert.DeserializeObject<int[]>(idArr);
            foreach (var item in id)
            {
                await _client.RemoveFriendAsync(new UserBaseWCF { Id = item });
            }
            return Json(new { Code = NotifyType.Success });
        }

        [HttpPost]
        public async Task<JsonResult> CreateChat(int id)
        {

            if (await _client.CreateChatAsync(new UserBaseWCF { Id = id }))
            {
                return Json(new { Code = NotifyType.Success });
            }
            return Json(new { Code = NotifyType.Warning, Message = "Новый диалог не создан..." });
        }
        [HttpPost]
        public async Task<JsonResult> CreateGroup(string idArr, string groupName)
        {
            var users = new List<UserBaseWCF>();
            var id = JsonConvert.DeserializeObject<int[]>(idArr);
            foreach (var item in id)
            {
                users.Add(new UserBaseWCF { Id = item });
            }
            if (await _client.CreateGroupAsync(users.ToArray(), groupName))
            {
                return Json(new { Code = NotifyType.Success });
            }
            return Json(new { Code = NotifyType.Warning, Message = "Новый диалог не создан..." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Логин не может быть пустым!" });
            }
            if (await _client.ChangeProfileInfoAsync(null, login))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Логин изменен на {login}!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Логин НЕ изменен..."});
        }

        [HttpPost]
        public async Task<JsonResult> ChangeDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Имя не может быть пустым!" });
            }
            if (await _client.ChangeProfileInfoAsync(displayName, null))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Имя изменено на {displayName}!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Имя НЕ изменен..." });
        }
        [HttpPost]
        public async Task<JsonResult> ChangePassword(string newPass, string repNewPass, string oldPass)
        {
            if (string.IsNullOrWhiteSpace(newPass) &&
                string.IsNullOrWhiteSpace(repNewPass) &&
                string.IsNullOrWhiteSpace(oldPass))
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Заполните все поля!" });
            }
            if(newPass != repNewPass)
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Пароли не совпадают..." });
            }
            if (await _client.ChangePasswordAsync(newPass, oldPass))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Пароль изменен УСПЕШНО!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Пароль НЕ изменен..." });
        }
        [HttpPost]
        public async Task<JsonResult> ChangeEmailSendCode(string newEmail, string pass)
        {
            if (string.IsNullOrWhiteSpace(pass) &&
                string.IsNullOrWhiteSpace(newEmail))
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Заполните все поля!" });
            }

            if (Regex.IsMatch(newEmail, _patternEmail))
            {
                if (await _client.SendCodeForSetNewEmailAsync(newEmail, pass))
                {
                    return Json(new { Code = NotifyType.Success, Message = $"Код отправлен на {newEmail}" });
                }
                else
                {
                    return Json(new { Code = NotifyType.Warning, Message = $"Проверьте правильность пароля" });
                }
            }
            else
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Некорректно введена почта!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Код НЕ отправлен." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeEmail(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { Code = NotifyType.Warning, Message = $"Введите код с почты!" });
            }
            if (await _client.SetNewEmailAsync(code))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Почта изменена УСПЕШНО!" });
            }
            return Json(new { Code = NotifyType.Warning, Message = $"Почта НЕ изменена..." });
        }
    }


}