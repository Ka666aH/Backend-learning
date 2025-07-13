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
            services.AddTransient<PaymentProcessor>();
            services.AddTransient<PaymentLogger>();
            var serviceProvider = services.BuildServiceProvider();
            var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();

            //Создание платежей
            List<Payment> payments = new List<Payment>()
            {
                new Payment (100.5m, PaymentType.Card),
                new Payment(200m, PaymentType.Cash),
                new Payment(450m, PaymentType.Card),
            };
            //Запуск параллельной обработки платежей
            var paymentTasks = payments.Select(p => paymentProcessor.ProcessPaymentAsync(p)).ToArray();
            await Task.WhenAll(paymentTasks);
            Console.ReadKey();
        }
        public class PaymentProcessor
        {
            private readonly PaymentLogger paymentLogger;
            public PaymentProcessor(PaymentLogger _paymentLogger)
            {
                paymentLogger = _paymentLogger;
            }
            //Обработка платежей
            public async Task ProcessPaymentAsync(Payment p)
            {
                await Task.Run(() =>
                {
                    //Отслеживание процесса обработки платежа
                    Console.WriteLine($"Payment №{Task.CurrentId} started in {Thread.CurrentThread.ManagedThreadId} thread.");
                    Task.Delay(1000);
                    paymentLogger.LogPayment(p.Amount, p.PaymentType);
                    Console.WriteLine($"Payment №{Task.CurrentId} ended in {Thread.CurrentThread.ManagedThreadId} thread.");
                });
            }
        }
        //Оплата
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
        //Логгер оплаты
        public class PaymentLogger
        {
            private readonly ILogger logger;
            public PaymentLogger(ILogger _logger)
            {
                logger = _logger;
            }
            public void LogPayment(decimal amount, PaymentType paymentType)
            {
                logger.Log($"{DateTime.Now} | ${amount} | {paymentType.ToString()}");
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