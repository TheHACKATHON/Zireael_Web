using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public partial class HomeController : Controller, ICeadChatServiceCallback
    {
        private CeadChatServiceClient _client;
        public HomeController(CeadChatServiceClient client)
        {
            _client = client;

        }
        public async Task<ActionResult> Index()
        {
            if(Session["token"] != null)
            {
                var user = await _client.CheckSessionAsync($"{Session["token"]}");
                if (user != null)
                {
                    ViewBag.A = user.DisplayName;
                    return View("Index");
                }
            }

            return View("Auth");
        }

        public async Task<ActionResult> About()
        {
            ViewBag.SessionId = Session.SessionID;
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
                Session.Add("token", user.Session);
            }
            
            if (user is null) return Json(new { code = "ERROR", error = "Неверный пароль." });
            return Json(new { code = "OK" });
        }

        public async Task<ActionResult> Callback()
        {
            var k = Request.Cookies["3eqew"];
            //Response.Cookies["3eqew"] = 1312312;
            //var msg = new MessageWCF
            //{
            //    Text = "text",
            //    GroupId = 1,
            //    DateTime = DateTime.Now,
            //    Sender = new UserWCF() { Id = 16 }
            //};

            //ViewBag.id = await _client.SendMessageAsync(msg, DateTime.Now.Ticks);
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