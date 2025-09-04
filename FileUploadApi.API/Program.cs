using Amazon.S3;
using FileUploadApi.Domain.Options;
using FileUploadApi.Application.Services;
using FileUploadApi.Domain.Repositories;
using FileUploadApi.Infrastructure.Data;
using FileUploadApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// S3
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
    
    var config = new AmazonS3Config
    {
        ServiceURL = options.EndpointUrl,
        ForcePathStyle = true,
        AuthenticationRegion = options.Region
    };
    return new AmazonS3Client(
        options.AccessKey,
        options.SecretKey,
        config
    );
});

builder.Services.AddScoped<IStorageService, S3StorageService>();


builder.Services.AddScoped<IUserPhotoRepository, UserPhotoRepository>();
builder.Services.AddScoped<IUserPhotoService, UserPhotoService>();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();