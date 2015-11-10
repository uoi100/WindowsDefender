namespace WindowsDefender_WebApp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using Microsoft.AspNet.Identity;

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class DB : IdentityDbContext<ApplicationUser>
    {
        public DB() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static DB Create()
        {
            return new DB();
        }

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Friend> Friends { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<MatchHistory> MatchHistories { get; set; }
        public virtual DbSet<MatchHistoryDetail> MatchHistoryDetails { get; set; }
        public virtual DbSet<SpecialAbility> SpecialAbilities { get; set; }
        public virtual DbSet<ThreadPost> ThreadPosts { get; set; }
        public virtual DbSet<Thread> Threads { get; set; }
        public virtual DbSet<Tower> Towers { get; set; }
        public virtual DbSet<Virus> Viruses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Map>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Map>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Map>()
                .Property(e => e.ImageUrl)
                .IsUnicode(false);

            modelBuilder.Entity<MatchHistory>()
                .HasMany(e => e.MatchHistoryDetails)
                .WithRequired(e => e.MatchHistory)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MatchHistoryDetail>()
                .Property(e => e.Team)
                .IsFixedLength();

            modelBuilder.Entity<SpecialAbility>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<SpecialAbility>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<ThreadPost>()
                .Property(e => e.Post)
                .IsUnicode(false);

            modelBuilder.Entity<Thread>()
                .Property(e => e.Subject)
                .IsUnicode(false);

            modelBuilder.Entity<Thread>()
                .HasMany(e => e.ThreadPosts)
                .WithRequired(e => e.Thread)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tower>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Tower>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Tower>()
                .Property(e => e.ImageUrl)
                .IsUnicode(false);

            modelBuilder.Entity<Virus>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Virus>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Virus>()
                .Property(e => e.ImageUrl)
                .IsUnicode(false);
        }
    }
}
