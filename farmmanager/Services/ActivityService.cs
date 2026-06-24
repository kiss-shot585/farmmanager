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
    public class ActivityService
    {
        public async Task<List<Activite>> GetAllAsync(bool includeInactive = false)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.Activities.AsQueryable();
            if (!includeInactive) query = query.Where(a => a.IsActive);
            return await query.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<Activite?> GetByIdAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            return await ctx.Activities.FindAsync(id);
        }

        public async Task<Activite> CreateAsync(Activite activity)
        {
            
            await using var ctx = new PlantationDbContext();
            ctx.Activities.Add(activity);
            await ctx.SaveChangesAsync();
            return activity;
        }

        public async Task UpdateAsync(Activite activity)
        {
            await using var ctx = new PlantationDbContext();
            ctx.Activities.Update(activity);
            await ctx.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            await ctx.Activities.Where(a => a.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsActive, false));
        }
    }


}
