using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CareFlow.Infrastructure.Data;
using CareFlow.Application.Interfaces;
using CareFlow.Infrastructure.Repositories;

namespace CareFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       string connectionString)
    {
        services.AddDbContext<AppDBContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
