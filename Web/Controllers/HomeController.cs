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
        private int id = 0;
        private CeadChatServiceClient _client;
        public HomeController(CeadChatServiceClient client)
        {
            _client = client;

        }
        public async Task<ActionResult> Index()
        {
            /*UserWCF user = null;
            if(Session["token"] == null)
            {
                user = await _client.LogInAsync("qwerty14", "QwertyQwerty", null);
                if (user != null)
                {
                    Session.Add("token", _client.InnerChannel.SessionId);
                }
            }
            else
            {
                user = await _client.LogInAsync(null, null, Session["token"].ToString());
                Session["token"] = _client.InnerChannel.SessionId;
            }
            
            if(user != null)
            {
                ViewBag.UserS = user.Token;
            }
            else
            {
                ViewBag.User = new { Login = "null" };
            }
            //id = user.Id;
            ViewBag.SessionId = Session.SessionID;*/
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

        public async Task<JsonResult> Login()
        {


            return Json(null);
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