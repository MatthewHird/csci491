using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestProject.Models;
using TestProject.Areas.MyArea.Models;

namespace TestProject.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {}

        public DbSet<TestProject.Areas.MyArea.Models.MyModel2> MyModel2 { get; set; }

        public DbSet<TestProject.Areas.MyArea.Models.MyModel3> MyModel3 { get; set; }

        public DbSet<TestProject.Areas.MyArea.Models.MyModel1> MyModel1 { get; set; }

        public DbSet<TestProject.Areas.MyArea.Models.MyModel4> MyModel4 { get; set; }

        public DbSet<TestProject.Models.DrSuess> DrSuess { get; set; }

        public DbSet<TestProject.Models.RedFish> RedFish { get; set; }

        public DbSet<TestProject.Models.BlueFish> BlueFish { get; set; }

        public DbSet<TestProject.Models.Thing1> Thing1 { get; set; }

        public DbSet<TestProject.Models.Thing2> Thing2 { get; set; }

    }
}
