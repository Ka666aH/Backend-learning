
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//🔹 Задача 5: Дебаг асинхронного кода
//Цель: Найти и исправить ошибку в коде, где происходит deadlock.

//    Задание:
//Объяснить, почему возникает deadlock.
//Переписать код, чтобы избежать блокировки.

namespace _06TaskAsync
{
    internal class Program
    {
        // Где-то в синхронном коде:
        //public static void Main()
        public static async Task Main()
        {
            //var data = GetDataAsync().Result; // Deadlock!

            //Вариант 1
            var data = await GetDataAsync();

            //Вариант 2
            //var result = await Task.Run(() =>
            //{
            //    Thread.Sleep(1000);
            //    return "Данные";
            //}).ConfigureAwait(false); // ← Здесь отключаем контекст
            //var data = result.ToString();

            //Вариант 3
            //var data = Task.Run(async () => await GetDataAsync()).Result;

            Console.WriteLine(data);
            Console.ReadKey();
        }

        public static async Task<string> GetDataAsync()
        {
            var result = await Task.Run(() =>
            {
                Thread.Sleep(1000);
                return "Данные";
            });
            return result;
        }
    }
}
