using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _04TaskAsync
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //CancellationTokenSource cts = new CancellationTokenSource();
            //cts.CancelAfter(TimeSpan.FromMilliseconds(7000));
            //CancellationToken cancellationToken = cts.Token;
            string url = "https://example.com";
            Console.WriteLine($"Начата загрузка страницы по адресу {url}.");
            var userCts = new CancellationTokenSource(); // Отмена по нажатию клавиши
            var timeoutCts = new CancellationTokenSource(); // Отмена по времени
            var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(10000));

            try
            {
                Task<int> downloadPageSizeTask = DownloadPageSizeAsync(url, combinedTokenSource.Token);

                Task.Run(() =>
                {
                    Console.WriteLine($"Введите нажмите любую клавишу для отмены операции.");
                    Console.ReadKey();
                    if (!downloadPageSizeTask.IsCompleted) userCts.Cancel();
                    else Environment.Exit(0);
                });

                int downloadPageSizeTaskResult = await downloadPageSizeTask;
                if (downloadPageSizeTaskResult >= 0) Console.WriteLine($"\nРазмер страницы равен: {downloadPageSizeTaskResult}.");

            }
            catch (OperationCanceledException) when (userCts.IsCancellationRequested)
            {
                Console.WriteLine("\nЗагрузка отменена пользователем.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nЗагрузка отменена из-за таймаута.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.ReadKey();
        }

        public static async Task<int> DownloadPageSizeAsync(string url, CancellationToken cancellationToken)
        {
            await Task.Delay(6000, cancellationToken);

            //if (cancellationToken.IsCancellationRequested)
            //{
            //    throw new OperationCanceledException();
            //}

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
            catch (OperationCanceledException)
            {
                //Console.WriteLine($"\nОтменено: {ex.Message}");
                //return -1;
                throw;
            }

            //catch (TaskCanceledException ex)
            //{
            //    Console.WriteLine($"\nОтменено: {url}");
            //    return -1;
            //}

            catch (Exception ex)
            {
                Console.WriteLine($"\nНеизвестная ошибка: {ex.Message}");
                return -1;
            }
        }
    }
}
