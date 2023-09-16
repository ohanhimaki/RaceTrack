using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RaceTrack.Core.Messaging;

namespace RaceTrack
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         private ServiceProvider serviceProvider;
        
            protected override void OnStartup(StartupEventArgs e)
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                serviceProvider = serviceCollection.BuildServiceProvider();
        
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        
            private void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<EventAggregator>();
                services.AddSingleton<MainWindow>();
            }
    }
}