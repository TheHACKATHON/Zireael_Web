using AdditionsLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    // ConfigurationManager.AppSettings.Get("PatternPassword");
    public partial class HomeController : Controller
    {
        private const int MESSAGES_COUNT = 20;
        private const string DefaultAvatar = "/Content/Images/Zireael_back_128px.png";
        private readonly CeadChatServiceClient _client;
        private readonly PrivateFontCollection _fonts = new PrivateFontCollection();

        public HomeController(CeadChatServiceClient client)
        {
            _client = client;
            var file = HostingEnvironment.MapPath("~/Content/Fonts/RobotoMono-Bold.ttf");
            if (file != null)
            {
                _fonts.AddFontFile(file);
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteMessages(string messagesIdJson)
        {
            var messagesArray = JsonConvert.DeserializeObject<int[]>(messagesIdJson);
            if (messagesArray != null)
            {
                foreach (var id in messagesArray)
                {
                    if (!await _client.DeleteMessageAsync(id))
                    {
                        return Json(new NotifyError("Произошла ошибка, обновите страницу или попробуйте позже"));
                    }
                }

                return Json(new { Code = NotifyType.Success });
            }

            return Json(new NotifyError("Произошла ошибка, обновите страницу или попробуйте позже"));
        }

        [HttpPost]
        public async Task<JsonResult> GetMessages(int? groupId)
        {
            if (groupId != null)
            {
                var messages = await _client.GetMessagesBetweenAsync((int)groupId, 0, MESSAGES_COUNT);
                if (messages != null)
                {
                    for (int i = 0; i < messages.Count(); i++)
                    {
                        if (messages[i] is MessageFileWCF messageFile)
                        {
                            messages[i] = messageFile;
                            messageFile.File.Hash = SizeSuffix(messageFile.File.Lenght);
                        }
                    }
                    messages = messages.OrderBy(m => m.DateTime).ToArray();
                    return Json(new { Code = NotifyType.Success, messages, groupId });
                }
            }

            return Json(new NotifyError("Чат не найден, обновите страницу или попробуйте позже"));
        }

        [HttpPost]
        public async Task<JsonResult> GetOldMessages(int? groupId, int? lastMessageId)
        {
            if (groupId != null && lastMessageId != null)
            {
                var messages = await _client.GetMessagesAfterAsync((int)groupId, (int)lastMessageId, MESSAGES_COUNT);
                if (messages != null)
                {
                    messages = messages.OrderByDescending(m => m.DateTime).ToArray();
                    return Json(new { Code = NotifyType.Success, messages, groupId });
                }
            }
            return Json(new { Code = NotifyType.Success, messages = new List<MessageWCF>() });
        }

        [HttpPost]
        public async Task ReadMessages(int? groupId)
        {
            if (groupId != null)
            {
                await _client.ReadAllMessagesInGroupAsync((int)groupId);
            }
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(string text, int groupId, long hash)
        {
            var msg = new MessageWCF
            {
                Text = text,
                GroupId = groupId,
                DateTime = DateTime.Now
            };

            var messageId = await _client.SendMessageTransactionAsync(msg, hash);
            if (messageId != -1)
            {
                return Json(new { Code = NotifyType.Success });
            }

            return Json(new NotifyError("Не удалось отправить сообщение"));
        }

        [HttpPost]
        public async Task<JsonResult> Logout()
        {
            if (await _client.LogOutAsync())
            {
                return Json(true);
            }

            return Json(false);
        }

        [HttpGet]
        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client)]
        public async Task<ActionResult> UserImage(int userId = 0, string hash = "", int number = 0)
        {
            if (number > 1000) number = 0;
            var avatar = await GetAvatar(true, userId, hash, number);
            if (avatar.BigData != null)
            {
                return File(avatar.BigData, "image/png");
            }

            return HttpNotFound();
        }

        [HttpGet]
        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client)]
        public async Task<ActionResult> GroupImage(int groupId = 0, string hash = "", int number = 0)
        {
            var avatar = await GetAvatar(false, groupId, hash, number);
            if (avatar.BigData != null)
            {
                return File(avatar.BigData, "image/png");
            }

            return HttpNotFound();
        }

        private async Task<AvatarWCF> GetAvatar(bool isUserImage, int id, string hash, int avatarNumber = 0)
        {
            var avatar = isUserImage
                ? (await _client.GetAvatarUserAsync(id, avatarNumber))
                : (await _client.GetAvatarGroupsAsync(new[] { id }))?.SingleOrDefault();

            if (avatar is null)
            {
                string username = null;
                if (isUserImage)
                {
                    username = await _client.GetNameAsync(id);
                    if (username is null) return null;
                    avatar = new AvatarUserWCF
                    {
                        User = new UserBaseWCF { Id = id, DisplayName = username }
                    };
                }
                else
                {
                    username = await _client.GetGroupNameAsync(id);
                    if (username is null) return null;
                    avatar = new AvatarGroupWCF
                    {
                        Group = new GroupWCF { Id = id, Name = username }
                    };
                }
                
                var path = HostingEnvironment.MapPath(DefaultAvatar);
                if (path != null)
                {
                    using (var bitmap = Image.FromFile(path))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.DrawString(username.Substring(0, 1).ToUpper(), new Font(_fonts.Families[0], 84),
                                Brushes.White, new RectangleF(12, -13, 128, 128),
                                new StringFormat(StringFormatFlags.NoClip));
                        }

                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            avatar.BigData = ms.ToArray();
                        }
                    }
                }
                else
                {
                    return null;
                }
            }


            if (avatar is AvatarGroupWCF groupAvatar)
            {
                if (HashCode.GetMD5(groupAvatar.Group.Name).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return avatar;
                }
            }
            else if (avatar is AvatarUserWCF userAvatar)
            {
                if (HashCode.GetMD5(userAvatar.User.DisplayName).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return avatar;
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (Request.Cookies["SessionId"] != null)
            {
                var user = await _client.CheckSessionAsync($"{Request.Cookies["SessionId"].Value}");
                if (user != null)
                {
                    ViewBag.DontReaded = _client.GetDontReadMessagesFromGroups(user.Groups.Select(g => g.Id).ToArray());
                    user.Groups.ToList().ForEach(g =>
                        g.Name = g.Type.Equals(GroupType.SingleUser)
                            ? g.Users.SingleOrDefault(u => u.Id != user.Id)?.DisplayName
                            : g.Name);

                    ViewBag.Groups = user.Groups.OrderByDescending(g => g.LastMessage.DateTime);
                    ViewData["myProfile"] = await _client.GetMyProfileAsync();
                    return View("Index");
                }
            }

            return View("Auth");
        }

        [HttpPost]
        public ActionResult AuthAjax(string type = "log")
        {
            string view = null;
            switch (type)
            {
                case "reg":
                    {
                        view = "PartialRegistration";
                    }
                    break;
                case "log":
                    {
                        view = "PartialLogin";
                    }
                    break;
                case "pass":
                    {
                        view = "PartialPassword";
                    }
                    break;
                case "learn":
                    {
                        view = "PartialLearnMore";
                    }
                    break;
                default: goto case "log";
            }

            return PartialView(view);
        }

        [HttpPost]
        public async Task<JsonResult> Login(string login, string password)
        {
            UserWCF user = null;

            user = await _client.LogInAsync(login, password, null);
            if(user != null && user.IsBlocked)
            {
                return Json(new { Code = NotifyType.Error, Error = "Ваш аккаунт заблокирован на 24 часа, попробуйте зайти позже" });
            }
            if (user != null)
            {
                //Session.Add("token", user.Session);
                Response.Cookies.Add(new HttpCookie("SessionId", user.Session));
                Response.Cookies["SessionId"].Expires = DateTime.Now + new TimeSpan(30, 0, 0, 0);
                Clients.Remove(_client);
                Clients.Add(Response.Cookies["SessionId"].Value, _client);
            }

            if (user is null) return Json(new NotifyError("Неверный логин или пароль"));
            return Json(new { Code = NotifyType.Success });
        }

        private readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }

            if (value.Equals(0))
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            var mag = (int)Math.Log(value, 1024);
            var adjustedSize = (decimal)value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }    
}