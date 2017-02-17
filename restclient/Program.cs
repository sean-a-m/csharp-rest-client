using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace restclient
{
    class Program
    {

        static void Main(string[] args)
        {

            var titles = getArticleTitles();
            var articles = getArticles();
            var calItems = getCalendarItems();
            var messages = discoverFirstThread();

            Console.WriteLine("Last 10 Articles:\n");

            foreach (var article in articles)
            {
                Console.WriteLine("Title:" + article["title"] + " Author: " + getAuthorName(article["authorId"]));
            }

            Console.WriteLine("\n\nCalendar Events:\n");

            foreach (var item in getCalendarItems())
            {
                Console.WriteLine(item["summary"]);
            }

            Console.WriteLine("\n\nThread messages: ");

            foreach (var post in messages)
            {
                Console.WriteLine("Author: " + getAuthorName(post["authorId"]) + " Time: " + post["postTime"] + " Content: " + post["content"]);
            }

            Console.ReadLine();
        }

        private static IEnumerable<dynamic> getArticleTitles()
        {
            List<Dictionary<string, dynamic>> jsonThing = parseJsonArrayToDict("http://acadianasoftwaregroup.org/api/cms?o=10&s=0");
            jsonThing.Sort((i, j) => j["dateCreated"].CompareTo(i["dateCreated"]));
            return jsonThing.Take(10).Select(i => i["title"]);
        }

        private static IEnumerable<dynamic> getArticles()
        {
            List<Dictionary<string, dynamic>> jsonThing = parseJsonArrayToDict("http://acadianasoftwaregroup.org/api/cms?o=10&s=0");
            jsonThing.Sort((i, j) => j["dateCreated"].CompareTo(i["dateCreated"]));
            return jsonThing.Take(10);
        }

        private static List<Dictionary<string, dynamic>> getCalendarItems()
        {
            var calArray = parseJsonArrayToDict("http://acadianasoftwaregroup.org/api/cal/event");
            calArray.RemoveAll(i => isOlderThanSixMonths(i["endDate"]));
            return calArray;
        }

        
        private static List<Dictionary<string, dynamic>> discoverFirstThread()
        {
            var boardList = parseJsonArrayToDict("https://acadianasoftwaregroup.org/api/bb");
            string firstBoardId = boardList.Last()["boardId"];
            var threadList = parseJsonArrayToDict(getBoardUri(firstBoardId));
            string firstThreadId = threadList.First()["threadId"];
            var posts = parseJsonArrayToDict(getPostsUri(firstBoardId, firstThreadId));
            return posts;
        }

        private static bool isOlderThanSixMonths (DateTime thisDateTime)
        {
            return (thisDateTime.AddMonths(6) < DateTime.Now);
        }
         
        private static string getAuthorName(string uuid)
        {
            string apiRoot = "http://acadianasoftwaregroup.org/api/user/profiles/";
            var userJson = parseJsonItemToDict(apiRoot + uuid);
            return userJson["name"];
        }

        private static string getProfileUri(string uuid)
        {
            return "https://acadianasoftwaregroup.org/api/user/profiles/" + uuid;
        }

        private static string getPostsUri(string boardId, string threadId)
        {
            return "https://acadianasoftwaregroup.org/api/bb/" + boardId + "/" + threadId;
        }

        private static string getThreadUri(string boardId, string threadId)
        {
            return "https://acadianasoftwaregroup.org/api/bb/" + boardId + "/thread/" + threadId;
        }
        
        private static string getBoardUri(string boardId)
        {
            return "https://acadianasoftwaregroup.org/api/bb/" + boardId;
        }   

        private static Dictionary<string,dynamic> parseJsonItemToDict(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseString = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseString);
            
        }

        private static List<Dictionary<string, dynamic>> parseJsonArrayToDict(string uri)
        {
            
            HttpWebRequest requesto = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse responso = (HttpWebResponse)requesto.GetResponse();
            StreamReader reader = new StreamReader(responso.GetResponseStream());
            String responseString = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(responseString);
        }



    }
}
