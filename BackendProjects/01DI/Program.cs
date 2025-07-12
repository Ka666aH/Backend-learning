using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01DI
{
    //Пример реализации DI от DeepSeek

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Создаем коллекцию сервисов
            IServiceCollection services = new ServiceCollection();

            // 2. РЕГИСТРИРУЕМ сервисы и их зависимости в контейнере.
            //    Говорим: "Когда кто-то запросит IMessageService, используй SmsService (или EmailService) с жизненным циклом Transient"
            services.AddTransient<IMessageService, SmsService>(); // Попробуйте заменить на EmailService
                                                                  // services.AddTransient<IMessageService, EmailService>();
            services.AddTransient<MessageProcessor, MessageProcessor>(); // Контейнер сам разберет зависимость MessageProcessor -> IMessageService

            // 3. Строим провайдер сервисов (контейнер)
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // 4. ЗАПРАШИВАЕМ объект из контейнера.
            //    Контейнер автоматически создаст MessageProcessor и внедрит в него зарегистрированную реализацию IMessageService (SmsService)
            var processor = serviceProvider.GetService<MessageProcessor>();

            // 5. Используем объект (все зависимости уже внедрены!)
            await processor.ProcessMessageAsync();

            // 6. Демонстрация Transient: каждый GetService дает НОВЫЙ экземпляр
            //var service1 = serviceProvider.GetService<IMessageService>();
            //var service2 = serviceProvider.GetService<IMessageService>();
            //Console.WriteLine($"Transient the same? {ReferenceEquals(service1, service2)}"); // False

            // 7. Демонстрация Singleton (раскомментируйте регистрацию ниже и в классе ServiceProvider)
            // services.AddSingleton<IMessageService, SmsService>();
            // var service3 = serviceProvider.GetService<IMessageService>();
            // var service4 = serviceProvider.GetService<IMessageService>();
            // Console.WriteLine($"Singleton the same? {ReferenceEquals(service3, service4)}"); // True

            Console.ReadKey();
        }
    }
    // Интерфейс зависимости (абстракция)
    public interface IMessageService
    {
        Task<string> GetMessageAsync();
    }

    // Реализация зависимости №1 (Деталь)
    public class EmailService : IMessageService
    {
        public async Task<string> GetMessageAsync()
        {
            await Task.Delay(100); // Имитация асинхронной работы
            return "Hello from Email Service!";
        }
    }

    // Реализация зависимости №2 (Деталь)
    public class SmsService : IMessageService
    {
        public async Task<string> GetMessageAsync()
        {
            await Task.Delay(50);
            return "Hello from SMS Service!";
        }
    }

    // Класс, который зависит от IMessageService (Consumer)
    public class MessageProcessor
    {
        private readonly IMessageService _messageService;

        // Зависимость ВНЕДРЯЕТСЯ через конструктор!
        public MessageProcessor(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task ProcessMessageAsync()
        {
            var message = await _messageService.GetMessageAsync();
            Console.WriteLine($"Processing: {message}");
        }
    }

    //DI Container

    public interface IServiceCollection
    {
        void AddSingleton<TService, TImplementation>() where TImplementation : TService;
        void AddTransient<TService, TImplementation>() where TImplementation : TService;
        IServiceProvider BuildServiceProvider();
    }

    public interface IServiceProvider
    {
        TService GetService<TService>();
    }

    public class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

        public void AddSingleton<TService, TImplementation>() where TImplementation : TService
        {
            _descriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
        }

        public void AddTransient<TService, TImplementation>() where TImplementation : TService
        {
            _descriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
        }

        public IServiceProvider BuildServiceProvider()
        {
            return new ServiceProvider(_descriptors);
        }
    }

    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();

        public ServiceProvider(IEnumerable<ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors.ToDictionary(d => d.ServiceType);
        }

        public TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        private object GetService(Type serviceType)
        {
            if (!_descriptors.TryGetValue(serviceType, out var descriptor))
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
            }

            // Реализация Singleton (очень упрощенная, без потокобезопасности)
            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                if (!_singletonInstances.TryGetValue(serviceType, out var instance))
                {
                    instance = CreateInstance(descriptor.ImplementationType);
                    _singletonInstances[serviceType] = instance;
                }
                return instance;
            }

            // Реализация Transient
            return CreateInstance(descriptor.ImplementationType);
        }

        private object CreateInstance(Type implementationType)
        {
            // Упрощенная реализация: находит первый конструктор и пытается разрешить его параметры
            var constructor = implementationType.GetConstructors().First();
            var parameters = constructor.GetParameters();
            var parameterInstances = parameters.Select(p => GetService(p.ParameterType)).ToArray();
            return constructor.Invoke(parameterInstances);
        }
    }

    public enum ServiceLifetime
    {
        Singleton,
        Transient
    }

    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }
    }
}
