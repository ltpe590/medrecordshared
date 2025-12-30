using Core.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Http;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WPF.Models.ViewModels;

namespace WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer("Server=localhost;Database=MedRecordsDB;Trusted_Connection=true;TrustServerCertificate=true;"));
            
            // Add HTTP client with Infrastructure services
            services.AddHttpClient<IApiConnectionProvider, ApiService>();

            // Repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<ITestCatalogRepository, TestCatalogRepository>();
            services.AddScoped<IGenericRepository<Patient>, GenericRepository<Patient>>();
            services.AddScoped<IGenericRepository<Visit>, GenericRepository<Visit>>();

            // Services
            services.AddHttpClient<ApiService>();
            services.AddHttpClient<LoginService>();
            services.AddScoped<AppSettings>();
            services.AddScoped<LoginService>();
            services.AddScoped<LoginService>();
            services.AddScoped<PatientService>();
            
            
            // ViewModels
            services.AddScoped<MainViewModel>();

            // Windows
            services.AddScoped<MainWindow>();
        }
    }
}