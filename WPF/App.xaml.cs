using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Services;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Http;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WPF.Configuration;
using WPF.ViewModels;

namespace WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

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
            // 1. Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    "Server=localhost;Database=MedRecordsDB;Trusted_Connection=true;TrustServerCertificate=true;"));

            // 2. HTTP clients
            services.AddHttpClient<ILoginService, LoginService>();
            services.AddHttpClient<IApiConnectionProvider, ApiService>();
            services.AddHttpClient<IPatientHttpClient, PatientHttpClient>();

            // 3. HTTP-abstraction registration
            services.AddSingleton<AppSettings>();
            services.AddSingleton<IPatientHttpClient, PatientHttpClient>();

            // 4. Repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<ITestCatalogRepository, TestCatalogRepository>();
            services.AddScoped<IGenericRepository<Patient>, GenericRepository<Patient>>();
            services.AddScoped<IGenericRepository<Visit>, GenericRepository<Visit>>();
            services.AddScoped<ILabResultsMappingService, LabResultsMappingService>();

            // 5. Services
            services.AddScoped<AppSettings>();
            services.AddScoped<LoginService>();
            services.AddScoped<PatientService>();
            services.AddScoped<IUserService, UserService>();

            // 6. ViewModels / Windows
            services.AddScoped<MainViewModel>();
            services.AddScoped<MainWindow>();
        }
    }
}