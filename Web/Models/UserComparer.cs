using System.Collections.Generic;
using Web.ServiceReference1;

namespace Web.Controllers
{
    public class UserComparer : IEqualityComparer<UserBaseWCF>
    {
        public bool Equals(UserBaseWCF x, UserBaseWCF y)
        {
            if (x.Id.Equals(y.Id))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(UserBaseWCF obj)
        {
            return obj.Id;
        }
    }
}