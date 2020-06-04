using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebParser;
using PagedList.Mvc;
using PagedList;
using Microsoft.Ajax.Utilities;

namespace WebParser.Controllers
{

    public class MultiController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();


        [HttpGet]
        public ActionResult Index(string message)
        {
            string messageType = null;
            if (message != null)
            {
                if (message == "Произошла ошибка или запись уже есть.")
                {
                    messageType = "danger";
                }
                else messageType = "success";
                ViewData["messageType"] = messageType;
                ViewData["message"] = message;
            }

            var publications = from pub in db.publications
                               orderby pub.title
                               select pub.title;
            var source = from s in db.sources
                         orderby s.item_title
                         select s.item_title;
            var type = from t in db.types
                       orderby t.type_name
                       select t.type_name;
            var authors = from au in db.authors
                          orderby au.initials
                          select au.initials;
            var keywords = from k in db.keywords
                           orderby k.keyword
                           select k.keyword;
            ViewBag.publications = publications;
            ViewBag.authors = authors;
            ViewBag.keywords = keywords;
            ViewBag.type = type;
            ViewBag.source = source;
            return View();
        }

        [HttpPost]
        public ActionResult CreateAP(string Authors, string[] Publications)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            string Publications2 = Publications[0];
            authors a = db.authors.Where(x => x.initials == Authors).FirstOrDefault();
            publications p = db.publications.Where(x => x.title == Publications2).FirstOrDefault();
            if (a != null && p != null && !p.authors.Contains(a))
            {
                a.publications.Add(p);
                p.authors.Add(a);
                db.SaveChanges();
                localMessage = $"Связь между автором: {a.initials} и публикацией: {p.title} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { message = localMessage });
        }

        [HttpPost]
        public ActionResult CreatePS(string Sources, string Publications)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";

            sources a = db.sources.Where(x => x.item_title == Sources).FirstOrDefault();
            publications p = db.publications.Where(x => x.title == Publications).FirstOrDefault();
            if (a != null && p != null && !p.sources.Contains(a))
            {
                a.publications.Add(p);
                p.sources.Add(a);
                db.SaveChanges();
                localMessage = $"Связь между источником: {a.item_title} и публикацией: {p.title} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { message = localMessage });
        }

        [HttpPost]
        public ActionResult CreatePK(string Keywords, string Publications)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";

            keywords a = db.keywords.Where(x => x.keyword == Keywords).FirstOrDefault();
            publications p = db.publications.Where(x => x.title == Publications).FirstOrDefault();
            if (a != null && p != null && !p.keywords.Contains(a))
            {
                a.publications.Add(p);
                p.keywords.Add(a);
                db.SaveChanges();
                localMessage = $"Связь между словом: {a.keyword} и публикацией: {p.title} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { message = localMessage });
        }

        [HttpPost]
        public ActionResult CreatePT(string Type, string Publications)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";

            types a = db.types.Where(x => x.type_name == Type).FirstOrDefault();
            publications p = db.publications.Where(x => x.title == Publications).FirstOrDefault();
            if (a != null && p != null && !p.types.Contains(a))
            {
                a.publications.Add(p);
                p.types.Add(a);
                db.SaveChanges();
                localMessage = $"Связь между типом: {a.type_name} и публикацией: {p.title} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { message = localMessage });
        }
    }
}