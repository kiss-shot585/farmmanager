using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.Models
{
    public class WorkEntry
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        // Foreign keys
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ParcelId { get; set; }
        public string ParcelName { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ObjectiveOfDay { get; set; } = string.Empty;

        /// <summary>Quantity planned (e.g., 2.5 hectares, 100 kg)</summary>
        public decimal ObjectivePlanned { get; set; }

        /// <summary>Quantity actually attained/completed</summary>
        public decimal ObjectiveAttained { get; set; }

        /// <summary>Daily wage or fixed amount override (0 = use activity rate)</summary>
        public decimal DailyWage { get; set; }

        /// <summary>Auto-calculated: (ObjectiveAttained / ObjectivePlanned) * Activity.UnitRate (or DailyWage if set)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountEarned { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Worker Worker { get; set; } = null!;
        public PlantationParcel Parcel { get; set; } = null!;
        public Activite Activity { get; set; } = null!;

        /// <summary>Completion percentage</summary>
        [NotMapped]
        public decimal CompletionRate => ObjectivePlanned > 0
            ? Math.Round(ObjectiveAttained / ObjectivePlanned * 100, 1)
            : 0;
    }

}
