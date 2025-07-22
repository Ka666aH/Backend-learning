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
namespace _05DI
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<SingletonService>();
            //services.AddSingleton<IServiceScopeFactory>(); //Он уже встроен в DI контейнер
            services.AddScoped<ScopedService>();
            var serviceProvider = services.BuildServiceProvider();
            var singletonProcessor = serviceProvider.GetService<SingletonService>();
            singletonProcessor?.Work();

        }
        public class SingletonService
        {
            private readonly IServiceScopeFactory serviceScopeFactory;
            public SingletonService(IServiceScopeFactory _serviceScopeFactory)
            {
                serviceScopeFactory = _serviceScopeFactory;
            }
            public void Work()
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetService<ScopedService>();
                    scopedService?.Work();
                }
                Console.WriteLine("SingletonService work.");
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
