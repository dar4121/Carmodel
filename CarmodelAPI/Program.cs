using CarmodelAPI.Models;
using CarmodelAPI.Repository;
using CarmodelAPI.Repository.Interface;
using CarmodelAPI.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Register validators
builder.Services.AddScoped<IValidator<CarmodelAPI.Models.ViewModels.CarModelCreateUpdateViewModel>, CarModelCreateUpdateValidator>();
builder.Services.AddScoped<IValidator<CarmodelAPI.Models.ViewModels.ImageUploadViewModel>, ImageUploadValidator>();

// Register DbContext
builder.Services.AddDbContext<CarModelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CarModelDB")));

// Register Repository
builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Set connection string
CarModelContext.SetConnectionString(
    builder.Configuration.GetConnectionString("CarModelDB"));

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

// Serve static files
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
