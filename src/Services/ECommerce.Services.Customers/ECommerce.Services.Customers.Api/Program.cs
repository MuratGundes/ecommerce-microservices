using System.Reflection;
using Customers.Api.Extensions.ApplicationBuilderExtensions;
using Customers.Api.Extensions.ServiceCollectionExtensions;
using ECommerce.Services.Customers;
using Hellang.Middleware.ProblemDetails;
using MicroBootstrap.Core.Dependency;
using MicroBootstrap.Logging;
using MicroBootstrap.Security.Jwt;
using MicroBootstrap.Swagger;
using MicroBootstrap.Web;
using MicroBootstrap.Web.Extensions;
using MicroBootstrap.Web.Extensions.ApplicationBuilderExtensions;
using MicroBootstrap.Web.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((env, c) =>
{
    // Handling Captive Dependency Problem
    // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
    // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
    // https://blog.ploeh.dk/2014/06/02/captive-dependency/
    if (env.HostingEnvironment.IsDevelopment() || env.HostingEnvironment.IsEnvironment("tests") ||
        env.HostingEnvironment.IsStaging())
    {
        c.ValidateScopes = true;
    }
});

builder.Services.AddControllers(options =>
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddApplicationOptions(builder.Configuration);
var loggingOptions = builder.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();

builder.AddCompression();
builder.AddCustomProblemDetails();

builder.Host.AddCustomSerilog(config =>
{
    config.WriteTo.File(
        Customers.Api.Program.GetLogPath(builder.Environment, loggingOptions) ?? "../logs/customers-service.log",
        outputTemplate: loggingOptions?.LogTemplate ??
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true);
});

builder.AddCustomSwagger(builder.Configuration, typeof(CustomersRoot).Assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.AddCustomAuthorization(
    rolePolicies: new List<RolePolicy>
    {
        new()
        {
            Name = CustomersConstants.Role.Admin, Roles = new List<string> { CustomersConstants.Role.Admin }
        },
        new()
        {
            Name = CustomersConstants.Role.User, Roles = new List<string> { CustomersConstants.Role.User }
        }
    });

builder.AddModulesServices();

var app = builder.Build();

var environment = app.Environment;

if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
{
    app.UseDeveloperExceptionPage();

    // Minimal Api not supported versioning in .net 6
    app.UseCustomSwagger();
}


ServiceActivator.Configure(app.Services);

app.UseProblemDetails();

app.UseRouting();

app.UseAppCors();

app.UseSerilogRequestLogging();

app.UseCustomHealthCheck();

await app.ConfigureModules();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapModulesEndpoints();

// automatic discover minimal endpoints
app.MapEndpoints();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

await app.RunAsync();

namespace Customers.Api
{
    public partial class Program
    {
        public static string? GetLogPath(IWebHostEnvironment env, LoggerOptions loggerOptions)
            => env.IsDevelopment() ? loggerOptions.DevelopmentLogPath : loggerOptions.ProductionLogPath;
    }
}
