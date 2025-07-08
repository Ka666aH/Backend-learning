using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _01TaskAsync
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Метод Main начал работу в потоке [{Thread.CurrentThread.ManagedThreadId}]");

            /*Task task = */WriteCharAsync('*');
            WriteChar('-');

            Console.WriteLine($"Метод Main закончил работу в потоке [{Thread.CurrentThread.ManagedThreadId}]");
            Console.ReadKey();
        }

        public static async Task WriteCharAsync(char s)
        {
            Console.WriteLine($"Метод WriteCharAsync начал работу в потоке [{Thread.CurrentThread.ManagedThreadId}]");

            await Task.Run(() => {
                Console.WriteLine($"Задача началась в потоке [{Thread.CurrentThread.ManagedThreadId}]");
                WriteChar(s);
                Console.WriteLine($"Задача закончилась в потоке [{Thread.CurrentThread.ManagedThreadId}]");
            });

            Console.WriteLine($"Метод WriteCharAsync закончил работу в потоке [{Thread.CurrentThread.ManagedThreadId}]");
        }

        public static void WriteChar(char s)
        {
            Console.WriteLine($"Метод WriteChar({s}) начал работу в потоке [{Thread.CurrentThread.ManagedThreadId}]. Задача [{Task.CurrentId}]");
            for (int i = 0; i < 40; i++)
            {
                Thread.Sleep(250);
                Console.Write(s);
            }
            Console.WriteLine($"\nМетод WriteChar({s}) закончил работу в потоке [{Thread.CurrentThread.ManagedThreadId}]");
        }
    }
}
