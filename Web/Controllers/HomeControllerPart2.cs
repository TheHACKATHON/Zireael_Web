﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AdditionsLibrary;
using Newtonsoft.Json;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public partial class HomeController
    {
        private const int EmailTokenLenght = 64;
        private const string NameEmailTokenCookie = "emailToken";
        private const string PatternLogin = @"^(?=.*[A-Za-z0-9]$)[A-Za-z][A-Za-z\d]{5,24}$";
        private const string PatternPassword = @"^(?=.*[a-zа-я])(?=.*[A-ZА-Я]).{8,32}$";
        private const string PatternEmail = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        private const string FatalError =
            "Мы потеряли связь с космосом, пытаемся восстановить квантовый соединитель. Попробуйте позже";

        private const string LoginMessage = "Требования для логина:\n" +
                                            "только латинские символы и цифры\n" +
                                            "минимум 6 символов\n" +
                                            "максимум 24 символа";

        private const string PasswordMessage = "Требования для пароля:\n" +
                                               "минимум 8 символов\n" +
                                               "максимум 32 символа\n" +
                                               "буква в нижнем регистре\n" +
                                               "буква в верхнем регистре";

        [HttpPost]
        public async Task<JsonResult> MenuContacts()
        {
            var friends = await _client.GetFriendsAsync();
            if (friends != null)
            {
                var view = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialMenuContacts", friends);
                return Json(new { Code = NotifyType.Success, view, title = "Контакты" });
            }

            return Json(new NotifyError(FatalError));
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

            return Json(new NotifyError(FatalError));
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

            return Json(new NotifyError(FatalError));
        }

        [HttpPost]
        public async Task<JsonResult> OpenProfile(string login)
        {
            if (!string.IsNullOrWhiteSpace(login))
            {
                var account = await _client.FindAsync(login);
                var myAccount = await _client.GetMyProfileAsync();
                
                if (account != null)
                {
                    ViewData["myProfile"] = myAccount;
                    var view = RazorViewToStringFormat.RenderRazorViewToString(this, "PartialProfile", account);
                    return Json(new { Code = NotifyType.Success, view, title = "Информация о пользователе" });
                }
            }
            return Json(new NotifyError(FatalError));
        }

        [HttpPost]
        public async Task<JsonResult> AddContact(string login)
        {
            var friend = await _client.AddFriendAsync(login);
            if (friend)
            {
                return Json(new { Code = NotifyType.Success, Message = $"{login} успешно добавлен!" });
            }

            return Json(new { Code = NotifyType.Warning, Message = $"Пользователь \"{login}\" не добавлен..." });
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
                return Json(new { Code = NotifyType.Warning, Message = "Логин не может быть пустым!" });
            }

            if (await _client.ChangeProfileInfoAsync(null, login))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Логин изменен на {login}!" });
            }

            return Json(new { Code = NotifyType.Warning, Message = "Логин не изменен..." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return Json(new { Code = NotifyType.Warning, Message = "Имя не может быть пустым!" });
            }

            if (await _client.ChangeProfileInfoAsync(displayName, null))
            {
                return Json(new { Code = NotifyType.Success, Message = $"Имя изменено на {displayName}!" });
            }

            return Json(new { Code = NotifyType.Warning, Message = "Имя неЕ изменен..." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword(string newPass, string repNewPass, string oldPass)
        {
            if (string.IsNullOrWhiteSpace(newPass) &&
                string.IsNullOrWhiteSpace(repNewPass) &&
                string.IsNullOrWhiteSpace(oldPass))
            {
                return Json(new { Code = NotifyType.Warning, Message = "Заполните все поля!" });
            }

            if (newPass != repNewPass)
            {
                return Json(new { Code = NotifyType.Warning, Message = "Пароли не совпадают..." });
            }

            if (await _client.ChangePasswordAsync(newPass, oldPass))
            {
                return Json(new { Code = NotifyType.Success, Message = "Пароль изменен успешно!" });
            }

            return Json(new { Code = NotifyType.Warning, Message = "Пароль не изменен..." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeEmailSendCode(string newEmail, string pass)
        {
            if (string.IsNullOrWhiteSpace(pass) &&
                string.IsNullOrWhiteSpace(newEmail))
            {
                return Json(new { Code = NotifyType.Warning, Message = "Заполните все поля!" });
            }

            if (Regex.IsMatch(newEmail, PatternEmail))
            {
                if (await _client.SendCodeForSetNewEmailAsync(newEmail, pass))
                {
                    return Json(new { Code = NotifyType.Success, Message = $"Код отправлен на {newEmail}" });
                }

                return Json(new { Code = NotifyType.Warning, Message = "Проверьте правильность пароля" });
            }

            return Json(new { Code = NotifyType.Warning, Message = "Некорректно введена почта!" });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeEmail(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { Code = NotifyType.Warning, Message = "Введите код с почты!" });
            }

            if (await _client.SetNewEmailAsync(code))
            {
                return Json(new { Code = NotifyType.Success, Message = "Почта изменена успешно!" });
            }

            return Json(new { Code = NotifyType.Warning, Message = "Почта не изменена..." });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeImage()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                byte[] bData = null, sData = null;
                try
                {
                    ImageWork.ImageForDB(file.InputStream, out sData, out bData);
                }
                catch (NotSupportedException ex)
                {
                    return Json(new
                    {
                        Code = NotifyType.Warning,
                        Message = $"Изображение должно быть минимум {ImageWork.MinimumSize}x{ImageWork.MinimumSize}px"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { Code = NotifyType.Error, Message = "Ошибка обработки файла!" });
                }

                var avatar = new AvatarUserWCF { BigData = bData, SmallData = sData, Format = ".png" };
                if (await _client.SetAvatarUserAsync(avatar))
                {
                    return Json(new { Code = NotifyType.Success, Message = "Аватар изменен успешно!" });
                }
                else
                {
                    return Json(new { Code = NotifyType.Error, Message = "Не удалось установить аватар!" });
                }
            }

            return Json(new { Code = NotifyType.Error, Message = "Файл не найден!" });
        }

        #region Registration

        [HttpPost]
        public async Task<ActionResult> SendCodeForChangePassword(string loginOrEmail)
        {
            if (await _client.SendCodeForRestorePasswordAsync(loginOrEmail))
            {
                ViewBag.Email = loginOrEmail;
                return PartialView("PartialSendCode");
            }

            return PartialView("PartialPassword");
        }

        [HttpPost]
        public async Task<JsonResult> RestorePassword(string loginOrEmail, string pass, string repPass, string code)
        {
            if (pass == repPass)
            {
                if (Regex.IsMatch(pass, PatternPassword))
                {
                    if (await _client.RestorePasswordAsync(loginOrEmail, code, pass))
                    {
                        return Json(new { message = "Пароль успешно изменен!", type = NotifyType.Success });
                    }

                    return Json(new
                    { message = "Ошибка! Проверте правильность введенных данных.", type = NotifyType.Warning });
                }

                return Json(new { message = PasswordMessage, type = NotifyType.Warning });
            }

            return Json(new { message = "Пароли не совпадают!", type = NotifyType.Warning });
        }

        [HttpPost]
        public async Task<JsonResult> RegistrationSendCode(string email)
        {
            var token = Generator.String(EmailTokenLenght);
            var code = await _client.SendCodeOnEmailAsync(email, token);
            if (code != null)
            {
                if (code != "")
                {
                    Response.Cookies.Add(new HttpCookie(NameEmailTokenCookie, token));
                    return Json(new { message = $"Код отправлен на {email.ToLower()}!", type = NotifyType.Success });
                }

                return Json(
                    new { message = $"Аккаунт с почтой {email.ToLower()} уже создан!", type = NotifyType.Warning });
            }

            return Json(new { message = $"Ошибка! Код не отправлен на {email.ToLower()}!", type = NotifyType.Error });
        }

        [HttpPost]
        public async Task<JsonResult> Registration(string login, string password, string repPassword, string email,
            string code)
        {
            if (Regex.IsMatch(login, PatternLogin))
            {
                if (!await _client.LoginExistAsync(login))
                {
                    if (!await _client.EmailExistAsync(email))
                    {
                        if (Regex.IsMatch(password, PatternPassword))
                        {
                            if (password == repPassword)
                            {
                                var cook = Request.Cookies[NameEmailTokenCookie]?.Value;
                                if (cook != null)
                                {
                                    var emailFromDB = await _client.GetEmailByCookieAndCodeAsync(cook, code);
                                    if (emailFromDB != null)
                                    {
                                        if (await _client.RegistrationAsync(new UserWCF
                                        { Email = emailFromDB, Login = login, PasswordHash = password }))
                                        {
                                            var html = RazorViewToStringFormat.RenderRazorViewToString(this,
                                                "PartialLogin", null);
                                            return Json(new
                                            {
                                                message = "Вы зарегистрированы успешно",
                                                type = NotifyType.Success,
                                                html
                                            });
                                        }

                                        return Json(new { message = FatalError, type = NotifyType.Error });
                                    }

                                    return Json(new { message = "Не верный код с почты", type = NotifyType.Warning });
                                }

                                return Json(new { message = "Отправьте код на Вашу почту!", type = NotifyType.Warning });
                            }

                            return Json(new { message = "Пароли не совпадают!", type = NotifyType.Warning });
                        }

                        return Json(new
                        {
                            message = PasswordMessage,
                            type = NotifyType.Warning
                        });
                    }

                    return Json(new
                    { message = "Пользователь с таким email уже зарегистрирован!", type = NotifyType.Warning });
                }

                return Json(new { message = "Логин занят", type = NotifyType.Warning });
            }

            return Json(new
            {
                message = LoginMessage,
                type = NotifyType.Warning
            });
        }

        #endregion
    }
}