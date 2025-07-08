using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace AsyncHttpClientDemo
{
    class Program
    {
        // Метод Main МОЖЕТ быть асинхронным (начиная с C# 7.1)!
        static async Task Main(string[] args)
        {
            // Создаем экземпляр HttpClient (лучше в реальности использовать IHttpClientFactory!)
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Console.WriteLine("Начало запроса...");

                    // АСИНХРОННЫЙ вызов: не блокирует главный поток!
                    HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/users");

                    // Проверяем успешность запроса
                    response.EnsureSuccessStatusCode(); // Выбросит исключение при ошибке HTTP

                    // АСИНХРОННО читаем тело ответа (строку JSON)
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Запрос завершен!");
                    Console.WriteLine($"Получено {responseBody.Length} байт");
                    // Дальше можно парсить JSON (например, с помощью System.Text.Json)
                    Console.WriteLine(responseBody); // Вывод всего JSON (может быть долгим)
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Ошибка HTTP: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Неизвестная ошибка: {ex.Message}");
                }
            }
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
