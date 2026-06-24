using farmmanager.Data;
using farmmanager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace farmmanager.Services
{
    public class WorkerService
    {
        public async Task<List<Worker>> GetAllAsync(bool includeInactive = false)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.Workers.AsQueryable();
            if (!includeInactive) query = query.Where(w => w.IsActive);
            return await query.OrderBy(w => w.Group).ThenBy(w => w.Name).ToListAsync();
        }

        public async Task<Worker?> GetByIdAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            return await ctx.Workers.FindAsync(id);
        }

        public async Task<Worker> CreateAsync(Worker worker)
        {
            await using var ctx = new PlantationDbContext();
            ctx.Workers.Add(worker);
            await ctx.SaveChangesAsync();
            return worker;
        }

        public async Task UpdateAsync(Worker worker)
        {
            await using var ctx = new PlantationDbContext();
            ctx.Workers.Update(worker);
            await ctx.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            var worker = await ctx.Workers.FindAsync(id);
            if (worker != null)
            {
                worker.IsActive = false;
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetGroupsAsync()
        {
            await using var ctx = new PlantationDbContext();
            return await ctx.Workers
                .Where(w => w.IsActive)
                .Select(w => w.Group)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }
    }
}
