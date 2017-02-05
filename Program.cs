using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Xml.Serialization;

namespace HabrJob
{
    public enum Education
    {
        None,
        Higher,
        IncompleteHigher,
        SecondaryVocational,
        Secondary,
        Pupil
    }

    public enum Employment
    {
        Full,
        Partial,
        Freelance
    }

    public class HabraJob
    {
        public HabraJob()
        {

        }
        private string _habraString;
        //private string _price = string.Empty;
        public string Url { get; set; } = " ";
        public string Title { get; set; } = " ";
        public string Skills { get; set; } = " ";
        public string CompanyName { get; set; } = " ";
        public string Meta { get; set; } = " ";
        public string Salary { get; set; } = " ";


    }


    class Program
    {
        public static WebClient wClient;
      //  public static WebRequest request;
        //public static WebResponse response;
        public static List<HabraJob> jobList;
        public static Encoding encode = System.Text.Encoding.GetEncoding("utf-8");


        /*public static string GetHtmlString(string html)
        {
            request = WebRequest.Create(html);
            response = request.GetResponse();
            
            using (StreamReader sReader = new StreamReader (response.GetResponseStream()))
            {
                string htmlString = sReader.ReadToEnd();
                return htmlString; 
            }
        }*/

        public static void SerializeToXml(List<HabraJob> list)
        {
            //работает
            using (StreamWriter output = new StreamWriter(string.Format("report {0}.xml", System.DateTime.Today.ToString("yyyyMMdd"))))
            {
                XmlSerializer serializer = new XmlSerializer(typeof (List<HabraJob>));
                serializer.Serialize(output, list);
            }
        }
        
        public static int GetPagesCount(HtmlDocument htmlDoc)
        {
            //работает
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("/ html / body / div[4] / div / div / div[6] / a[1]");
            return (int.Parse(node.InnerText) / 25) + 1;
        }

        public static void GetJobInfo(HtmlDocument htmlDoc)
        {
            var list = htmlDoc.GetElementbyId("jobs_list").ChildNodes;

             foreach (var node in list)
              {
                  //Node <div class="info"
                  var jobNode = node.FirstChild.ChildNodes.Where(x=>x.Name == "div").ToArray();

                  Console.WriteLine("------------  " +jobNode[0].FirstChild.InnerText+"------------------------- \n");

                  int jobNodesCount = jobNode[0].ChildNodes.ToArray().Count();
                HabraJob habrJob = new HabraJob();
                habrJob.Title = jobNode[0].SelectSingleNode("*[@class='title']").InnerText ?? " ";

                habrJob.Skills = (jobNode[0].SelectSingleNode("*[@class='skills']" )== null) ? " ": jobNode[0].SelectSingleNode("*[@class='skills']").InnerText.Replace("&bull;", "--");

                habrJob.CompanyName = (jobNode[0].SelectSingleNode("*[@class='company_name']") == null) ? " " : jobNode[0].SelectSingleNode("*[@class='company_name']").InnerText.Replace("&bull;", "--");

                habrJob.Meta = (jobNode[0].SelectSingleNode("*[@class='meta']") == null) ? " " : jobNode[0].SelectSingleNode("*[@class='meta']").InnerText.Replace("&bull;", "--");

                habrJob.Salary = (jobNode[0].SelectSingleNode("*[@class='salary']") == null) ? " " : jobNode[0].SelectSingleNode("*[@class='salary']").InnerText;

                // Console.WriteLine(habrJob.Title+"\n"+ habrJob.Skills+"\n"+ habrJob.CompanyName+"\n"+ habrJob.Meta+"\n"+ habrJob.Salary+"\n");
                jobList.Add(habrJob);
                
              }
        }



        static void Main()
        {

            wClient = new WebClient();
            var doc = new HtmlDocument();
            jobList = new List<HabraJob>();
            
            wClient.Encoding = encode;

            Console.WriteLine("Request............. " );

            doc.LoadHtml(wClient.DownloadString("https://moikrug.ru/vacancies")); // для теста парсим 16 страницу 

            GetJobInfo (doc);

            int pagesCount = GetPagesCount(doc);
            for (int i = 2; i <= pagesCount; i++)
            {
                Console.WriteLine("Page "+i);
                doc.LoadHtml(wClient.DownloadString(string.Format("https://moikrug.ru/vacancies?page={0}",i)));
               GetJobInfo(doc);
            }
            
            SerializeToXml(jobList);


            Console.WriteLine("----------Done---------- ");
            Console.ReadLine();
        }
    }
}
