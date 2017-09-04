using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrothersAssistant
{
    public class User
    {
        public string Username { get; set; }
        public int UserID { get; set; }
        public Boolean Active { get; set; }

        public User getUser(string name)
        {
            User localUser = new User();
            //Connect to db

            //Retrieve user

            //Return user

            return localUser;
        }
    }

}
