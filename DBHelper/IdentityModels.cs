using LimLink_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LimLink_API.DBHelper
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string AccountType { get; set; }
    }

    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<HistoryLinks> HistoryLinks { get; set; }
        public DbSet<UserLinks> UserLinks { get; set; }
        public DbSet<PaymentHistory> PaymentHistory { get; set; }
        public DbSet<AccountSetting> AccountSetting { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
