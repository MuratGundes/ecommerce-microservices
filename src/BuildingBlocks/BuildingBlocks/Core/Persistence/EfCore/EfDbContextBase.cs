using System.Collections.Immutable;
using System.Data;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Domain.Model;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Core.Persistence.EfCore;

public abstract class EfDbContextBase :
    DbContext,
    IDomainEventContext,
    IDbFacadeResolver,
    IDbContext
{
    private readonly IDomainEventPublisher _domainEventPublisher;

    private IDbContextTransaction? _currentTransaction;

    protected EfDbContextBase(DbContextOptions options) : base(options)
    {
    }

    protected EfDbContextBase(DbContextOptions options, IDomainEventPublisher domainEventPublisher) : base(options)
    {
        _domainEventPublisher = domainEventPublisher;
        _domainEventPublisher = Guard.Against.Null(domainEventPublisher, nameof(domainEventPublisher));
        System.Diagnostics.Debug.WriteLine($"{GetType().Name}::ctor");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        AddingSofDeletes(modelBuilder);
    }

    private static void AddingSofDeletes(ModelBuilder builder)
    {
        var types = builder.Model.GetEntityTypes().Where(x => x.ClrType.IsAssignableTo(typeof(IHaveSoftDelete)));
        foreach (var entityType in types)
        {
            // 1. Add the IsDeleted property
            entityType.AddProperty("IsDeleted", typeof(bool));

            // 2. Create the query filter
            var parameter = Expression.Parameter(entityType.ClrType);

            // EF.Property<bool>(TEntity, "IsDeleted")
            var propertyMethodInfo = typeof(EF).GetMethod("Property").MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

            // EF.Property<bool>(TEntity, "IsDeleted") == false
            BinaryExpression compareExpression =
                Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));

            // TEntity => EF.Property<bool>(TEntity, "IsDeleted") == false
            var lambda = Expression.Lambda(compareExpression, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://youtu.be/x-UXUGVLMj8?t=4515
        // https://enterprisecraftsmanship.com/posts/domain-events-simple-reliable-solution/
        // https://lostechies.com/jimmybogard/2014/05/13/a-better-domain-events-pattern/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/
        // https://ardalis.com/immediate-domain-event-salvation-with-mediatr/
        await _domainEventPublisher.PublishAsync(GetAllUncommittedEvents().ToArray(), cancellationToken);

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers)
        // performed through the DbContext will be committed
        var result = await SaveChangesAsync(cancellationToken);

        return true;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // https://www.meziantou.net/entity-framework-core-generate-tracking-columns.htm
    // https://www.meziantou.net/entity-framework-core-soft-delete-using-query-filters.htm
    private void OnBeforeSaving()
    {
        var now = DateTime.Now;

        // var userId = GetCurrentUser(); // TODO: Get current user
        foreach (var entry in ChangeTracker.Entries<IHaveAudit>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues["LastModified"] = now;
                    entry.CurrentValues["LastModifiedBy"] = 1;
                    break;
                case EntityState.Added:
                    entry.CurrentValues["Created"] = now;
                    entry.CurrentValues["CreatedBy"] = 1;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IHaveCreator>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues["Created"] = now;
                entry.CurrentValues["CreatedBy"] = 1;
            }
        }

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IHaveSoftDelete)
                        entry.CurrentValues["IsDeleted"] = false;
                    break;
                case EntityState.Deleted:
                    if (entry.Entity is IHaveSoftDelete)
                    {
                        entry.State = EntityState.Modified;
                        Entry(entry.Entity).CurrentValues["IsDeleted"] = true;
                    }

                    break;
            }
        }
    }

    public Task RetryOnExceptionAsync(Func<Task> operation)
    {
        return Database.CreateExecutionStrategy().ExecuteAsync(operation);
    }

    public Task<TResult> RetryOnExceptionAsync<TResult>(Func<Task<TResult>> operation)
    {
        return Database.CreateExecutionStrategy().ExecuteAsync(operation);
    }

    public IReadOnlyList<IDomainEvent> GetAllUncommittedEvents()
    {
        var domainEvents = ChangeTracker
            .Entries<IHaveEventSourcedAggregate>()
            .Where(x => x.Entity.GetUncommittedEvents().Any())
            .SelectMany(x => x.Entity.FlushUncommittedEvents())
            .ToList();

        return domainEvents.ToImmutableList();
    }

    public Task ExecuteTransactionalAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                await action();

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public Task<T> ExecuteTransactionalAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                var result = await action();

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
