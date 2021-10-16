using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sample.Data.Entities;

namespace Sample.Data.DbContexts
{
    public class SampleDataDbContext:DbContext
    {
        public SampleDataDbContext([NotNull] DbContextOptions options):base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
