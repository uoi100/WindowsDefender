namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ThreadPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PostId { get; set; }

        public int ThreadId { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [Required]
        public string Post { get; set; }

        public DateTime Date { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual Thread Thread { get; set; }
    }
}
