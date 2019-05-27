using System.Runtime.Serialization;

namespace Wcf_CeadChat_ServiceLibrary
{
    [DataContract]
    public enum GroupType
    {
        [EnumMember]
        SingleUser,
        [EnumMember]
        MultyUser,
        [EnumMember]
        Channel
    }
}
