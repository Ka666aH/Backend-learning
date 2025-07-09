using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _02TaskAsync
{
    internal class Program
    {
        public static bool taskRunning = false;
        static async Task Main(string[] args)
        {
            string url = "https://example.com";
            Task<int> downloadPageSizeTask = DownloadPageSizeAsync(url);

            while (taskRunning)
            {
                Console.Write("*");
                Thread.Sleep(1000);
            }
            int downloadPageSizeTaskResult = await downloadPageSizeTask;
            if (downloadPageSizeTaskResult >= 0) Console.WriteLine($"\nРазмер страницы равен: {downloadPageSizeTaskResult}.");
            //else Console.WriteLine($"\nЗадача завершилась с ошибкой {downloadPageSizeTaskResult}.");
            Console.ReadKey();
        }

        public static async Task<int> DownloadPageSizeAsync(string url)
        {
            #region мой код
            //taskRunning = true;
            //return await Task<int>.Run(() =>
            //{
            //    Thread.Sleep(5000);
            //    try
            //    {
            //        int bytes;
            //        using (WebClient webClient = new WebClient())
            //        {
            //            bytes = webClient.DownloadData(url).Length;
            //        }
            //        return bytes;
            //    }
            //    catch (Exception ex)
            //    {
            //        //return ex.HResult;
            //        Console.WriteLine($"\nЗадача завершилась с ошибкой: {ex.Message}.");
            //        return -1;
            //    }
            //    finally
            //    {
            //        taskRunning = false;
            //    }
            //});
            #endregion
            taskRunning = true;

            try
            {
                using (var client = new HttpClient())
                {
                    byte[] data = await client.GetByteArrayAsync(url);
                    return data.Length;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nСетевая ошибка: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nНеизвестная ошибка: {ex.Message}");
                return -1;
            }
            finally
            {
                taskRunning = false;
            }
        }
    }
}
