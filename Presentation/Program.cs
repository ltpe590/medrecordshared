using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Models;
using Infrastructure.Repositories;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add repository pattern
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// Add other services
builder.Services.AddScoped<IGenericRepository<Patient>, IGenericRepository<Patient>>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add your DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repository pattern
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add specific repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddScoped<ITestCatalogRepository, TestCatalogRepository>();
builder.Services.AddScoped<ILabResultsMappingService, LabResultsMappingService>();