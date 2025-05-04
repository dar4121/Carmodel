using CarmodelAPI.Models;
using CarmodelAPI.Repository;
using CarmodelAPI.Repository.Interface;
using CarmodelAPI.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddScoped<IValidator<CarmodelAPI.Models.ViewModels.CarModelCreateUpdateViewModel>, CarModelCreateUpdateValidator>();
builder.Services.AddScoped<IValidator<CarmodelAPI.Models.ViewModels.ImageUploadViewModel>, ImageUploadValidator>();


builder.Services.AddDbContext<CarModelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CarModelDB")));


builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


CarModelContext.SetConnectionString(
    builder.Configuration.GetConnectionString("CarModelDB"));

app.UseHttpsRedirection();


app.UseCors("AllowAll");


app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
