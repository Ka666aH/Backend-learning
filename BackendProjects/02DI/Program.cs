using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static _02DI.Program;

namespace _02DI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Установка конфигурации приложения
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            //Получение режима работы (логгера) из конфигурации
            string? loggerType = configuration["Logger"];
            var services = new ServiceCollection();

            switch (loggerType)
            {
                case "Console":
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    break;
                case "File":
                    services.AddSingleton<ILogger, FileLogger>();
                    break;
                default: throw new InvalidOperationException("Неизвестный логгер");
            }
            services.AddTransient<IProcessPayment,ProcessPayment>();
            services.AddTransient<IPaymentLogger, PaymentLogger>();
            services.AddTransient<PaymentProcessor>();
            var serviceProvider = services.BuildServiceProvider();
            var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();

            //Создание платежей
            List<Payment> payments = new List<Payment>()
            {
                new Payment (100.50m, PaymentType.Card),
                new Payment(200m, PaymentType.Cash),
                new Payment(449.99m, PaymentType.Card),
            };
            //Запуск параллельной обработки платежей
            var paymentTasks = payments.Select(p => paymentProcessor.MakePaymentAsync(p)).ToArray();
            await Task.WhenAll(paymentTasks);
            Console.ReadKey();
        }
        public class PaymentProcessor
        {
            private readonly IProcessPayment processPayment;
            private readonly IPaymentLogger paymentLogger;

            public PaymentProcessor(IProcessPayment _processPayment, IPaymentLogger _logPayment)
            {
                processPayment = _processPayment;
                paymentLogger = _logPayment;
            }
            //Обработка платежей
            public async Task MakePaymentAsync(Payment p)
            {
                await processPayment.ProcessPaymentAsync();
                await paymentLogger.LogPayment(p.Amount, p.PaymentType);
            }
        }

        public interface IProcessPayment
        {
            Task ProcessPaymentAsync();
        }

        public class ProcessPayment : IProcessPayment
        {
            public async Task ProcessPaymentAsync()
            {
                await Task.Run(async() =>
                {
                    //Отслеживание процесса обработки платежа
                    int? taskId = Task.CurrentId;
                    Console.WriteLine($"Payment №{taskId} started in {Thread.CurrentThread.ManagedThreadId} thread.");
                    await Task.Delay(1000);
                    Console.WriteLine($"Payment №{taskId} ended in {Thread.CurrentThread.ManagedThreadId} thread.");
                });
            }
        }
        public interface IPaymentLogger
        {
            Task LogPayment(decimal amount, PaymentType paymentType);
        }

        //Логгер оплаты
        public class PaymentLogger : IPaymentLogger
        {
            private readonly ILogger logger;
            public PaymentLogger(ILogger _logger)
            {
                logger = _logger;
            }
            public async Task LogPayment(decimal amount, PaymentType paymentType)
            {
                //Отслеживание процесса обработки платежа
                await Task.Run(() =>
                {
                    Console.WriteLine($"Logging №{Task.CurrentId} started in {Thread.CurrentThread.ManagedThreadId} thread.");
                    logger.Log($"{DateTime.Now} | ${amount} | {paymentType.ToString()}");
                    Console.WriteLine($"Logging №{Task.CurrentId} ended in {Thread.CurrentThread.ManagedThreadId} thread.");
                });
            }
        }
        public class Payment
        {
            public decimal Amount { get; private set; }
            public PaymentType PaymentType { get; private set; }

            public Payment(decimal _amount, PaymentType _paymentType)
            {
                Amount = _amount;
                PaymentType = _paymentType;
            }
        }
        //Типы оплаты
        public enum PaymentType
        {
            Card,
            Cash
        }
        //Интерфейс логгера
        public interface ILogger
        {
            void Log(string message);
        }
        //Реализация консольного логгера
        public class ConsoleLogger : ILogger
        {
            public void Log(string message)
            {
                Console.WriteLine(message);
            }
        }
        //Реализация файлового логгера
        public class FileLogger : ILogger
        {
            private static readonly object lockObj = new object();
            public void Log(string message)
            {
                //Потокобезопасная запись в файл
                lock (lockObj)
                {
                    File.AppendAllText("log.txt", $"{message}\n");
                }
            }
        }
    }
}