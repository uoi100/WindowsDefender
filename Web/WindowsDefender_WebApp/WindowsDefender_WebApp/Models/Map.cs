namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Map
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MapId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public int DifficultyRating { get; set; }

        [StringLength(100)]
        public string ImageUrl { get; set; }
    }
}
