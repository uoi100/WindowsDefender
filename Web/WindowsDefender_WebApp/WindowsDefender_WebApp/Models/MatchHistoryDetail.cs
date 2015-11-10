namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MatchHistoryDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int HistoryId { get; set; }

        public int MatchId { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        public bool WonGame { get; set; }

        [Required]
        [StringLength(1)]
        public string Team { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual MatchHistory MatchHistory { get; set; }
    }
}
