using GameCloud.Business.Extensions;
using GameCloud.Functioning.Extensions;
using GameCloud.Persistence.Contexts;
using GameCloud.WebAPI.Exceptions;
using GameCloud.WebAPI.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequireGameKeyFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<UserAlreadyExistExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidUserClaimsExceptionHandler>();
builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidCredentialExceptionHandler>();
builder.Services.AddExceptionHandler<FunctionCallExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddScriptingServices();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    using (var scope = app.Services.CreateScope())
    {
        try 
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameCloudDbContext>();
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration error: {ex.Message}");
            throw;
        }
    }}

app.UseHttpsRedirection();

app.UseExceptionHandler(_ => { });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();