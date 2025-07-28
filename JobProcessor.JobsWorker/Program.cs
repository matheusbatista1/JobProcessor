using JobProcessor.Infra;
using JobProcessor.Application;
using DotNetEnv;
using JobProcessor.JobsWorker;

var envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
Env.Load(envPath);

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();