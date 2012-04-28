using System.Data.Entity;

namespace IDI.Framework.Model
{
    public class TrainData : DbContext
    {
        public byte[] Image { get; set; }
        public User User { get; set; }
    }
}