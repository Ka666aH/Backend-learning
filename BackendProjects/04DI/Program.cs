using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static _04DI.Program;

namespace _04DI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Установка конфигурации приложения
            var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["Logger"] = "File"
                            })
                            .Build();

            //Получение режима работы (логгера) из конфигурации
            string? loggerType = config["Logger"];
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

            services.AddKeyedTransient<IProcessPayment>("standart", (sp, key) => new ProcessPayment());
            services.AddKeyedTransient<IProcessPayment>("logging", (sp, key) =>
            {
                var processPayment = sp.GetRequiredKeyedService<IProcessPayment>("standart");
                var logger = sp.GetRequiredService<ILogger>();
                return new PaymentLoggingDecorator(processPayment, logger);
            });
            var serviceProvider = services.BuildServiceProvider();
            var paymentProcessor = serviceProvider.GetRequiredKeyedService<IProcessPayment>("logging");

            //Создание платежей
            List<Payment> payments = new List<Payment>()
            {
                new Payment (100.50m, PaymentType.Card),
                new Payment(200m, PaymentType.Cash),
                new Payment(449.99m, PaymentType.Card),
            };

            //Запуск параллельной обработки платежей
            var paymentTasks = payments.Select(p => paymentProcessor.ProcessPaymentAsync(p)).ToArray();
            await Task.WhenAll(paymentTasks);
            Console.ReadKey();
        }

        public interface IProcessPayment
        {
            Task ProcessPaymentAsync(Payment p);
        }

        public class ProcessPayment : IProcessPayment
        {
            public async Task ProcessPaymentAsync(Payment p)
            {
                await Task.Run(async () =>
                {
                    //Отслеживание процесса обработки платежа
                    int? taskId = Task.CurrentId;
                    Console.WriteLine($"Payment №{taskId} started in {Thread.CurrentThread.ManagedThreadId} thread.");
                    await Task.Delay(1000);
                    Console.WriteLine($"Payment №{taskId} ended in {Thread.CurrentThread.ManagedThreadId} thread.");
                });
            }
        }

        //Логгер оплаты
        public class PaymentLoggingDecorator : IProcessPayment
        {
            private readonly ILogger logger;
            private readonly IProcessPayment processPayment;
            public PaymentLoggingDecorator(IProcessPayment _processPayment, ILogger _logger)
            {
                logger = _logger;
                processPayment = _processPayment;
            }
            public async Task ProcessPaymentAsync(Payment p)
            {
                //Отслеживание процесса обработки платежа
                await Task.Run(async () =>
                {
                    int? taskId = Task.CurrentId;
                    logger.Log($"Payment №{taskId} log started at {DateTime.Now}\tPayment info:\t|{p.Amount}\t| {p.PaymentType}");
                    await processPayment.ProcessPaymentAsync(p);
                    logger.Log($"Payment №{taskId} log ended at {DateTime.Now}\tPayment info:\t|{p.Amount}\t| {p.PaymentType}");

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