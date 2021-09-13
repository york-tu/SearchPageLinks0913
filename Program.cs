using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace SearchPageLinks_0913
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentdate = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
            string Upperfolderpath = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")); // 存檔路徑 (程式資料夾第一層)
            StreamWriter TotalResult = new StreamWriter($@"{Upperfolderpath}\TotalReport_{currentdate}.TXT", true); // 寫text檔 total report
            StreamWriter ErrorResult = new StreamWriter($@"{Upperfolderpath}\ErrorREport(NeedToCheck)_{currentdate}.TXT", true); // 寫text檔 fail report

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.esunbank.com.tw/bank/personal#");// 輸入要爬的網址
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string Code = reader.ReadToEnd(); //reader.ReadToEnd() 表示取得網頁的原始碼


            string pattern = @"((http|ftp|https)://)(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?"; // 判斷網址URL
            //①：該正則表達式匹配的字符串必須以http://、https://、ftp://開頭；
            //②：該正則表達式能匹配URL或者IP地址；（如：http://www.baidu.com 或者http://192.168.1.1）
            //③：該正則表達式能匹配到URL的末尾，即能匹配到子URL；（如能匹配：http://www.baidu.com/s?wd=a&rsv_spt=1&issp=1&rsv_bp=0&ie=utf-8&tn=baiduhome_pg&inputT=1236）
            //④：該正則表達式能夠匹配端口號；

            MatchCollection matchSources = Regex.Matches(Code, pattern); // 找出所有URL字串

            string[] linkToArray = new string[matchSources.Count];
            for (int i = 0; i < matchSources.Count; i++)
            {
                linkToArray[i] = matchSources[i].ToString(); // 將 MatchCollection matchSources 裡所有字串丟到新string array "linkToArray"
            }
            var briefArray = linkToArray.Distinct().ToArray(); // 刪掉array內重複字串

            int n = 1;
            foreach (var pagelink in briefArray)
            {

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(pagelink); // 打https request 到server
                myRequest.Method = "GET";
                myRequest.AllowAutoRedirect = true;
                // 有時候 Web 主機會回應 HTTP 狀態碼 300,301,302 到用戶端，如果我們想要截取這個資訊，就必須將 AllowAutoRedirect 屬性設成 false ，它的預設值為 ture 。
                // 如果值為 ture ，當送出 HttpWebRequest 後，伺服器會自動導向新的位置，而不會回應狀態碼給應用程式，而用戶端取得的 WebResponse 將是最後網頁的資訊。
                // 如果值為 false ，所有具 300 至 399 之 HTTP 狀態碼的回應都會傳回至應用程式。

                myRequest.Timeout = 10000;  //超時時間10秒

                try
                {
                    HttpWebResponse res = (HttpWebResponse)myRequest.GetResponse();
                    TotalResult.WriteLine($"List_{n},  '{pagelink}',       Result: '{res.StatusCode}'");
                    n++;
                }
                catch (WebException e)
                {
                    if (!e.Message.Contains("302"))
                    {
                        ErrorResult.WriteLine($"List_{n},  '{pagelink}',       Detail: '{e.Message}'");
                    }
                    n++;
                }
            }
            TotalResult.Close();
            ErrorResult.Close();
        }
    }
}
