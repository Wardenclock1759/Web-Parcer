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

namespace WebParser.Controllers
{
    public class PublicationsController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: Publications
        [HttpGet]
        public ActionResult Index(string searchText, string message, int? i, bool update = false, int id = 0)
        {
            string messageType = null;
            if (update && db.publications.Count((a) => a.id == id) == 1)
            {
                publications targetPublication = db.publications.Find(id);
                if (targetPublication != null)
                {
                    ViewData["Title"] = targetPublication.title;
                    ViewData["Year"] = targetPublication.year;
                    ViewData["Update"] = update;
                    ViewData["Id"] = id;
                }
            }
            if (message != null)
            {
                if (message == "Произошла ошибка или запись уже есть." || message == "Произошла непредвиденная ошибка." || message == "Произошла непредвиденная ошибка или такая публикация уже есть.")
                {
                    messageType = "danger";
                }
                else messageType = "success";
                ViewData["messageType"] = messageType;
                ViewData["message"] = message;
            }
            return View((db.publications.Where(publications => publications.title.StartsWith(searchText) || searchText == null)).ToList().ToPagedList(i ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Create(string titleInput, int yearInput)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            if (ModelState.IsValid && db.publications.Count((a) => a.title == titleInput) == 0)
            {
                int current_id = db.publications.Count();
                while (db.publications.Count((a) => a.id == current_id) == 1) { current_id++; }
                publications newPublication = new publications();
                newPublication.id = current_id;
                newPublication.title = titleInput;
                newPublication.year = Convert.ToInt16(yearInput);
                db.publications.Add(newPublication);
                db.SaveChanges();
                localMessage = $"Запись {titleInput} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Publications/Edit/5
        public ActionResult Edit(int? id)
        {
            string localMessage = "Произошла непредвиденная ошибка.";
            if (id != null)
            {
                publications targetPublication = db.publications.Find(id);
                if (targetPublication != null)
                {
                    return RedirectToAction("/Index", new { i = 1, id = id, update = true });
                }
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        [HttpPost]
        public ActionResult Update(string titleInput, int idInput, int yearInput)
        {
            string localMessage = "Произошла непредвиденная ошибка или такая публикация уже есть.";
            if (ModelState.IsValid && db.publications.Count((a) => a.id == idInput) == 1 && db.publications.Count((a) => (a.title == titleInput) && (a.year == yearInput)) == 0)
            {
                publications targetPublication = db.publications.Find(idInput);
                localMessage = $"Запись {targetPublication.title} успешно изменена на {titleInput}.";
                targetPublication.title = titleInput;
                db.Entry(targetPublication).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Publications/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            if (id != null)
            {
                publications targetPublication = db.publications.Find(id);
                if (targetPublication != null)
                {
                    db.publications.Remove(targetPublication);
                    db.SaveChanges();
                    localMessage = $"Запись {targetPublication.title} была успешно удалена.";
                    return RedirectToAction("/Index", new { i = 1, message = localMessage });
                }
            }
            localMessage = "Произошла непредвиденная ошибка.";
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
