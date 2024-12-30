using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositeries
{
    public class RepositeryContext:DbContext
    {
        public RepositeryContext(DbContextOptions<RepositeryContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<MasterItem> masterItems { get; set; }
        public DbSet<Detailitem> detailitems { get; set; }

    }
}
