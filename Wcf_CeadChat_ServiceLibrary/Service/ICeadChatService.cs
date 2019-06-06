using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Services;

namespace Wcf_CeadChat_ServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(IUserChanged), ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign, SessionMode =SessionMode.Required)]
    public interface ICeadChatService
    {
        #region WPF Client methods
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        UserWCF Connect(string sessionId, string connectionId);

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        UserWCF CheckSession(string session);
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        //[WebGet(UriTemplate = "/get-books", ResponseFormat = WebMessageFormat.Json)]
        bool Registration(UserWCF newUser);//получаем нового юзера для регистрации, возвращает true, если успешно

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool LoginExist(string login);//проверка, есть ли пользователь с таким логином
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool EmailExist(string email);//проверка, есть ли пользователь с таким email
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        UserWCF LogIn(string login, string password, string token);//вход в учетку. Получаем данные юзера у которого совпадаю логин и пароль

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool CreateGroup(IEnumerable<UserBaseWCF> users, string nameGroup);//создание групы (где >2 пользователей)

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool CreateChat(UserBaseWCF user);//создание чата (только 2 пользователя)

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        UserBaseWCF Find(string login);//получить совпадение по логину

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<UserBaseWCF> FindAll(string login, int count);//получить совпадение по логину из всех пользователей

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        int SendMessage(MessageWCF message, long hash);//отправка сообщения

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool DeleteMessage(MessageWCF message);//удаление сообщения

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool AddFriendsToGroup(GroupWCF group, IEnumerable<UserBaseWCF> friend);//добавление пользователя в чат

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool AddFriend(string friend);//добавление пользователя в друзья

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool RemoveFriend(UserBaseWCF friend);//удаление пользователя с друзей

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<MessageWCF> GetMessagesFromGroup(GroupWCF group);//получить сообщения с группы

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool RemoveGroup(GroupWCF group);//удалить группу

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        Package GetPackageFromFile(int messageId, int packNumber);//получить данные из файла

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SendPackageToFile(int messageId, Package package);//получить данные из файла

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<UserBaseWCF> GetFriends();//получить контактов

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SetAvatarUser(AvatarUserWCF avatar);//установка аватара

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<AvatarUserWCF> GetAvatarUsers(IEnumerable<int> users);//получение аватара пользователей

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        Dictionary<int, int> GetDontReadMessagesFromGroups(IEnumerable<int> groupsId);//получение количество не прочитаных сообщений для отправляющего запрос

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool ReadAllMessagesInGroup(int groupId);//прочитать все сообщения в группе для отправляющего

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool ReadAllMessagesInAllGroups();//прочитать все сообщения во всех группах для отправляющего

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SetAvatarGroup(AvatarGroupWCF avatar);//установить аватарку группе

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<AvatarWCF> GetAvatarGroups(IEnumerable<int> groups);//получить аватарки групп

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool ChangeProfileInfo(string displayName, string login);//изменить параметры профиля(в не нужные параметры передавать null)

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool ChangePassword(string newPassword, string oldPassword);//изменить пароль

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool LogOut();//изменить пароль

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<MessageWCF> GetMessagesBetween(int groupId,int startIdx, int count);//получить сообщения в диапазоне

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool ChangeTextInMessage(MessageWCF message);//изменить сообщения

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SendCodeForRestorePassword(string loginOrEmail);//отправить код подтверждения смены пароля на почту. Возвращает true если успешно отправлено

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool RestorePassword(string loginOrEmail, string recoveryCode, string newPassword);//применить код подтверждения (работает 1 час). Возвращает true если успешно отправлено

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SendCodeForSetNewEmail(string newEmail, string password);//отправить код подтверждения, смены почты, на почту. Возвращает true если успешно отправлено

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool SetNewEmail(string recoveryCode);//применить код подтверждения (работает 1 час). Возвращает true если успешно отправлено

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool RemoveOrExitFromGroup(int groupId, int userIdForRemove);//удаление пользователя с группы(если вызывающий создатель) или выйти с группы если вызывающий учасник

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool AddToBlackList(UserBaseWCF user);//добавление пользователя в черный список

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        bool RemoveFromBlackList(UserBaseWCF userIdForRemove);//удаление пользователя с черного список

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        IEnumerable<UserBaseWCF> GetBlackList();//получить черный список
        #endregion

        #region WEB Client
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        string SendCodeOnEmail(string email, string cookie);//отправка сообщения на почту
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        string GetEmailByCookieAndCode(string cookie, string code);//пролучить почту, если кук и кода совпадают. Иначе возвращает null
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        string GetName(int id);
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        string GetGroupName(int id);

        #endregion
    }

    public interface IUserChanged
    {
        #region WPF Client callbacks
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void CreateChatCallback(GroupWCF group, int creatorId, string sessionId);//оповещение о добавлении пользователя в чат
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void CreateMessageCallback(MessageWCF message, long hash, string sessionId);//оповещение о добавлении сообщения
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void DeleteMessageCallback(MessageWCF message, string sessionId);//оповещение о удалении сообщения
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void NewLastMessageCallback(MessageWCF message, string sessionId);//оповещение о обновлении последнего сообщения
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group, string sessionId);//оповещение о добавлении в группу
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void RemoveGroupCallback(GroupWCF group, string sessionId);//оповещение о удалении группы
        //[OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        //void CreateMessageFileCallback(MessageFileWCF messageFile);//оповещение о добавлении файлового сообщения
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void SetAvatarCallback(AvatarWCF avatar, UserBaseWCF user);//оповещение о установки аватарки для пользователя
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void SetAvatarForGroupCallback(AvatarWCF avatar, GroupWCF group);//оповещение о установки аватарки для пользователя
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender, string sessionId);//оповещение о прочитке сообщений в группе
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void SendedPackageCallback(int msgId, int numberPackage);//оповещение о приеме пакета
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void ChangeOnlineStatusCallback(UserBaseWCF user, string sessionId);//оповещение о изменения статуса онлайн
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void IsOnlineCallback(string sessionId);//проверка статуса онлайн
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void ChangeTextInMessageCallback(MessageWCF message, string sessionId);//оповещение о изменении текста сообщения
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user, string sessionId);//оповещение о исключении/подидании группы
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void LogOutCallback(string sessionId);//оповещение для выхода с учетки
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void AddUserToBlackListCallback(UserBaseWCF user, string sessionId);
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void RemoveUserFromBlackListCallback(UserBaseWCF user, string sessionId);
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void AddContactCallback(UserBaseWCF user, string sessionId);
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign)]
        void RemoveContactCallback(UserBaseWCF user, string sessionId);
        #endregion
    }
}
