using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using CactusPie.MapLocation.Minimap.Data;
using CactusPie.MapLocation.Minimap.Services;
using CactusPie.MapLocation.Minimap.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace CactusPie.MapLocation.Minimap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Logger logger = CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                logger.Error(args.ExceptionObject as Exception, "An exception occured");
            };
            
            IComponentContext container = CreateContainer();

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }

        private IComponentContext CreateContainer()
        {
            IConfiguration configuration = GetConfiguration();
            var mapConfiguration = configuration.Get<MapConfiguration>();

            if (mapConfiguration?.ListenIpAddress == null)
            {
                throw new InvalidOperationException("Cannot retrieve the listen IP address from the configuration!");
            }

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<PlotWindow>().AsSelf().InstancePerDependency();
            containerBuilder.RegisterType<MainWindow>().AsSelf().InstancePerDependency();
            containerBuilder.RegisterType<AddNewMapDialog>().AsSelf().InstancePerDependency();
            containerBuilder.RegisterType<MapDataReceiver>().As<IMapDataReceiver>().InstancePerDependency();
            containerBuilder.RegisterType<MapCreationDataManager>().As<IMapCreationDataManager>().SingleInstance();

            containerBuilder.Register(_ =>
            {
                IPAddress ipAddress = IPAddress.Parse(mapConfiguration.ListenIpAddress);
                return new IPEndPoint(ipAddress, mapConfiguration.ListenPort);
            }).AsSelf();

            containerBuilder.Register(_ => new UdpClient()).AsSelf();

            return containerBuilder.Build();
        }

        private static IConfiguration GetConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            return config;
        }

        private Logger CreateLogger()
        {
            Logger? logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .CreateLogger();

            return logger;
        }
    }
}