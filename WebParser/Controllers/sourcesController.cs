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
    public class sourcesController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: sources
        public ActionResult Index(string searchText, string message, int? i, bool update = false, int id = 0)
        {
            string messageType = null;
            if (update && db.sources.Count((a) => a.id == id) == 1)
            {
                sources targetSource = db.sources.Find(id);
                if (targetSource != null)
                {
                    ViewData["Issue"] = targetSource.journal_issue;
                    ViewData["Volume"] = targetSource.journal_volume;
                    ViewData["Title"] = targetSource.item_title;
                    ViewData["Update"] = update;
                    ViewData["Id"] = id;
                }
            }
            if (message != null)
            {
                if (message == "Произошла ошибка или запись уже есть." || message == "Произошла непредвиденная ошибка." || message == "Произошла непредвиденная ошибка или такой источник уже есть.")
                {
                    messageType = "danger";
                }
                else messageType = "success";
                ViewData["messageType"] = messageType;
                ViewData["message"] = message;
            }
            return View((db.sources.Where(source => source.item_title.StartsWith(searchText) || searchText == null)).ToList().ToPagedList(i ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Create(string titleInput, int issueInput, int volumeInput)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            if (ModelState.IsValid && db.sources.Count((a) => a.item_title == titleInput) == 0)
            {
                int current_id = db.sources.Count();
                while (db.sources.Count((a) => a.id == current_id) == 1) { current_id++; }
                sources newSource = new sources();
                newSource.id = current_id;
                newSource.item_title = titleInput;
                if (issueInput != 0 && volumeInput != 0)
                {
                    newSource.journal_issue = issueInput;
                    newSource.journal_volume = volumeInput;
                }
                db.sources.Add(newSource);
                db.SaveChanges();
                localMessage = $"Запись {titleInput} успешна добавлена.";
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: sources/Edit/5
        public ActionResult Edit(int? id)
        {
            string localMessage = "Произошла непредвиденная ошибка.";
            if (id != null)
            {
                sources targetSource = db.sources.Find(id);
                if (targetSource != null)
                {
                    return RedirectToAction("/Index", new { i = 1, id = id, update = true });
                }
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        [HttpPost]
        public ActionResult Update(string titleInput, int idInput, int issueInput, int volumeInput)
        {
            string localMessage = "Произошла непредвиденная ошибка или такой источник уже есть.";
            if (ModelState.IsValid && db.sources.Count((a) => a.id == idInput) == 1 && db.sources.Count((a) => (a.item_title == titleInput) && (a.journal_issue == issueInput) && (a.journal_volume == volumeInput)) == 0)
            {
                sources targetSource = db.sources.Find(idInput);
                localMessage = $"Запись {targetSource.item_title}; {targetSource.journal_volume}; {targetSource.journal_issue}; успешна изменена на {titleInput}; {volumeInput}; {issueInput}.";
                targetSource.item_title = titleInput;
                targetSource.journal_issue = issueInput;
                targetSource.journal_volume = volumeInput;
                db.Entry(targetSource).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: sources/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            if (id != null)
            {
                sources targetSource = db.sources.Find(id);
                if (targetSource != null)
                {
                    db.sources.Remove(targetSource);
                    db.SaveChanges();
                    localMessage = $"Запись {targetSource.item_title} была успешно удалена.";
                    return RedirectToAction("/Index", new { i = 1, message = localMessage });
                }
            }
            localMessage = "Произошла непредвиденная ошибка.";
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }
    }
}
