using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _05TaskAsync
{
    internal class Program
    {
       
        public static int count = 0;

        private static readonly object _lock = new object();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        static async Task Main(string[] args)
        {

            //Код 1
            //Task[] tasks = Enumerable.Range(0, 1000000)
            //             .Select(_ => IncrementCount())
            //             .ToArray();

            //Код 2
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                tasks[i] = Task.Run(/*async*/ () =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        // если включить, то всё работает правильно(не включать). Замедляется выполнение задачи => меньше конкуренция.
                        //Console.WriteLine($"Поток[{Thread.CurrentThread.ManagedThreadId}]\tЗадача[{Task.CurrentId}]");


                        //Вариант 1
                        //count++;

                        //Вариант 2
                        //синхронизация для инкремента
                        Interlocked.Increment(ref count);

                        //Вариант 3
                        //Гарантирует, что только один поток за раз может выполнять участок код
                        //lock(_lock)
                        //{
                        //    count++;
                        //}

                        //Вариант 4
                        //await semaphore.WaitAsync(); //задача входит, если есть место
                        //try
                        //{
                        //    count++;
                        //}
                        //finally
                        //{
                        //    semaphore.Release(); //задача выходит
                        //}
                    }
                });
            }

            //Занимательно.
            //Код 1 запускает все задачи разом. Это сильно нагружает память процессора, но не нагружает сам процессор.Вычисления идут очень долго. Но результат правильный – ровно 1000000.
            //Код 2 запускает задачи циклами.Из - за этого некоторые прошлые задачи успевают закончится. Этот код сильно нагружает процессор, но не память.Расчёт почти моментальный, но не правильный.

            await Task.WhenAll(tasks);
            Console.WriteLine($"\n{count}");
            Console.ReadKey();
        }

        public static async Task IncrementCount()
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"Поток[{Thread.CurrentThread.ManagedThreadId}]\tЗадача[{Task.CurrentId}]");
                count++;
            });
        }
    }
}
