using System.Data.Entity;

namespace IDI.Framework.Model
{
    public class IDIContext : DbContext
    {
        public IDbSet<User> Users { get; set; }
    }
}