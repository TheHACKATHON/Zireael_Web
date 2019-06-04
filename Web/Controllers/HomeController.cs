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
                    ViewBag.Groups = user.Groups;
                    return View("Index");
                }
            }

            return View("Auth");
        }

        public async Task<ActionResult> About()
        {
            ViewBag.SessionId = Request.Cookies["SessionId"].Value;
            return View();
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

        public async Task<ActionResult> Callback()
        {
            return View();
        }

        public async Task<ActionResult> Auth()
        {
            // код
            return View("Auth");
        }


        #region callback
        public void CreateChatCallback(GroupWCF group, int creatorId)
        {
            //throw new NotImplementedException();
        }

        public void CreateMessageCallback(MessageWCF message, long hash)
        {
            //throw new NotImplementedException();
        }

        public void DeleteMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void NewLastMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group)
        {
            //throw new NotImplementedException();
        }

        public void RemoveGroupCallback(GroupWCF group)
        {
            //throw new NotImplementedException();
        }

        public void SetAvatarCallback(AvatarWCF avatar, UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void SetAvatarForGroupCallback(AvatarWCF avatar, GroupWCF group)
        {
            //throw new NotImplementedException();
        }

        public void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender)
        {
            //throw new NotImplementedException();
        }

        public void SendedPackageCallback(int msgId, int numberPackage)
        {
            //throw new NotImplementedException();
        }

        public void ChangeOnlineStatusCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void IsOnlineCallback()
        {
            //throw new NotImplementedException();
        }

        public void ChangeTextInMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void LogOutCallback()
        {
            //throw new NotImplementedException();
        }

        public void AddUserToBlackListCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void RemoveUserFromBlackListCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void AddContactCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void RemoveContactCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}