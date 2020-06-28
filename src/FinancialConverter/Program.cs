namespace FinancialConverter
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;

    public class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal(
                    (Exception)eventArgs.ExceptionObject,
                    "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var container = ConfigureServices(configuration);
            var logger = container.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting Financial Converter.");

            try
            {
                var runner = container.GetRequiredService<CodaConverter>();

                runner.Start();

                logger.LogInformation("Running... Press CTRL + C to exit.");
                ct.WaitHandle.WaitOne();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            logger.LogInformation("Stopping...");
            ct.WaitHandle.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            var builder = new ContainerBuilder();

            builder
                .RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetService<ILoggerFactory>();

            services
                .Configure<CodaConverterConfiguration>(configuration.GetSection(CodaConverterConfiguration.ConfigurationPath));

            builder
                .RegisterType<CodaConverter>()
                .SingleInstance();

            builder
                .Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
