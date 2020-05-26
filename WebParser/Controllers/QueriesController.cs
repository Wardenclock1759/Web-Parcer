using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        private SqlDataAdapter dataAdapter;

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
            string query = "";
            string comma_str = "";
            string article_str = "";
            string chapter_str = "";

            //var queryString


            if (cbArticle == "on" && cbChapter == "on")
            {
                comma_str = ", ";
                chapter_str = "'Chapter','ConferencePaper','ReferenceWorkEntry'";
                article_str = "'Article'";
            }
            else if (cbArticle == "on")
            {
                comma_str = "";
                article_str = "'Article'";
                chapter_str = "";
            }
            else if (cbChapter == "on")
            {
                comma_str = "";
                article_str = "";
                chapter_str = "'Chapter', 'ConferencePaper', 'ReferenceWorkEntry'";
            }
            else
            {
                comma_str = ", ";
                chapter_str = "'Chapter', 'ConferencePaper', 'ReferenceWorkEntry'";
                article_str = "'Article'";
            }


            if (cbYear == "on" && CheckYear(tbMinYear, tbMaxYear))
            {
                int min_year = int.Parse(tbMinYear);
                int max_year = int.Parse(tbMaxYear);

                query = "select " +
                    "distinct year as 'Год издания', " +
                    "count(*) over (partition by year) as 'Количество' " +
                    "from publications " +
                    $"where year between {min_year} and {max_year}";

                if (cbSource == "on" && tbSource != "")
                {
                    query = "select distinct year as 'Год издания', count(*) over (partition by year) as 'Количество' " +
                "from publications " +
                "where " +
                $"year between {min_year} and {max_year} " +
                "and id in ( " +
                "select publication_id " +
                "from publications_sources " +
                "where source_id in ( " +
                "select id from sources " +
                $"where item_title like '%{tbSource}%') " +
                "intersect " +
                "select publication_id " +
                "from publications_types " +
                "where " +
                "type_id in ( " +
                "select id " +
                "from types " +
                $"where type_name in ({article_str}{comma_str}{chapter_str})))";

                }
                if (cbAuthorsNumber == "on" && Int32.TryParse(num, out int numAuthor))
                {
                    query = "select distinct year as 'Год издания', count(*) over (partition by year) as 'Количество'  " +
                        " from publications " +
                        "where " +
                        $"year between {min_year} and {max_year} " +
                        "and id in ( " +
                        "select publication_id " +
                        "from publications_authors " +
                        "group by publication_id " +
                        $"having count(author_id) = {numAuthor}); ";
                }
                dataAdapter = new SqlDataAdapter(query, connection);
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
            if (cbKeywords == "on" && tbKeywords != "")
            {
                SqlDataAdapter sqlAd;
                dataTable = new System.Data.DataTable();
                string[] keywords = GetKeywords(tbKeywords);
                foreach (string el in keywords)
                {
                    query = "select distinct k.keyword as 'Ключевое слово', " +
                    "count(*) over (partition by p_k.keyword_id) as 'Количество' " +
                    "from publications_keywords p_k " +
                    "join keywords k " +
                    "on k.id = p_k.keyword_id " +
                    $"where lower(k.keyword) in ('{el}'); ";

                    sqlAd = new SqlDataAdapter(query, connection);
                    sqlAd.Fill(dataTable);
                }

                return dataTable;
            }
            if (cbUnique == "on")
            {
                query = "select distinct p.title as 'Публикация', a.initials as 'Автор', s.item_title as 'Источник', p.year as 'Год издания', s.journal_issue as 'Номер журнала'" +
                    "from publications p " +
                    "join publications_authors p_a " +
                    "on p.id = p_a.publication_id " +
                    "join authors a " +
                    "on a.id = p_a.author_id " +
                    "join publications_sources p_s " +
                    "on p.id = p_s.publication_id " +
                    "join sources s " +
                    "on p_s.source_id = s.id ";

                dataAdapter = new SqlDataAdapter(query, connection);
                dataAdapter.Fill(dataTable);
                return dataTable;
            }


            return null;

        }

        private bool CheckYear(string n1, string n2)
        {
            int num1;
            int num2;
            return (Int32.TryParse(n1, out num1) && Int32.TryParse(n2, out num2));
        }

        private string[] GetKeywords(string input)
        {
            char[] delimiter = { ',' };
            string[] keywords = input.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].ToLower().Replace(" ", "");
            }
            return keywords;
        }
    }
}