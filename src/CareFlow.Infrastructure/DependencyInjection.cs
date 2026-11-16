using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CareFlow.Infrastructure.Data;

namespace CareFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       string connectionString)
    {
        services.AddDbContext<AppDBContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}
