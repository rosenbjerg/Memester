using System.Threading.Tasks;
using Memester.Core;
using Memester.Models;
using Microsoft.EntityFrameworkCore;

namespace Memester.Database
{
    public class DatabaseContext : DbContext, IAsyncInitialized
    {
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<LoginToken> LoginTokens { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Meme> Memes { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<FavoritedMeme> FavoritedMemes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Thread>().HasMany(t => t.Memes).WithOne(m => m.Thread).HasForeignKey(m => m.ThreadId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasMany(m => m.Favorited).WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(m => m.Votes).WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(m => m.Favorited).WithOne(f => f.User).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(m => m.Votes).WithOne(f => f.User).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Meme>().HasMany(m => m.Favorites).WithOne(f => f.Meme).HasForeignKey(f => f.MemeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Meme>().HasOne(m => m.Thread).WithMany(t => t.Memes).HasForeignKey(m => m.ThreadId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Meme>().HasMany(m => m.Votes).WithOne(f => f.Meme).HasForeignKey(f => f.MemeId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoginToken>().HasKey(fm => fm.Key);
            modelBuilder.Entity<FavoritedMeme>().HasKey(fm => new {fm.UserId, fm.MemeId});
            modelBuilder.Entity<Vote>().HasKey(fm => new {fm.UserId, fm.MemeId});
        }

        public async Task Initialize()
        {
            await Database.EnsureCreatedAsync();
        }
    }
}