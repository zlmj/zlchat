using chatDAL;
using chatModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace chatBLL
{
    public class UserBLL
    {
        public string IsOnLine(User user)
        {
            IList<User> users = UserDAL.Instance.GetUser(user);
            if (users.Count > 0)
            {
                foreach (var item in users)
                {
                    if (item.Status == 0)
                    {
                        return JsonConvert.SerializeObject(item);
                    }
                }
            }
            return null;
        }
    }
}