using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public partial class HomeController : Controller
    {
        private string _defaultAvatar = "/Content/Images/Zireael_back.png";
        private CeadChatServiceClient _client;
        public HomeController(CeadChatServiceClient client)
        {
            _client = client;

        }
        [HttpPost]
        public async Task<JsonResult> GetMessages(int groupId)
        {
            var messages = await _client.GetMessagesBetweenAsync(groupId, 0, 10);

            var avatarsDictionary = new List<object>();

            var avatars = _client.GetAvatarUsers( messages.Select(m => m.Sender).Distinct(new UserComparer()).ToArray() );
            foreach (var user in messages.Select(m => m.Sender).Distinct(new UserComparer()))
            {
                if (avatars.Any(a => a.User.Id.Equals(user.Id)))
                {
                    avatarsDictionary.Add(new { userId = user.Id, avatar = $"/user/{user.Id}" });
                }
                else {
                    avatarsDictionary.Add(new { userId = user.Id, avatar = _defaultAvatar });
                }
            }
            return Json(new { messages, avatarsDictionary, _defaultAvatar});
        }

        public async Task<JsonResult> Logout()
        {
            if(await _client.LogOutAsync())
            {
                return Json(true);
            }
            return Json(false);
        }

        public ActionResult UserImage(int userId = 0, int id = 1)
        {
            var avatar = _client.GetAvatarUsers(new[] { new UserBaseWCF { Id = userId } }).SingleOrDefault();
            if (avatar is null) throw new HttpException(404, "Файл не найден");
            return File(avatar.BigData, "image/png");
        }

        public async Task<ActionResult> Index()
        {
            if (Request.Cookies["SessionId"] != null)
            {
                var user = await _client.CheckSessionAsync($"{Request.Cookies["SessionId"].Value}");
                if (user != null)
                {
                    var avatars = new Dictionary<int, string>();
                    foreach (var group in user.Groups)
                    {
                        var stringAvatar = string.Empty;
                        var userType = string.Empty;
                        var id = -1;
                        AvatarWCF avatar = null;
                        if (group.Type.Equals(GroupType.SingleUser))
                        {
                            var userNoNeEtot = group.Users.SingleOrDefault(u => u.Id != user.Id);
                            group.Name = userNoNeEtot.DisplayName;
                            avatar = _client.GetAvatarUsers(new[] { new UserBaseWCF { Id = userNoNeEtot.Id } }).SingleOrDefault();
                            userType = "user";
                            id = userNoNeEtot.Id;

                        }
                        else
                        {
                            avatar = _client.GetAvatarGroups(new[] { new GroupWCF{ Id = group.Id } }).SingleOrDefault();
                            userType = "group";
                            id = group.Id;
                        }

                        if (avatar is null)
                        {
                            stringAvatar = _defaultAvatar;
                        }
                        else
                        {
                            stringAvatar = $"/{userType}/{id}";
                        }
                        avatars.Add(group.Id, stringAvatar);
                    }

                    ViewBag.DefaultAvatar = _defaultAvatar;
                    ViewBag.Avatars = avatars;
                    ViewBag.DontReaded = _client.GetDontReadMessagesFromGroups(user.Groups.Select(g => g.Id).ToArray());
                    ViewBag.Groups = user.Groups.OrderByDescending(g => g.LastMessage.DateTime);
                    return View("Index");
                }
            }

            return View("Auth");
        }

        public async Task<JsonResult> GetAvatars(int[] ids)
        {
            // todo получение ссылок на аватары
            // формат: { Id, Path }

            return Json(null);
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
            
            if (user is null) return Json(new { code = NotifyType.Error.ToString(), error = "Неверный логин или пароль" });
            return Json(new { code = NotifyType.Success.ToString() });
        }
    }
}