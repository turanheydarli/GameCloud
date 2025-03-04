using GameCloud.Business.Extensions;
using GameCloud.Caching.Extensions;
using GameCloud.Functioning.Extensions;
using GameCloud.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDistributedCache(builder.Configuration);
builder.Services.AddScriptingServices();

builder.Services.AddProblemDetails();

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();