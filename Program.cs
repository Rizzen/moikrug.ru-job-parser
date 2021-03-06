﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Xml.Serialization;

namespace HabrJob
{
    class Program
    {
        public static WebClient wClient;
        public static List<HabraJob> jobList;
        public static Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

        public static void SerializeToXml(List<HabraJob> list)
        {
           
            using (StreamWriter output = new StreamWriter(string.Format("report {0}.xml", System.DateTime.Today.ToString("yyyyMMdd"))))
            {
                XmlSerializer serializer = new XmlSerializer(typeof (List<HabraJob>));
                serializer.Serialize(output, list);
            }
        }
        
        public static int GetPagesCount(HtmlDocument htmlDoc)
        {
           
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("/ html / body / div[4] / div / div / div[6] / a[1]");
            return (int.Parse(node.InnerText) / 25) + 1;
        }

        public static void GetJobInfo(HtmlDocument htmlDoc)
        {
            var list = htmlDoc.GetElementbyId("jobs_list").ChildNodes;

             foreach (var node in list)
              {
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

            doc.LoadHtml(wClient.DownloadString("https://moikrug.ru/vacancies")); 

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
