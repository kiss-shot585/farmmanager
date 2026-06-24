using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace farmmanager.Data
{
    internal class DatabaseInitializer
    {
        public static async Task InitializeAsync()
        {
            await using var context = new PlantationDbContext();
            await context.Database.MigrateAsync();
        }

        public static async Task EnsureCreatedAsync()
        {
            await using var context = new PlantationDbContext();
            // For development: ensures DB and schema exist without migrations
            await context.Database.EnsureCreatedAsync();
        }
    }
}
