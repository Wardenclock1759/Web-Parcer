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
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using System.Reflection;

namespace WebParser.Controllers
{
    public class PublicationsController : Controller
    {
        private u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        // GET: Publications
        [HttpGet]
        public ActionResult Index(string searchText, string message, List<publications> list, int? i, bool update = false, int id = 0, int minYear = 1, int auNum = -1, string soString = "Empty", string typeString = "NoType", string word = "")
        {
            if (searchText != null)
            {
                searchText = searchText.ToLower();
            }
            ViewData["Titlesss"] = "";
            string messageType = null;
            if (update && db.publications.Count((a) => a.id == id) == 1)
            {
                publications targetPublication = db.publications.Find(id);
                if (targetPublication != null)
                {
                    ViewData["Titlesss"] = targetPublication.title;
                    ViewData["Year"] = targetPublication.year;
                    ViewData["Update"] = update;
                    ViewData["Id"] = id;
                    var queryUnique = from pub in db.publications
                                      where pub.authors.Count != 0 && pub.sources.Count != 0 && pub.id == id
                                      orderby pub.title
                                      select new
                                      {
                                          Title = pub.title,
                                          Authors = from p in db.publications
                                                    from author in p.authors
                                                    where p.id == pub.id && p.authors.Count() != 0
                                                    orderby author.initials
                                                    select author.initials,
                                          Sources = from p in db.publications
                                                    from sourc in p.sources
                                                    where p.id == pub.id && p.sources.Count() != 0
                                                    orderby sourc.item_title
                                                    select sourc.item_title,
                                          Issue = from p in db.publications
                                                  from sourc in p.sources
                                                  where p.id == pub.id && p.sources.Count() != 0
                                                  orderby sourc.journal_issue
                                                  select sourc.journal_issue,
                                          Volume = from p in db.publications
                                                   from sourc in p.sources
                                                   where p.id == pub.id && p.sources.Count() != 0
                                                   orderby sourc.journal_volume
                                                   select sourc.journal_volume,
                                          Year = pub.year,
                                          Words = pub.keywords.Select(x => x.keyword)
                                      };
                    DataTable result = LINQResultToDataTable(queryUnique);
                    ViewBag.QueryResult = result;
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
            var authors = from au in db.authors
                          orderby au.initials
                          select au.initials;
            ViewBag.authors = authors;
            var source = from s in db.sources
                         orderby s.item_title
                         select s.item_title;
            ViewBag.source = source;
            var type = from t in db.types
                       orderby t.type_name
                       select t.type_name;
            ViewBag.type = type;

            return View((db.publications.Where(publications => (publications.title.ToLower().Contains(searchText) || searchText == null) && (publications.year == minYear || minYear == 1) && (publications.authors.Count() == auNum || auNum == -1)).OrderBy(x => x.title).ToList().ToPagedList(i ?? 1, 10)));
        }

        [HttpPost]
        public ActionResult Create(string titleInput, int yearInput, string[] Authors, string Words, string Source, string Type, int issueInput = 0, int volumeInput = 0)
        {
            string localMessage = "Произошла ошибка или запись уже есть.";
            List<int> authorId = GetAuthors(Authors);
            string[] words = GetKeywords(Words);

            try
            {
                if (ModelState.IsValid && db.publications.Count((a) => a.title == titleInput) == 0 && authorId.Count != 0)
                {
                    int current_id = db.publications.Count();
                    while (db.publications.Count((a) => a.id == current_id) == 1) { current_id++; }
                    publications newPublication = new publications();

                    newPublication.id = current_id;
                    newPublication.title = titleInput;
                    newPublication.year = Convert.ToInt16(yearInput);
                    foreach (int authorid in authorId)
                    {
                        authors a = db.authors.Find(authorid);
                        if (a != null)
                        {
                            newPublication.authors.Add(a);
                            a.publications.Add(newPublication);
                        }
                    }
                    foreach (string keyword in words)
                    {
                        keywords k = db.keywords.Where(x => x.keyword == keyword).FirstOrDefault();
                        if (k == null)
                        {
                            int k_id = db.keywords.Count();
                            while (db.keywords.Count((a) => a.id == k_id) == 1) { k_id++; }
                            k = new keywords();
                            k.id = k_id;
                            k.keyword = keyword;
                            db.keywords.Add(k);
                            db.SaveChanges();
                        }
                        newPublication.keywords.Add(k);
                        k.publications.Add(newPublication);
                    }
                    sources s = db.sources.Where(x => x.item_title == Source).FirstOrDefault();
                    if (s != null)
                    {
                        newPublication.sources.Add(s);
                        if (issueInput > 0 && volumeInput > 0)
                        {
                            s.journal_issue = issueInput;
                            s.journal_volume = volumeInput;
                        }
                        else
                        {
                            s.journal_issue = null;
                            s.journal_volume = null;
                        }
                        s.publications.Add(newPublication);
                    }
                    types t = db.types.Where(x => x.type_name == Type).FirstOrDefault();
                    if (t != null)
                    {
                        newPublication.types.Add(t);
                        t.publications.Add(newPublication);
                    }

                    db.publications.Add(newPublication);
                    db.SaveChanges();
                    localMessage = $"Запись {titleInput} успешно добавлена.";
                }
            }
            catch
            {

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
        public ActionResult Update(string titleInput, int idInput, int yearInput, string[] Authors, string Words, string Source, string Type, int issueInput = 0, int volumeInput = 0)
        {
            List<int> authorId = GetAuthors(Authors);
            string[] words = GetKeywords(Words);
            string localMessage = "Произошла непредвиденная ошибка или такая публикация уже есть.";
            try
            {
                if (ModelState.IsValid && db.publications.Count((a) => a.id == idInput) == 1 && db.publications.Count((a) => (a.title == titleInput) && (a.year == yearInput)) == 0)
                {
                    publications targetPublication = db.publications.Find(idInput);
                    localMessage = $"Запись {targetPublication.title} успешно изменена на {titleInput}.";
                    targetPublication.title = titleInput;
                    targetPublication.year = Convert.ToInt16(yearInput);

                    foreach (authors au in targetPublication.authors) au.publications.Remove(targetPublication);
                    targetPublication.authors.Clear();
                    foreach (int authorid in authorId)
                    {
                        authors a = db.authors.Find(authorid);
                        if (a != null)
                        {
                            targetPublication.authors.Add(a);
                            a.publications.Add(targetPublication);
                        }
                    }
                    foreach (keywords kw in targetPublication.keywords) kw.publications.Remove(targetPublication);
                    targetPublication.keywords.Clear();
                    foreach (string keyword in words)
                    {
                        keywords k = db.keywords.Where(x => x.keyword == keyword).FirstOrDefault();
                        if (k == null)
                        {
                            int k_id = db.keywords.Count();
                            while (db.keywords.Count((a) => a.id == k_id) == 1) { k_id++; }
                            k = new keywords();
                            k.id = k_id;
                            k.keyword = keyword;
                            db.keywords.Add(k);
                            db.SaveChanges();
                        }
                        targetPublication.keywords.Add(k);
                        k.publications.Add(targetPublication);
                    }
                    foreach (sources so in targetPublication.sources) so.publications.Remove(targetPublication);
                    targetPublication.sources.Clear();
                    sources s = db.sources.Where(x => x.item_title == Source).FirstOrDefault();
                    if (s != null)
                    {
                        targetPublication.sources.Add(s);
                        if (issueInput > 0 && volumeInput > 0)
                        {
                            s.journal_issue = issueInput;
                            s.journal_volume = volumeInput;
                        }
                        else
                        {
                            s.journal_issue = null;
                            s.journal_volume = null;
                        }
                        s.publications.Add(targetPublication);
                    }
                    foreach (types ty in targetPublication.types) ty.publications.Remove(targetPublication);
                    targetPublication.types.Clear();
                    types t = db.types.Where(x => x.type_name == Type).FirstOrDefault();
                    if (t != null)
                    {
                        targetPublication.types.Add(t);
                        t.publications.Add(targetPublication);
                    }

                    db.Entry(targetPublication).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        // GET: Publications/Delete/5
        public ActionResult Delete(int? id)
        {
            string localMessage = null;
            if (id != null)
            {
                try
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
                catch
                {

                }
            }
            localMessage = "Произошла непредвиденная ошибка.";
            return RedirectToAction("/Index", new { i = 1, message = localMessage });
        }

        private List<int> GetAuthors(string[] initials)
        {
            List<int> result = new List<int>();
            if (initials.Count() > 0)
            {
                foreach (string s in initials)
                {
                    authors a = db.authors.Where(x => x.initials == s).FirstOrDefault();
                    if (a != null)
                    {
                        result.Add(a.id);
                    }
                }
            }
            return result;
        }

        private string[] GetKeywords(string words)
        {
            char[] delimiter = { ',' };
            string[] keywords = words.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].ToLower().Replace(" ", "");
            }
            return keywords;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public void Export(string searchText, int minYear = 1, int auNum = -1, string soString = "Empty", string typeString = "NoType", string word = "")
        {
            var publications = db.publications.Where(x => (x.title.ToLower().Contains(searchText) || searchText == null) && (x.year == minYear || minYear == 1) && (x.authors.Count() == auNum || auNum == -1)).OrderBy(x => x.title);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "Номер";
            Sheet.Cells["B1"].Value = "Название";
            Sheet.Cells["C1"].Value = "Год";
            int row = 2;
            foreach (var item in publications)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.title;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.year;
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

        private DataTable LINQResultToDataTable<T>(IEnumerable<T> Linqlist)
        {
            DataTable dt = new DataTable();

            PropertyInfo[] columns = null;

            if (Linqlist == null) return dt;

            foreach (T Record in Linqlist)
            {

                if (columns == null)
                {
                    columns = Record.GetType().GetProperties();
                    foreach (PropertyInfo GetProperty in columns)
                    {
                        Type colType = GetProperty.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dt.Columns.Add(new DataColumn(GetProperty.Name, colType));
                    }
                }

                DataRow dr = dt.NewRow();

                foreach (PropertyInfo pinfo in columns)
                {
                    dr[pinfo.Name] = pinfo.GetValue(Record, null) == null ? DBNull.Value : pinfo.GetValue
                    (Record, null);
                }

                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
