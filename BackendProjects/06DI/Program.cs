//Бонусная задача: Динамическое разрешение
//Цель: Реализовать выбор реализации во время выполнения.

//Задание:
//Зарегистрируйте реализации IReportGenerator
//Реализуйте фабричную функцию для динамического выбора
//Протестируйте создание разных отчетов

//Новые концепты:
//Фабрика как зависимость
//Функции разрешения сервисов
//Позднее связывание

//Пример реализации от QWEN

using Microsoft.Extensions.DependencyInjection;
using System;

namespace _06DI
{
    public enum ReportType { Pdf, Excel }

    public interface IReportGenerator
    {
        void Generate();
    }

    public class PdfReportGenerator : IReportGenerator
    {
        public void Generate() => Console.WriteLine("Generating PDF");
    }

    public class ExcelReportGenerator : IReportGenerator
    {
        public void Generate() => Console.WriteLine("Generating Excel");
    }

    public class ReportOrchestrator
    {
        private readonly Func<ReportType, IReportGenerator> _factory;

        public ReportOrchestrator(Func<ReportType, IReportGenerator> factory)
        {
            _factory = factory;
        }

        public void CreateReport(ReportType type)
        {
            var generator = _factory(type);
            generator.Generate();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Настройка DI-контейнера
            var services = new ServiceCollection();

            // Регистрация реализаций IReportGenerator
            services.AddTransient<PdfReportGenerator>();
            services.AddTransient<ExcelReportGenerator>();

            // Регистрация фабричной функции
            services.AddSingleton<Func<ReportType, IReportGenerator>>(serviceProvider => type =>
            {
                switch (type)
                {
                    case ReportType.Pdf:
                        return serviceProvider.GetRequiredService<PdfReportGenerator>();
                    case ReportType.Excel:
                        return serviceProvider.GetRequiredService<ExcelReportGenerator>();
                    default:
                        throw new ArgumentException("Invalid report type", nameof(type));
                }
            });

            // Регистрация ReportOrchestrator
            services.AddTransient<ReportOrchestrator>();

            // Создание провайдера сервисов
            var serviceProvider = services.BuildServiceProvider();

            // Получение ReportOrchestrator
            var orchestrator = serviceProvider.GetService<ReportOrchestrator>();

            // Протестируем создание разных отчётов
            Console.WriteLine("Creating PDF report...");
            orchestrator?.CreateReport(ReportType.Pdf);

            Console.WriteLine("\nCreating Excel report...");
            orchestrator?.CreateReport(ReportType.Excel);
        }
    }
}