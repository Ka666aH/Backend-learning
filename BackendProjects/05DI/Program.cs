//Задача 5: Валидация зависимостей
//Цель: Научиться обрабатывать сложные сценарии.

//Задание:
//Зарегистрируйте ScopedService и SingletonService как указано
//Попробуйте получить SingletonService вне области видимости
//Реализуйте корректное решение для SingletonService через IServiceScopeFactory

//Новые концепты:
//Проблемы Scoped в Singleton
//Использование IServiceScopeFactory
using Microsoft.Extensions.DependencyInjection;
using static _05DI.Program;
namespace _05DI
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<SingletonService>();
            //services.AddSingleton<IServiceScopeFactory>(); //Он уже встроен в DI контейнер (Вариант 1 Внутренная реализация)
            services.AddScoped<ScopedService>();
            var serviceProvider = services.BuildServiceProvider();

            //Вариант 1 Внутренная реализация
            var singletonProcessor = serviceProvider.GetService<SingletonService>();
            singletonProcessor?.Work();

            //Вариант 2 Общая реализация
            //var scope = serviceProvider.CreateScope();
            //var scopedProvider = scope.ServiceProvider;
            //var scopedSingletonProcessor = scopedProvider.GetService<SingletonService>();
            //scopedSingletonProcessor?.Work();
            Console.ReadKey();

        }
        public class SingletonService
        {
            //Вариант 1 Внутренная реализация
            private readonly IServiceScopeFactory serviceScopeFactory;
            public SingletonService(IServiceScopeFactory _serviceScopeFactory)
            {
                serviceScopeFactory = _serviceScopeFactory;
            }

            //Вариант 2 Общая реализация
            //private readonly ScopedService scopedService;
            //public SingletonService(ScopedService _scopedService)
            //{
            //    scopedService = _scopedService;
            //}

            public void Work()
            {
                //Вариант 1 Внутренная реализация
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetService<ScopedService>();
                    scopedService?.Work();
                }
                Console.WriteLine("SingletonService work.");

                //Вариант 2 Общая реализация
                //scopedService.Work();
                //Console.WriteLine("SingletonService work.");
            }
        }
        public class ScopedService()
        {
            public void Work()
            {
                Console.WriteLine("ScopedService work.");
            }
        }
    }
}
