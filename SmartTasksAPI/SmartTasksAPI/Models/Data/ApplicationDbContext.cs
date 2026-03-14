using Microsoft.EntityFrameworkCore;

namespace SmartTasksAPI.Models.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
        public DbSet<BoardList> Lists => Set<BoardList>();
        public DbSet<CardItem> Cards => Set<CardItem>();
        public DbSet<CardAssignment> CardAssignments => Set<CardAssignment>();
        public DbSet<CardComment> CardComments => Set<CardComment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<BoardMember>()
                .HasKey(x => new { x.BoardId, x.UserId });

            modelBuilder.Entity<CardAssignment>()
                .HasKey(x => new { x.CardId, x.UserId });

            modelBuilder.Entity<Board>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.OwnedBoards)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BoardMember>()
                .HasOne(x => x.Board)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.BoardId);

            modelBuilder.Entity<BoardMember>()
                .HasOne(x => x.User)
                .WithMany(x => x.BoardMemberships)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<BoardList>()
                .HasOne(x => x.Board)
                .WithMany(x => x.Lists)
                .HasForeignKey(x => x.BoardId);

            modelBuilder.Entity<CardItem>()
                .HasOne(x => x.List)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.ListId);

            modelBuilder.Entity<CardAssignment>()
                .HasOne(x => x.Card)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.CardId);

            modelBuilder.Entity<CardAssignment>()
                .HasOne(x => x.User)
                .WithMany(x => x.CardAssignments)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<CardComment>()
                .HasOne(x => x.Card)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.CardId);

            modelBuilder.Entity<CardComment>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
