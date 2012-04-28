using System.Data.Entity;

namespace IDI.Framework.Model
{
    public class User : DbContext
    {
        public string Username { get; set; }
        public string NickName { get; set; }
    }
}