using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace rssNews
{
    class ApplicationContext:DbContext
    {
        public DbSet<Source> Sources{ get; set; }
        public DbSet<News> News { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //объявление составного ключа
            modelBuilder.Entity<News>().HasKey(u => new { u.Headline, u.PublicationDate});
            //инициализаця таблицы Source
            modelBuilder.Entity<Source>().HasData(
            new Source[]
            {
                new Source { Id=1, Link="http://www.interfax.by/news/feed/", Name="Interfax"},
                new Source { Id=2, Link="http://habrahabr.ru/rss/", Name="HabraHabr"}
            });
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=NewsDB;Trusted_Connection=True;");
        //}
    }
}
