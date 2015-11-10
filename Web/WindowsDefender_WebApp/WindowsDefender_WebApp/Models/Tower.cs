namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tower
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TowerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public int Price { get; set; }

        public int Damage { get; set; }

        public int Speed { get; set; }

        public int Range { get; set; }

        public int LevelUnlocked { get; set; }

        [StringLength(100)]
        public string ImageUrl { get; set; }

        public int? SpAbilityId { get; set; }

        public virtual SpecialAbility SpecialAbility { get; set; }
    }
}
