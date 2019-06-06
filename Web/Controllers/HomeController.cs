using AdditionsLibrary;
using System;
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
    public partial class HomeController : Controller
    {
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
        public async Task<JsonResult> GetMessages(int? groupId)
        {
            if (groupId != null)
            {
                var messages = await _client.GetMessagesBetweenAsync((int)groupId, 0, 20);
                if (messages != null)
                {
                    messages = messages.OrderBy(m => m.DateTime).ToArray();
                    return Json(new { Code = NotifyType.Success, messages, groupId });
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
        [OutputCache(Duration = 1800, Location = OutputCacheLocation.Downstream)]
        public async Task<ActionResult> UserImage(int userId = 0, string hash = "")
        {
            var avatar = (await _client.GetAvatarUsersAsync(new[] { userId }))?.SingleOrDefault();
            if (avatar is null)
            {
                var username = await _client.GetNameAsync(userId);
                if (username is null) return HttpNotFound();
                avatar = new AvatarUserWCF
                {
                    User = new UserBaseWCF { Id = userId, DisplayName = username }
                };
                using (var bitmap = Image.FromFile(HostingEnvironment.MapPath(DefaultAvatar)))
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
                if (HashCode.GetMD5(avatar.User.DisplayName).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return File(avatar.BigData, "image/png");
                }
            }
            return HttpNotFound();
        }
        [HttpGet]
        [OutputCache(Duration = 1800, Location = OutputCacheLocation.Downstream)]
        public async Task<ActionResult> GroupImage(int groupId = 0, string hash = "")
        {
            var avatar = (await _client.GetAvatarGroupsAsync(new[] { groupId }))?.SingleOrDefault();
            if (avatar is null)
            {
                var username = await _client.GetGroupNameAsync(groupId);
                if (username is null) return HttpNotFound();
                avatar = new AvatarGroupWCF
                {
                    Group = new GroupWCF { Id = groupId, Name = username }
                };
                using (var bitmap = Image.FromFile(HostingEnvironment.MapPath(DefaultAvatar)))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        g.DrawString(username.Substring(0, 1).ToUpper(), new Font(_fonts.Families[0], 84), Brushes.White, new RectangleF(12, -13, 128, 128), new StringFormat(StringFormatFlags.NoClip));
                    }
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        avatar.BigData = ms.ToArray();
                    }
                }
            }
            if (avatar is AvatarGroupWCF groupAvatar)
            {
                if (HashCode.GetMD5(groupAvatar.Group.Name).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return File(avatar.BigData, "image/png");
                }
            }
            else if (avatar is AvatarUserWCF userAvatar)
            {
                if (HashCode.GetMD5(userAvatar.User.DisplayName).Equals(hash, StringComparison.OrdinalIgnoreCase))
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
                    ViewBag.DontReaded = _client.GetDontReadMessagesFromGroups(user.Groups.Select(g => g.Id).ToArray());
                    user.Groups.ToList().ForEach(g =>
                        g.Name = g.Type.Equals(GroupType.SingleUser)
                        ? g.Users.SingleOrDefault(u => u.Id != user.Id)?.DisplayName
                        : g.Name);

                    ViewBag.Groups = user.Groups.OrderByDescending(g => g.LastMessage.DateTime);

                    return View("Index");
                }
            }

            return View("Auth");
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
            return Json(new { Code = NotifyType.Success });
        }
    }
}