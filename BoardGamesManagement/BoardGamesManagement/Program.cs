using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public List<Session> Sessions { get; set; }
}

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public DateTime JoinDate { get; set; }
    public List<Session> Sessions { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; }

    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<Game> Games { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=boardgames.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(e =>
        {
            e.Property(g => g.Title)
             .HasMaxLength(100)
             .IsRequired();

            e.Property(g => g.MinPlayers).IsRequired();
            e.Property(g => g.MaxPlayers).IsRequired();
            e.HasMany(g => g.Sessions).WithOne(s => s.Game).HasForeignKey(s => s.GameId);
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.Property(m => m.FullName).IsRequired();
            e.Property(m => m.JoinDate).IsRequired();
            e.HasMany(m => m.Sessions).WithOne(s => s.Member).HasForeignKey(s => s.MemberId);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.Property(s => s.Date).IsRequired();
            e.Property(s => s.DurationMinutes).IsRequired();
        });
    }
}

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();
        db.Database.EnsureDeleted();  
        db.Database.EnsureCreated();

        SeedData(db);

        Console.WriteLine("Базу даних створено та заповнено.");
        Console.WriteLine($"Ігор: {db.Games.Count()}");
        Console.WriteLine($"Учасників: {db.Members.Count()}");
        Console.WriteLine($"Сесій: {db.Sessions.Count()}");
    }

    static void SeedData(AppDbContext db)
    {
        if (db.Games.Any() || db.Members.Any()) return;

        var games = new List<Game>
        {
            new Game { Title = "Catan", Genre = "Strategy", MinPlayers = 3, MaxPlayers = 4 },
            new Game { Title = "Uno", Genre = "Card", MinPlayers = 2, MaxPlayers = 10 },
            new Game { Title = "Carcassonne", Genre = "Tile", MinPlayers = 2, MaxPlayers = 5 },
            new Game { Title = "Dixit", Genre = "Creative", MinPlayers = 3, MaxPlayers = 6 },
            new Game { Title = "Chess", Genre = "Classic", MinPlayers = 2, MaxPlayers = 2 }
        };

        var members = new List<Member>
        {
            new Member { FullName = "Alice Smith", JoinDate = DateTime.Today.AddDays(-200) },
            new Member { FullName = "Bob Johnson", JoinDate = DateTime.Today.AddDays(-150) },
            new Member { FullName = "Carol White", JoinDate = DateTime.Today.AddDays(-100) },
            new Member { FullName = "Dave Brown", JoinDate = DateTime.Today.AddDays(-50) },
            new Member { FullName = "Eve Davis", JoinDate = DateTime.Today.AddDays(-10) }
        };

        db.Games.AddRange(games);
        db.Members.AddRange(members);
        db.SaveChanges();

        var random = new Random();
        var sessions = new List<Session>();

        for (int i = 0; i < 20; i++)
        {
            sessions.Add(new Session
            {
                GameId = games[random.Next(games.Count)].Id,
                MemberId = members[random.Next(members.Count)].Id,
                Date = DateTime.Today.AddDays(-random.Next(1, 100)),
                DurationMinutes = random.Next(30, 180)
            });
        }

        db.Sessions.AddRange(sessions);
        db.SaveChanges();
    }
}
