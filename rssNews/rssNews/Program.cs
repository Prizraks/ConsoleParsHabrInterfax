using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Net;


namespace rssNews
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("appsettings.json");
            // создаем конфигурацию
            var config = builder.Build();
            // строка подключения
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var options = optionsBuilder
                .UseSqlServer(connectionString)
                .Options;

            string interfaxUrl = "http://www.interfax.by/news/feed/";
            string habrUrl = "http://habrahabr.ru/rss/";
            Parsing(interfaxUrl, options);
            Parsing(habrUrl, options);

            Console.ReadLine();
        }
        static void Parsing(string url, DbContextOptions<ApplicationContext>options)
        {
            string xmlStr;
            int sourceId = 0;
            XmlReader reader;
            SyndicationFeed feed;
            if (url == "http://www.interfax.by/news/feed/")
            {
                sourceId = 1;
                using (var webClient = new WebClient())
                {
                    xmlStr = webClient.DownloadString(url);
                    xmlStr = xmlStr.Remove(0, 1);
                }
                reader = XmlReader.Create(new StringReader(xmlStr));
                feed = SyndicationFeed.Load(reader);
                reader.Close();
                Console.WriteLine("Interfax: ");
            }
            else
            {
                sourceId = 2;
                reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
                reader.Close();
                Console.WriteLine("------------------------------");
                Console.WriteLine("HabraHabr: ");
            }

            List<News> list = new List<News>();

            using (ApplicationContext db = new ApplicationContext(options))
            {
                foreach (SyndicationItem item in feed.Items)
                {
                    News obj = new News();
                    obj.Headline = item.Title.Text;
                    obj.Description = item.Summary.Text;
                    obj.PublicationDate = item.PublishDate.DateTime;
                    obj.Link = item.Links[0].Uri.OriginalString;
                    obj.SourceId = sourceId;
                    list.Add(obj);
                }
                Console.WriteLine("Прочитано новостей: "+ list.Count);

                var elem = db.News.Where(h=>h.SourceId==sourceId);

                if (elem.Count()!= 0)
                {
                    var element = elem.Max(x => x.PublicationDate);
                    list.RemoveAll(x => x.PublicationDate <= element);
                }
                try
                {
                    db.AddRange(list);
                    db.SaveChanges();
                    Console.WriteLine("Сохранено новостей: " + list.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

            }
        }
    }
}
