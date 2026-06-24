using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace farmmanager.Models
{
    public class Activite
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Unit rate paid per unit of work (e.g., per hectare, per kg, per day)</summary>
        public decimal UnitRate { get; set; }

        [MaxLength(30)]
        public string Unit { get; set; } = "day"; // day, kg, hectare, unit, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<WorkEntry> WorkEntries { get; set; } = new List<WorkEntry>();
    }

}
