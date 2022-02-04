using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Email;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Outbox.EF;
using BuildingBlocks.Messaging.Transport.Rabbitmq;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Scheduling.Hangfire;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(
        this WebApplicationBuilder builder,
        IConfiguration configuration)
    {
        AddInfrastructure(builder.Services, configuration);

        return builder;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        SnowFlakIdGenerator.Configure(1);
        services.AddCore();

        services.AddMonitoring(healthChecksBuilder =>
        {
            healthChecksBuilder.AddNpgSql(
                configuration.GetConnectionString("CustomersServiceConnection"),
                name: "Customers-Postgres-Check",
                tags: new[] { "customers-postgres" });

            var rabbitMqOptions = configuration.GetOptions<RabbitConfiguration>(nameof(RabbitConfiguration));

            healthChecksBuilder.AddRabbitMQ(
                $"amqp://{rabbitMqOptions.UserName}:{rabbitMqOptions.Password}@{rabbitMqOptions.HostName}{rabbitMqOptions.VirtualHost}",
                name: "CustomersService-RabbitMQ-Check",
                tags: new[] { "customers-rabbitmq" });
        });

        services.AddHangfireScheduler(configuration);

        services.AddMessaging(configuration)
            .AddEntityFrameworkOutbox<OutboxDataContext>(configuration);

        services.AddRabbitMqTransport(configuration);

        services.AddEmailService(configuration);
        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(TxBehavior<,>));
        });
        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCachingRequestPolicies(new List<Assembly> { Assembly.GetExecutingAssembly() });
        services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

        return services;
    }
}
