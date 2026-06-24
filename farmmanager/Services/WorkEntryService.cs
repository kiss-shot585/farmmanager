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
    public record WorkerSummary(
    int WorkerId,
    string WorkerName,
    string Group,
    int TotalDays,
    decimal TotalAmountEarned,
    decimal TotalObjectivePlanned,
    decimal TotalObjectiveAttained);

    public record ParcelSummary(
        int ParcelId,
        string ParcelName,
        string ParcelCode,
        int TotalEntries,
        decimal TotalAmountSpent,
        decimal TotalAreaWorked);

    public record ActivitySummary(
        int ActivityId,
        string ActivityName,
        string Unit,
        int TotalEntries,
        decimal TotalObjectivePlanned,
        decimal TotalObjectiveAttained,
        decimal TotalAmountSpent);

    public class WorkEntryService
    {
        public async Task<List<WorkEntry>> GetEntriesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? workerId = null,
            int? parcelId = null,
            int? activityId = null)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.WorkEntries
                .Include(e => e.Worker)
                .Include(e => e.Parcel)
                .Include(e => e.Activity)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(e => e.Date <= toDate.Value);
            if (workerId.HasValue) query = query.Where(e => e.WorkerId == workerId.Value);
            if (parcelId.HasValue) query = query.Where(e => e.ParcelId == parcelId.Value);
            if (activityId.HasValue) query = query.Where(e => e.ActivityId == activityId.Value);

            return await query.OrderByDescending(e => e.Date).ThenBy(e => e.Worker.Name).ToListAsync();
        }

        public async Task<WorkEntry> CreateAsync(WorkEntry entry)
        {
            await using var ctx = new PlantationDbContext();

            // Load activity to compute amount
            var activity = await ctx.Activities.FindAsync(entry.ActivityId);
            if (activity != null)
            {
                entry.AmountEarned = entry.DailyWage > 0
                    ? entry.DailyWage
                    : (entry.ObjectiveAttained / entry.ObjectivePlanned) * activity.UnitRate;
            }

            entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;

            ctx.WorkEntries.Add(entry);
            await ctx.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateAsync(WorkEntry entry)
        {
            await using var ctx = new PlantationDbContext();

            var activity = await ctx.Activities.FindAsync(entry.ActivityId);
            if (activity != null)
            {
                entry.AmountEarned = entry.DailyWage > 0
                    ? entry.DailyWage
                    : (entry.ObjectiveAttained / entry.ObjectivePlanned) * activity.UnitRate; 
            }

            entry.UpdatedAt = DateTime.Now;
            ctx.WorkEntries.Update(entry);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var ctx = new PlantationDbContext();
            await ctx.WorkEntries.Where(e => e.Id == id).ExecuteDeleteAsync();
        }

        // ── Financial Summary Queries ──────────────────────────────────────────

        public async Task<List<WorkerSummary>> GetWorkerSummariesAsync(
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.WorkEntries.Include(e => e.Worker).AsQueryable();
            if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(e => e.Date <= toDate.Value);

            return await query
                .GroupBy(e => new { e.WorkerId, e.Worker.Name, e.Worker.Group })
                .Select(g => new WorkerSummary(
                    g.Key.WorkerId,
                    g.Key.Name,
                    g.Key.Group,
                    g.Count(),
                    g.Sum(e => e.AmountEarned),
                    g.Sum(e => e.ObjectivePlanned),
                    g.Sum(e => e.ObjectiveAttained)))
                .OrderBy(s => s.Group)
                .ThenBy(s => s.WorkerName)
                .ToListAsync();
        }

        public async Task<List<ParcelSummary>> GetParcelSummariesAsync(
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.WorkEntries.Include(e => e.Parcel).AsQueryable();
            if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(e => e.Date <= toDate.Value);

            return await query
                .GroupBy(e => new { e.ParcelId, e.Parcel.Name, e.Parcel.Code })
                .Select(g => new ParcelSummary(
                    g.Key.ParcelId,
                    g.Key.Name,
                    g.Key.Code,
                    g.Count(),
                    g.Sum(e => e.AmountEarned),
                    g.Sum(e => e.ObjectiveAttained)))
                .OrderBy(s => s.ParcelName)
                .ToListAsync();
        }

        public async Task<List<ActivitySummary>> GetActivitySummariesAsync(
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.WorkEntries.Include(e => e.Activity).AsQueryable();
            if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(e => e.Date <= toDate.Value);

            return await query
                .GroupBy(e => new { e.ActivityId, e.Activity.Name, e.Activity.Unit })
                .Select(g => new ActivitySummary(
                    g.Key.ActivityId,
                    g.Key.Name,
                    g.Key.Unit,
                    g.Count(),
                    g.Sum(e => e.ObjectivePlanned),
                    g.Sum(e => e.ObjectiveAttained),
                    g.Sum(e => e.AmountEarned)))
                .OrderBy(s => s.ActivityName)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSpentAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            await using var ctx = new PlantationDbContext();
            var query = ctx.WorkEntries.AsQueryable();
            if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(e => e.Date <= toDate.Value);
            return await query.SumAsync(e => e.AmountEarned);
        }
    }

}
