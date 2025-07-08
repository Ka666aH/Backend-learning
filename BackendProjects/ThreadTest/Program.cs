using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadTest
{
    internal class Program
    {
        public static long result = 0;/*CalculateFartorial(number);*/
        static void Main(string[] args)
        {
            int number = 5;
            ThreadPoolWorker threadPoolWorker = new ThreadPoolWorker(new Action<object>(CalculateFartorialAction));
            threadPoolWorker.Run(number);
            threadPoolWorker.Wait();
            Console.ReadKey();
        }

        private static void CalculateFartorialAction(object arg)
        {
            int number = (int)arg;
            result = CalculateFartorial(number);
            Console.WriteLine($"\n\nРезультат: {result}\n");
        }

        private static long CalculateFartorial(int number)
        {
            Thread.Sleep(100);
            if (number == 1) return number;
            else return CalculateFartorial(number - 1) * number;
        }
    }
    class ThreadPoolWorker
    {
        private readonly Action<object> action;

        public ThreadPoolWorker(Action<object> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public bool Completed { get; private set; } = false;
        public Exception Exception { get; private set; } = null;
        public void Run(object state)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadExecution), state);
        }

        public void Wait()
        {
            while (!Completed)
            {
                Thread.Sleep(100);
                Console.Write("*");
            }

            if (Exception != null) throw Exception;
        }

        private void ThreadExecution(object state)
        {
            try
            {
                action.Invoke(state);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            finally
            {
                Completed = true;
            }
        }
    }
}
