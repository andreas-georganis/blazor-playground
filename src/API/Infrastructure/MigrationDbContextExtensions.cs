using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace BlazorPlayground.API.Infrastructure;

internal static class MigrateDbContextExtensions
{
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // Enable migration tracing
        //services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));
        //services.AddDbContextFactory<TContext>();
        return services.AddHostedService<MigrationHostedService<TContext>>();
    }

    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddSingleton<IDbSeeder<TContext>, TDbSeeder>();
        return services.AddMigration<TContext>();
    }

    private class MigrationHostedService<TContext>
        : BackgroundService where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger<MigrationHostedService<TContext>> _logger;
        private Func<TContext, CancellationToken, Task> _seeder;
        
        public MigrationHostedService(
            IDbContextFactory<TContext> contextFactory, 
            ILogger<MigrationHostedService<TContext>> logger,
            Func<TContext,CancellationToken, Task> seeder)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _seeder = seeder;
        }

        public MigrationHostedService(
            IDbContextFactory<TContext> contextFactory, 
            ILogger<MigrationHostedService<TContext>> logger,
            IDbSeeder<TContext>? seeder = null) : this(
            contextFactory, 
            logger, 
            seeder != null ? 
                async (context, ct) => await seeder.SeedAsync(context, ct) 
                : (_, _) => Task.CompletedTask)
        {
        }
        
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            using var migrationActivity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

            try
            {
                _logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                
                var strategy = context.Database.CreateExecutionStrategy();
                
                await strategy.ExecuteAsync( async () =>
                {
                    using var strategyActivity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

                    try
                    {
                        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
                    
                        await _seeder.Invoke(context, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        //activity?.SetExceptionTags(ex);
                        strategyActivity?.AddException(ex);

                        throw;
                    }
                    
                    
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                //activity?.SetExceptionTags(ex);
                migrationActivity?.AddException(ex);
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context, CancellationToken cancellationToken = default);
}