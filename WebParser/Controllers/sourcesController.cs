﻿using System;
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
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;

namespace WebParser.Controllers
{
    public class sourcesController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: sources
        public ActionResult Index(string searchText, string message, int? i, bool update = false, int id = 0)
        {
            if (searchText != null)
            {
                searchText = searchText.ToLower();
            }
            ViewData["Titlessss"] = "";
            string messageType = null;
            if (update && db.sources.Count((a) => a.id == id) == 1)
            {
                sources targetSource = db.sources.Find(id);
                if (targetSource != null)
                {
                    ViewData["Issue"] = targetSource.journal_issue;
                    ViewData["Volume"] = targetSource.journal_volume;
                    ViewData["Titlessss"] = targetSource.item_title;
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
            return View((db.sources.Where(source => source.item_title.ToLower().Contains(searchText) || searchText == null)).OrderBy(x => x.item_title).ToList().ToPagedList(i ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Create(string titleInput)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            try
            {
                if (ModelState.IsValid && db.sources.Count((a) => a.item_title == titleInput) == 0)
                {
                    int current_id = db.sources.Count();
                    while (db.sources.Count((a) => a.id == current_id) == 1) { current_id++; }
                    sources newSource = new sources();
                    newSource.id = current_id;
                    newSource.item_title = titleInput;
                    db.sources.Add(newSource);
                    db.SaveChanges();
                    localMessage = $"Запись {titleInput} успешно добавлена.";
                }
            }
            catch
            {

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
        public ActionResult Update(string titleInput, int idInput)
        {
            string localMessage = "Произошла непредвиденная ошибка или такой источник уже есть.";
            try
            {
                if (ModelState.IsValid && db.sources.Count((a) => a.id == idInput) == 1 && db.sources.Count((a) => (a.item_title == titleInput)) == 0)
                {
                    sources targetSource = db.sources.Find(idInput);
                    localMessage = $"Запись {targetSource.item_title}; успешна изменена на {titleInput}.";
                    targetSource.item_title = titleInput;
                    db.Entry(targetSource).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: sources/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            try
            {
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
            }
            catch
            {

            }
            localMessage = "Произошла непредвиденная ошибка.";
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        public void Export()
        {
            var sources = db.sources.OrderBy(x => x.item_title);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "Номер";
            Sheet.Cells["B1"].Value = "Название";
            Sheet.Cells["C1"].Value = "Серия";
            Sheet.Cells["D1"].Value = "Номер";
            int row = 2;
            foreach (var item in sources)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.item_title;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.journal_volume;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.journal_issue;
                row++;
            }
            Sheet.Cells["A:AZ"].AutoFitColumns();
            var cellFont = Sheet.Cells["A:AZ"].Style.Font;
            cellFont.SetFromFont(new Font("Times New Roman", 12));
            cellFont.Bold = true;
            Sheet.Cells["A:AZ"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Публикации.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}
