using AdditionsLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public partial class HomeController : Controller
    {
        private string _defaultAvatar = "/Content/Images/Zireael_back_128px.png";
        private readonly CeadChatServiceClient _client;
        private readonly PrivateFontCollection _fonts = new PrivateFontCollection();
        public HomeController(CeadChatServiceClient client)
        {
            _client = client;
            var file = HostingEnvironment.MapPath("~/Content/Fonts/RobotoMono-Regular.ttf");
            _fonts.AddFontFile(file);
        }
        [HttpPost]
        public async Task<JsonResult> GetMessages(int? groupId)
        {
            if (groupId != null)
            {
                var messages = await _client.GetMessagesBetweenAsync((int)groupId, 0, 10);

                if (messages != null)
                {
                    var avatarsDictionary = new List<object>();

                    var avatars = _client.GetAvatarUsers(messages.Select(m => m.Sender).Distinct(new UserComparer()).ToArray());
                    foreach (var user in messages.Select(m => m.Sender).Distinct(new UserComparer()))
                    {
                        if (avatars.Any(a => a.User.Id.Equals(user.Id)))
                        {
                            avatarsDictionary.Add(new { userId = user.Id, avatar = $"/user/{user.Id}" });
                        }
                        else
                        {
                            avatarsDictionary.Add(new { userId = user.Id, avatar = _defaultAvatar });
                        }
                    }
                    messages = messages.OrderBy(m => m.DateTime).ToArray();
                    return Json(new { Code = NotifyType.Success, messages, groupId, avatarsDictionary, _defaultAvatar });
                }
            }
            return Json(new NotifyError("Чат не найден, обновите страницу или попробуйте позже"));

        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(string text, int groupId, int hash)
        {
            var msg = new MessageWCF
            {
                Text = text,
                GroupId = groupId,
                DateTime = DateTime.Now,
            };

            var messageId = await _client.SendMessageAsync(msg, hash);
            if(messageId != -1)
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
        public async Task<ActionResult> UserImage(int userId = 0, string hash = "", int id = 1)
        {
            var avatar = (await _client.GetAvatarUsersAsync(new[] { new UserBaseWCF { Id = userId } }))?.SingleOrDefault();
            if (avatar is null)
            {
                var username = await _client.GetNameAsync(userId);
                if (username is null) return HttpNotFound();
                avatar = new AvatarUserWCF
                {
                    User = new UserBaseWCF { Id = userId, DisplayName = username }
                };
                using (var bitmap = Image.FromFile(HostingEnvironment.MapPath(_defaultAvatar)))
                {
                    using(var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawString(username.Substring(0,1).ToUpper(), new Font(_fonts.Families[0], 90), Brushes.White, new RectangleF(7,-20,128,128), new StringFormat(StringFormatFlags.NoClip));
                    }
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        avatar.BigData = ms.ToArray();
                    }
                }
            }
            if (avatar != null)
            {
                //if(HashCode.GetMD5(avatar.User.DisplayName).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return File(avatar.BigData, "image/png");
                }
            }
            return HttpNotFound();
        }
        [HttpGet]
        public async Task<ActionResult> GroupImage(int groupId = 0, string hash = "", int id = 1)
        {
            var avatar = (await _client.GetAvatarGroupsAsync(new[] { new GroupWCF { Id = groupId } }))?.SingleOrDefault();
            if (avatar is null)
            {
                var username = await _client.GetGroupNameAsync(groupId);
                if (username is null) return HttpNotFound();
                avatar = new AvatarGroupWCF
                {
                    Group = new GroupWCF{ Id = groupId, Name = username }
                };
                using (var bitmap = Image.FromFile(HostingEnvironment.MapPath(_defaultAvatar)))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawString(username.Substring(0, 1).ToUpper(), new Font(_fonts.Families[0], 90), Brushes.White, new RectangleF(7, -20, 128, 128), new StringFormat(StringFormatFlags.NoClip));
                    }
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        avatar.BigData = ms.ToArray();
                    }
                }
            }
            if (avatar != null)
            {
               // if (HashCode.GetMD5(avatar.Group.Name).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return File(avatar.BigData, "image/png");
                }
            }
            return HttpNotFound();
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (Request.Cookies["SessionId"] != null)
            {
                var user = await _client.CheckSessionAsync($"{Request.Cookies["SessionId"].Value}");
                if (user != null)
                {
                    //var avatars = new Dictionary<int, string>();
                    //foreach (var group in user.Groups)
                    //{
                    //    var stringAvatar = string.Empty;
                    //    var userType = string.Empty;
                    //    var id = -1;
                    //    AvatarWCF avatar = null;
                    //    if (group.Type.Equals(GroupType.SingleUser))
                    //    {
                    //        var userNoNeEtot = group.Users.SingleOrDefault(u => u.Id != user.Id);
                    //        group.Name = userNoNeEtot.DisplayName;
                    //        avatar = _client.GetAvatarUsers(new[] { new UserBaseWCF { Id = userNoNeEtot.Id } }).SingleOrDefault();
                    //        userType = "user";
                    //        id = userNoNeEtot.Id;

                    //    }
                    //    else
                    //    {
                    //        avatar = _client.GetAvatarGroups(new[] { new GroupWCF { Id = group.Id } }).SingleOrDefault();
                    //        userType = "group";
                    //        id = group.Id;
                    //    }

                    //    if (avatar is null)
                    //    {
                    //        stringAvatar = _defaultAvatar;
                    //    }
                    //    else
                    //    {
                    //        stringAvatar = $"/{userType}/{id}";
                    //    }
                    //    avatars.Add(group.Id, stringAvatar);
                    //}

                    //ViewBag.DefaultAvatar = _defaultAvatar;
                    //ViewBag.Avatars = avatars;
                    ViewBag.DontReaded = _client.GetDontReadMessagesFromGroups(user.Groups.Select(g => g.Id).ToArray());
                    ViewBag.Groups = user.Groups.OrderByDescending(g => g.LastMessage.DateTime);
                    return View("Index");
                }
            }

            return View("Auth");
        }
        [HttpPost]
        public async Task<JsonResult> GetAvatars(int[] ids)
        {
            // todo получение ссылок на аватары
            // формат: { Id, Path }
            var users = new List<UserWCF>();
            foreach (var id in ids)
            {
                users.Add(new UserWCF() { Id = id });
            }
            var avatars = await _client.GetAvatarUsersAsync(users.ToArray());
            var result = new List<object>();
            foreach (var userId in ids)
            {
                AvatarWCF avatar = null;
                if((avatar = avatars.FirstOrDefault(a => a.User.Id.Equals(userId))) != null)
                {
                    result.Add(new { Id = userId, Path = $"/user/{userId}" });
                }
                else
                {
                    result.Add(new { Id = userId, Path = _defaultAvatar });
                }
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> AuthAjax(string type = "log")
        {
            var view = string.Empty;
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
            if (user != null)
            {
                //Session.Add("token", user.Session);
                Response.Cookies.Add(new HttpCookie("SessionId", user.Session));
                Response.Cookies["SessionId"].Expires = DateTime.Now + new TimeSpan(30, 0, 0, 0);
                Clients.Remove(_client);
                Clients.Add(Response.Cookies["SessionId"].Value, _client);
            }

            if (user is null) return Json(new NotifyError("Неверный логин или пароль"));
            return Json(new { code = NotifyType.Success });
        }
    }
}