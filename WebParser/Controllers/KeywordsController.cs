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
    public class KeywordsController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: Keywords
        [HttpGet]
        public ActionResult Index(string searchText, string message, int? i, bool update = false, int id = 0)
        {

            string messageType = null;
            if (update && db.keywords.Count((a) => a.id == id) == 1)
            {
                keywords targetKeyword = db.keywords.Find(id);
                if (targetKeyword != null)
                {
                    ViewData["Word"] = targetKeyword.keyword;
                    ViewData["Update"] = update;
                    ViewData["Id"] = id;
                }
            }
            if (message != null)
            {
                if (message == "Произошла ошибка или запись уже есть." || message == "Произошла непредвиденная ошибка." || message == "Произошла непредвиденная ошибка или такое слово уже есть.")
                {
                    messageType = "danger";
                }
                else messageType = "success";
                ViewData["messageType"] = messageType;
                ViewData["message"] = message;
            }
            return View((db.keywords.Where(word => word.keyword.StartsWith(searchText) || searchText == null)).ToList().ToPagedList(i ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Create(string keywordInput)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            if (ModelState.IsValid && db.keywords.Count((a) => a.keyword == keywordInput) == 0)
            {
                int current_id = db.keywords.Count();
                while (db.keywords.Count((a) => a.id == current_id) == 1) { current_id++; }
                keywords newWord = new keywords();
                newWord.id = current_id;
                newWord.keyword = keywordInput;
                db.keywords.Add(newWord);
                db.SaveChanges();
                localMessage = $"Запись {keywordInput} успешна добавлена.";
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Keywords/Edit/5
        public ActionResult Edit(int? id)
        {
            string localMessage = "Произошла непредвиденная ошибка.";
            if (id != null)
            {
                keywords targetWord = db.keywords.Find(id);
                if (targetWord != null)
                {
                    return RedirectToAction("/Index", new { i = 1, id = id, update = true });
                }
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        [HttpPost]
        public ActionResult Update(string keywordInput, int idInput)
        {
            string localMessage = "Произошла непредвиденная ошибка или такое слово уже есть.";
            if (ModelState.IsValid && db.keywords.Count((a) => a.id == idInput) == 1 && db.keywords.Count((a) => a.keyword == keywordInput) == 0)
            {
                keywords targetWord = db.keywords.Find(idInput);
                localMessage = $"Запись {targetWord.keyword} успешна изменена на {keywordInput}.";
                targetWord.keyword = keywordInput;
                db.Entry(targetWord).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Keywords/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            if (id != null)
            {
                keywords targetWord = db.keywords.Find(id);
                if (targetWord != null)
                {
                    db.keywords.Remove(targetWord);
                    db.SaveChanges();
                    localMessage = $"Запись {targetWord.keyword} была успешно удалена.";
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
