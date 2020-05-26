using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace WebParser.Controllers
{
    public class QueriesController : Controller
    {
        private SqlConnection connection;
        private System.Data.DataTable dataTable = new DataTable();
        u0979199_springer_dataEntities1 db = new u0979199_springer_dataEntities1();

        private void EstablishConnection()
        {
            connection = new SqlConnection(@"Data Source=31.31.196.234;Initial Catalog=u0979199_springer_data;Persist Security Info=True;User ID=u0979199_spender;Password=LErwjfu4c9");
            connection.Open();
        }

        [HttpPost]
        public ActionResult GetParams(string yearCheck, string minYear, string maxYear, string sourceCheck, string source, string numCheck, string num, string wordCheck, string word, string chapterCheck, string articleCheck, string uniqueCheck)
        {
            EstablishConnection();
            dataTable = GenerateQuery(yearCheck, minYear, maxYear, sourceCheck, source, numCheck, num, wordCheck, word, chapterCheck, articleCheck, uniqueCheck);
            if (dataTable != null)
            {
                ViewBag.QueryResult = dataTable;
            }
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        private DataTable GenerateQuery(string cbYear, string tbMinYear, string tbMaxYear, string cbSource, string tbSource, string cbAuthorsNumber, string num, string cbKeywords, string tbKeywords, string cbChapter, string cbArticle, string cbUnique)
        {
            DataTable result = new DataTable();
            int min_year = 0;
            int max_year = 0;
            if (cbYear == "on" && CheckYear(tbMinYear, tbMaxYear))
            {
                min_year = int.Parse(tbMinYear);
                max_year = int.Parse(tbMaxYear);
            }
            
            if (cbSource == "on" && tbSource != "")
            {
                if (cbArticle == "on" && cbChapter == "on")
                {
                    var q_sourceBoth = from publications in db.publications
                                       where publications.year >= min_year && publications.year <= max_year && publications.sources.Where(x => x.item_title.StartsWith(tbSource)).Count() != 0 && publications.types.Where(x => x.id == 2).Count() != 0 && publications.types.Where(x => x.id == 1).Count() != 0
                                       select publications.id;
                }
                else if (cbArticle == "on")
                {
                    var q_sourceArticle = from publications in db.publications
                                          where publications.year >= min_year && publications.year <= max_year && publications.sources.Where(x => x.item_title.StartsWith(tbSource)).Count() != 0 && publications.types.Where(x => x.id == 1).Count() != 0
                                          select publications.id;
                }
                else if (cbChapter == "on")
                {
                    var q_sourceChapter = from publications in db.publications
                                          where publications.year >= min_year && publications.year <= max_year && publications.sources.Where(x => x.item_title.StartsWith(tbSource)).Count() != 0 && publications.types.Where(x => x.id == 2).Count() != 0
                                          select publications.id;
                }
            }


            if (cbYear == "on" && CheckYear(tbMinYear, tbMaxYear))
            {
                var q_year = from publications in db.publications
                        where publications.year >= min_year && publications.year <= max_year
                        select new { Title = publications.title};

                result = LINQResultToDataTable(q_year);

                if (cbAuthorsNumber == "on" && Int32.TryParse(num, out int numAuthor))
                {

                    var queryAuthorNumber = from publications in db.publications
                                      where publications.authors.Count() == numAuthor && publications.year >= min_year && publications.year <= max_year
                                      select publications.id;

                }
                return result;
            }
            if (cbKeywords == "on" && tbKeywords != "")
            {
                List<int> keywords = GetKeywords(tbKeywords);
                var queryAuthorNumber = from pub in db.publications
                                        from w in pub.keywords
                                        where keywords.Contains(w.id)
                                        group w by w.keyword into c
                                        select new { name = c.Key, count = c.Count(), pubId = from p in c select p.id};
                result = LINQResultToDataTable(queryAuthorNumber);
                return result;
            }
            if (cbUnique == "on")
            {

                var queryUnique = from pub in db.publications
                                  where pub.authors.Count != 0 && pub.sources.Count != 0
                                  select pub.id;

            }
            return null;

        }

        private bool CheckYear(string n1, string n2)
        {
            int num1;
            int num2;
            return (Int32.TryParse(n1, out num1) && Int32.TryParse(n2, out num2));
        }

        private List<int> GetKeywords(string input)
        {
            char[] delimiter = { ',' };
            string[] keywords = input.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            List<int> keys = new List<int>();
            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].ToLower().Replace(" ", "");
            }
            foreach (string word in keywords)
            {
                keywords w = db.keywords.Where(x => x.keyword == word).FirstOrDefault();
                if (w != null)
                {
                    keys.Add(w.id);
                }
            }
            return keys;
        }

        public DataTable LINQResultToDataTable<T>(IEnumerable<T> Linqlist)
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