using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

//🔹 Бонус: Продвинутая задача(Async/ Await + LINQ)
//Цель: Написать метод, который принимает массив URL и возвращает первый успешно загруженный результат (остальные отменяются).
//Условия:

//Использовать CancellationTokenSource и Task.WhenAny.

//Если все запросы fail, бросить AggregateException.


namespace _07TaskAsync
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string[] urls = { 
                "https://invalid.url", //нерабочий
                "https://example.com", //рабочий
                "https://google.com", //рабочий
                "https://www.bybit.com/ru-RU/trade/*-/ETH/USDT", //бесконечная загрузка
            };

            try
            {
                Console.WriteLine($"Загружаемые сайты:\n{string.Join("\n", urls)}");
                string data = await GetFirstSuccessfulAsync(urls);
                Console.WriteLine($"Первый успешный результат: {data}.");
            }
            catch (AggregateException agEx)
            {
                Console.WriteLine($"Все запросы завершились с ошибками:");
                foreach (var ex in agEx.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"- {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка: {ex.Message}");
            }

            Console.ReadKey();
        }

        public static async Task<string> GetFirstSuccessfulAsync(string[] urls)
        {
            CancellationTokenSource mainCts = new CancellationTokenSource();
            CancellationTokenSource timeoutCts = new CancellationTokenSource();
            //CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(mainCts.Token, timeoutCts.Token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            Task[] tasks = urls.Select(url => DownloadPageSizeAsync(url, mainCts.Token, timeoutCts.Token)).ToArray();
            var exceptions = new List<Exception>();
            while (tasks.Count() > 0)
            {
                try
                {
                    Task<string> completedTask = (Task<string>)await Task.WhenAny(tasks);
                    tasks = tasks.Where(t => t != completedTask).ToArray();
                    //string result = await completedTask;
                    string result = completedTask.Result;
                    //if (completedTask.Exception == null)
                    if (completedTask.Status == TaskStatus.RanToCompletion)
                    {
                        mainCts.Cancel();
                        return result;
                    }
                    //if (completedTask.IsFaulted)
                    //{
                    //    exceptions.AddRange(completedTask.Exception.InnerExceptions);
                    //}
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions); //Все запросы завершились с ошибкой
        }

        public static async Task<string> DownloadPageSizeAsync(string url, CancellationToken mainToken, CancellationToken timeout)
        {
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(mainToken, timeout);
            CancellationToken cancellationToken = cts.Token;
            try
            {
                using (var client = new HttpClient())
                {
                    //byte[] data = await client.GetByteArrayAsync(url, cancellationToken);

                    // 7. Передаем токен в запрос
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    // 8. Проверяем успешность статуса
                    response.EnsureSuccessStatusCode();

                    // 9. Читаем данные (можно использовать ReadAsByteArrayAsync для простоты)
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    return $"{url}: {data.Length} байт";
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка загрузки {url}: {ex.Message}", ex);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new OperationCanceledException($"Превышено время загрузки. Загрузка {url} отменена.");
            }
            catch (OperationCanceledException) when (mainToken.IsCancellationRequested)
            {
                //Console.WriteLine($"Одна из необходимых страниц загружена. Загрузка {url} отменена.");
                throw new OperationCanceledException($"Одна из необходимых страниц загружена. Загрузка {url} отменена.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Неизвестная ошибка: {ex.Message}");
            }
        }
    }
}
