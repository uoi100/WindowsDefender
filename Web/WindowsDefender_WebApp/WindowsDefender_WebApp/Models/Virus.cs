namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Viruses")]
    public partial class Virus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VirusId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public int Speed { get; set; }

        public int Armour { get; set; }

        public int Resistance { get; set; }

        public int Health { get; set; }

        public int? SpAbilityId { get; set; }

        [StringLength(100)]
        public string ImageUrl { get; set; }

        public virtual SpecialAbility SpecialAbility { get; set; }
    }
}
