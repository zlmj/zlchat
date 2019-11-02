using chatDAL;
using chatModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatBLL
{
    public class RecordBLL
    {
        public IList<Record> GetAll(string system,string maincode,string mainid)
        {
            return RecordDAL.Instance.GetAll(system,maincode,mainid);
        }


        public IList<User> GetUsers(string system, string maincode, string mainid)
        {
            return null;
        }
    }
}
