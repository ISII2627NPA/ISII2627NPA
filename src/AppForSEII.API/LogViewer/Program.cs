using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LogViewer;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Cargar la configuración desde el archivo appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Instanciar el suscriptor y conectarlo a RabbitMQ
        var subscriber = new Subscriber(configuration);
        await subscriber.StartAsync();

        // 3. Mantener la consola abierta para escuchar los mensajes en vivo
        Console.WriteLine("LogViewer iniciado correctamente.");
        Console.WriteLine("Esperando logs de la API... (Pulsa [Enter] para salir)\n");
        Console.ReadLine();
    }
}
