using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareFlow.Infrastructure.Data;

public class AppDbContextFactory
    : IDesignTimeDbContextFactory<AppDBContext>
{
    public AppDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();

        optionsBuilder.UseSqlite("Data Source=careflow.db");

        return new AppDBContext(optionsBuilder.Options);
    }
}
