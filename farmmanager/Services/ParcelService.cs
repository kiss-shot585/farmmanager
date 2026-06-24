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
    public class ParcelService
    {
        public async Task<List<PlantationParcel>> GetAllAsync(bool includeInactive = false)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.Parcels.AsQueryable();
            if (!includeInactive) query = query.Where(p => p.IsActive);
            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<PlantationParcel?> GetByIdAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            return await ctx.Parcels.FindAsync(id);
        }

        public async Task<PlantationParcel> CreateAsync(PlantationParcel parcel)
        {
            await using var ctx = new PlantationDbContext();
            ctx.Parcels.Add(parcel);
            await ctx.SaveChangesAsync();
            return parcel;
        }

        public async Task UpdateAsync(PlantationParcel parcel)
        {
            await using var ctx = new PlantationDbContext();
            ctx.Parcels.Update(parcel);
            await ctx.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            var parcel = await ctx.Parcels.FindAsync(id);
            if (parcel != null)
            {
                parcel.IsActive = false;
                await ctx.Parcels.Where(p => p.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false));
            }
        }
    }
}
