using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//Вариант 3 Scrutor
using Scrutor;
using static _04DI.Program;

namespace _04DI
{
    class Program
    {
        public enum ConfigurationFields { Mode, Logger }
        public enum ConfigurationMode { Standard, Logging }
        public enum ConfigurationLogger { Console, File }
        static async Task Main(string[] args)
        {
            //Установка конфигурации приложения
            var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                //Вариант 2 Factory
                                //[ConfigurationFields.Mode.ToString()] = ConfigurationMode.Logging.ToString(),
                                [nameof(ConfigurationFields.Logger)] = nameof(ConfigurationLogger.Console),
                            })
                            .Build();

            //Получение режима работы (логгера) из конфигурации
            string? loggerType = config[nameof(ConfigurationFields.Logger)];
            var services = new ServiceCollection();
            //Вариант 3 Scrutor
            switch (loggerType)
            {
                case nameof(ConfigurationLogger.Console):
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    break;
                case nameof(ConfigurationLogger.File):
                    services.AddSingleton<ILogger, FileLogger>();
                    break;
                default:
                    throw new InvalidOperationException("Неизвестный логгер");
            }

            //Вариант 3 Scrutor
            services.AddTransient<IProcessPayment, ProcessPayment>();
            services.Decorate<IProcessPayment, PaymentLoggingDecorator>();


            //Вариант 2 Factory
            //services.AddSingleton<IConfiguration>(config);
            //services.AddSingleton<IModeFactory, ModeFactory>();

            //Вариант 1 Keyed Services
            //services.AddKeyedTransient<IProcessPayment>("standart", (sp, key) => new ProcessPayment());
            //services.AddKeyedTransient<IProcessPayment>("logging", (sp, key) =>
            //{
            //    var processPayment = sp.GetRequiredKeyedService<IProcessPayment>("standart");
            //    var logger = sp.GetRequiredService<ILogger>();
            //    return new PaymentLoggingDecorator(processPayment, logger);
            //});

            var serviceProvider = services.BuildServiceProvider();

            //Вариант 3 Scrutor
            var processPaymentProcessor = serviceProvider.GetRequiredService<IProcessPayment>();

            //Вариант 2 Factory
            //var modeFactory = serviceProvider.GetRequiredService<IModeFactory>();
            //var processPaymentProcessor = modeFactory.Create();

            //Вариант 1 Keyed Services
            //var processPaymentProcessor = serviceProvider.GetRequiredKeyedService<IProcessPayment>("logging");

            //Создание платежей
            List<Payment> payments = new List<Payment>()
            {
                new Payment (100.50m, PaymentType.Card),
                new Payment(200m, PaymentType.Cash),
                new Payment(449.99m, PaymentType.Card),
            };

            //Запуск параллельной обработки платежей
            var paymentTasks = payments.Select(p => processPaymentProcessor.ProcessPaymentAsync(p)).ToArray();
            await Task.WhenAll(paymentTasks);
            Console.ReadKey();
        }

        //Вариант 2 Factory
        //public interface IModeFactory
        //{
        //    IProcessPayment Create();
        //}
        //public class ModeFactory : IModeFactory
        //{
        //    private readonly IConfiguration config;

        //    public ModeFactory(IConfiguration _config)
        //    {
        //        config = _config;
        //    }

        //    public IProcessPayment Create()
        //    {
        //        IProcessPayment service;

        //        switch (config[nameof(ConfigurationFields.Mode)])
        //        {
        //            case nameof(ConfigurationMode.Standard):
        //                service = new ProcessPayment();
        //                break;

        //            case nameof(ConfigurationMode.Logging):
        //                var loggerType = config["Logger"];
        //                ILogger logger = loggerType switch
        //                {
        //                    nameof(ConfigurationLogger.Console) => new ConsoleLogger(),
        //                    nameof(ConfigurationLogger.File) => new FileLogger(),
        //                    _ => throw new InvalidOperationException("Неизвестный логгер")
        //                };
        //                service = new PaymentLoggingDecorator(new ProcessPayment(), logger);
        //                break;

        //            default:
        //                throw new InvalidOperationException("Неизвестный режим");
        //        }

        //        return service;
        //    }
        //}

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