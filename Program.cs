using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using APISignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure AWS options
var awsOptions = builder.Configuration.GetAWSOptions();
var credentials = new BasicAWSCredentials(
    builder.Configuration["AWS:AccessKey"],
    builder.Configuration["AWS:SecretKey"]
);
awsOptions.Credentials = credentials;
builder.Services.AddDefaultAWSOptions(awsOptions);

// Register AWS Services
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddAWSService<IAmazonS3>();

// Configure AWSSettings
builder.Services.Configure<AWSSettings>(builder.Configuration.GetSection("AWSSettings"));

// Add hosted services
builder.Services.AddHostedService<ReportConsumer>();
builder.Services.AddHostedService<NotificationConsumer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.MapHub<ReportHub>("/api/reportHub"); // Register SignalR hub route 

app.Run();