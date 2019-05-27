namespace Wcf_CeadChat_ServiceLibrary
{
    public class AvatarUser : Avatar
    {
        virtual public UserBase User { get; set; }
        public AvatarUser()
        {

        }
        public AvatarUser(AvatarUserWCF avatar, UserBase user)
        {
            SmallData = avatar.SmallData;
            BigData = avatar.BigData;
            Format = avatar.Format;
            DateTime = avatar.DateTime;
            User = user;
        }
    }

}
