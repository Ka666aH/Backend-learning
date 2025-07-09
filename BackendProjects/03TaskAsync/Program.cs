using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _03TaskAsync
{
    internal class Program
    {
        public static bool taskRunning = false;
        static async Task Main(string[] args)
        {
            Console.WriteLine($"\nMain начался: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
            string[] urls = { "https://example.com", "https://google.com", "https://github.com" };
            Task<int> getTotalPageSizeTask = GetTotalPageSizeAsync(urls);

            while (taskRunning)
            {
                Console.Write("*");
                Thread.Sleep(10);
            }
            int getTotalPageSizeTaskResult = await getTotalPageSizeTask;
            Console.WriteLine($"Общий размер страниц равен: {getTotalPageSizeTaskResult} байт.");
            Console.WriteLine($"\nMain закончился: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
            Console.ReadKey();
        }

        public static async Task<int> GetTotalPageSizeAsync(string[] urls)
        {
            Console.WriteLine($"\nGetTotalPageSizeAsync начался: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
            taskRunning = true;
            //int totalPageSize = 0;
            List<Task<int>> tasks = new List<Task<int>>();
            foreach (string url in urls)
            {
                Task<int> downloadPageSizeTask = DownloadPageSizeAsync(url);
                tasks.Add(downloadPageSizeTask);

                ////синхронная обработка
                //int taskResult = await downloadPageSizeTask;
                //if (taskResult >= 0) totalPageSize += taskResult;
            }

            //асинхронная обработка
            await Task.WhenAll(tasks);
            int totalPageSize = 0;
            foreach (Task<int> task in tasks)
            {
                int taskResult = await task;
                if (taskResult >= 0) totalPageSize += taskResult;
            }

            //Нейросетевой кринж
            //// Создаем и запускаем все задачи
            //Task<int>[] downloadTasks = urls.Select(DownloadPageSizeAsync).ToArray();

            //// Ожидаем завершения всех задач
            //await Task.WhenAll(downloadTasks);

            //// Суммируем результаты
            //int totalPageSize = downloadTasks
            //    .Where(t => t.Status == TaskStatus.RanToCompletion && t.Result >= 0)
            //    .Sum(t => t.Result);

            taskRunning = false;
            Console.WriteLine($"\nGetTotalPageSizeAsync закончился: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
            return totalPageSize;

            #region Вывод асинхронный
            //*******************************************
            //Размер страницы равен: 1256 байт.
            //****
            //Размер страницы равен: 54163 байт.
            //*
            //Размер страницы равен: 287191 байт.
            //Общий размер страниц равен: 342610 байт.

            //Время операции (************************************************) х 10мс = 31 х 10 мс = 310 мс.
            #endregion
            #region Вывод синхронный
            //******************************************
            //Размер страницы равен: 1256 байт.
            //************************************************
            //Размер страницы равен: 54174 байт.
            //**********************************************
            //Размер страницы равен: 287191 байт.
            //Общий размер страниц равен: 342621 байт.

            //Время операции(****************************************************************************************************************************************)  х 10мс = 166 х 10 = 1660 мс
            #endregion
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
            Console.WriteLine($"\nDownloadPageSizeAsync({url}) начался: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
            try
            {
                using (var client = new HttpClient())
                {
                    byte[] data = await client.GetByteArrayAsync(url);
                    Console.WriteLine($"\nРазмер страницы равен: {data.Length} байт.");
                    Console.WriteLine($"\nDownloadPageSizeAsync({url}) закончился: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
                    return data.Length;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nСетевая ошибка: {ex.Message}");
                Console.WriteLine($"\nDownloadPageSizeAsync({url}) закончился: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nНеизвестная ошибка: {ex.Message}");
                Console.WriteLine($"\nDownloadPageSizeAsync({url}) закончился: Поток[{Thread.CurrentThread.ManagedThreadId}]. Задача[{Task.CurrentId}]");
                return -1;
            }
        }
    }
}

//Main начался: Поток[1].Задача[]

//GetTotalPageSizeAsync начался: Поток[1].Задача[]

//DownloadPageSizeAsync(https://example.com) начался: Поток[1]. Задача[]

//DownloadPageSizeAsync(https://google.com) начался: Поток[1]. Задача[]

//DownloadPageSizeAsync(https://github.com) начался: Поток[1]. Задача[]
//**********************************************
//Размер страницы равен: 287190 байт.

//DownloadPageSizeAsync(https://github.com) закончился: Поток[13]. Задача[]
//**
//Размер страницы равен: 54166 байт.

//DownloadPageSizeAsync(https://google.com) закончился: Поток[12]. Задача[]
//**
//Размер страницы равен: 1256 байт.

//DownloadPageSizeAsync(https://example.com) закончился: Поток[13]. Задача[]

//GetTotalPageSizeAsync закончился: Поток[13].Задача[]
//Общий размер страниц равен: 342612 байт.

//Main закончился: Поток[1].Задача[]
