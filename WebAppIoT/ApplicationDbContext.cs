using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebAppIoT
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Data> Data { get; set; }
    }
    public class Data
    {
        [Key]
        public int Id { get; set; }
        public int GatewayId { get; set; }
        public int SensorId { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool Value { get; set; }
    }
}
