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
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;

namespace WebParser.Controllers
{
    public class AuthorController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: Author/Index/
        [HttpGet]
        public ActionResult Index(string searchText, string message, int? i, bool update = false, int id = 0)
        {
            if (searchText != null)
            {
                searchText = searchText.ToLower();
            }
            string messageType = null;
            if (update && db.authors.Count((a) => a.id == id) == 1)
            {
                authors targetAuthor = db.authors.Find(id);
                if (targetAuthor != null)
                {
                    ViewData["Initials"] = targetAuthor.initials;
                    ViewData["Update"] = update;
                    ViewData["Id"] = id;
                }
            }
            if (message != null)
            {
                if (message == "Произошла ошибка или запись уже есть." || message == "Произошла непредвиденная ошибка." || message == "Произошла непредвиденная ошибка или автор с таким именем уже есть.")
                {
                    messageType = "danger";
                }
                else messageType = "success";
                ViewData["messageType"] = messageType;
                ViewData["message"] = message;
            }
            return View((db.authors.Where(author => author.initials.ToLower().Contains(searchText) || searchText == null)).OrderBy(x => x.initials).ToList().ToPagedList(i ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Create(string authorInput)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            if (ModelState.IsValid && db.authors.Count((a) => a.initials == authorInput) == 0)
            {
                int current_id = db.authors.Count();
                while (db.authors.Count((a) => a.id == current_id) == 1) { current_id++; }
                authors newAuthor = new authors();
                newAuthor.id = current_id;
                newAuthor.initials = authorInput;
                db.authors.Add(newAuthor);
                db.SaveChanges();
                localMessage = $"Запись {authorInput} успешно добавлена.";
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Author/Edit/5
        public ActionResult Edit(int? id)
        {
            string localMessage = "Произошла непредвиденная ошибка.";
            if (id != null)
            {
                authors targetAuthor = db.authors.Find(id);
                if (targetAuthor != null)
                {
                    return RedirectToAction("/Index", new { i = 1, id = id, update = true });
                }
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        [HttpPost]
        public ActionResult Update(string authorInput, int idInput)
        {
            string localMessage = "Произошла непредвиденная ошибка или автор с таким именем уже есть.";
            if (ModelState.IsValid && db.authors.Count((a) => a.id == idInput) == 1 && db.authors.Count((a) => a.initials == authorInput) == 0)
            {
                authors targetAuthor = db.authors.Find(idInput);
                localMessage = $"Запись {targetAuthor.initials} успешно изменена на {authorInput}.";
                targetAuthor.initials = authorInput;
                db.Entry(targetAuthor).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Author/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            if (id != null)
            {
                authors targetAuthor = db.authors.Find(id);
                if (targetAuthor != null)
                {
                    db.authors.Remove(targetAuthor);
                    db.SaveChanges();
                    localMessage = $"Запись {targetAuthor.initials} была успешно удалена.";
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

        public void Export()
        {
            var authors = db.authors.OrderBy(x => x.initials);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "Номер";
            Sheet.Cells["B1"].Value = "Полное имя";
            int row = 2;
            foreach (var item in authors)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.initials;
                row++;
            }
            Sheet.Cells["A:AZ"].AutoFitColumns();
            var cellFont = Sheet.Cells["A:AZ"].Style.Font;
            cellFont.SetFromFont(new Font("Times New Roman", 12));
            cellFont.Bold = true;
            Sheet.Cells["A:AZ"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Авторы.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}
