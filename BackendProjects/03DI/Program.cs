using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//Пример реализации фабрики от DeepSeek

namespace _03DI
{
    class Program
    {
        static void Main()
        {
            // 1. Создаём простую конфигурацию
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StorageType"] = "Local" 
                })
                .Build();

            // 2. Регистрируем зависимости
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config); // даём доступ к конфигурации
            services.AddTransient<StorageFactory>();      // создаём фабрику
            services.AddTransient<DataProcessor>();       // создаём процессор

            var provider = services.BuildServiceProvider();

            // 3. Используем
            var processor = provider.GetRequiredService<DataProcessor>();
            processor.ProcessData("My important data");
        }
        // Требуемые компоненты:
        public enum StorageType { Local, Cloud }

        public interface IStorage
        {
            void Save(string data);
        }

        public class LocalStorage : IStorage
        {
            public void Save(string data)
                => Console.WriteLine($"Saving locally: {data}");
        }

        public class CloudStorage : IStorage
        {
            public void Save(string data)
                => Console.WriteLine($"Saving to cloud: {data}");
        }

        public class StorageFactory
        {
            private readonly IConfiguration config;
            public StorageFactory(IConfiguration _config)
                => config = _config;

            public IStorage Create()
            {
                var storageType = Enum.Parse<StorageType>(config["StorageType"]);
                return storageType
                switch
                {
                    StorageType.Cloud => new CloudStorage(),
                    StorageType.Local => new LocalStorage(),
                    _ => throw new InvalidOperationException("Неизвестный тип хранилища.")
                };
            }
        }

        public class DataProcessor
        {
            private readonly IStorage storage;
            public DataProcessor(StorageFactory factory)
                => storage = factory.Create();

            public void ProcessData(string data)
                => storage.Save(data);
        }
    }
}