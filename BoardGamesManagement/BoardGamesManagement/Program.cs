using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.Sqlite;

class Program
{
    static IDbConnection GetConnection() =>
        new SqliteConnection("Data Source=boardgames.db");

    static void Main()
    {
        if (!File.Exists("boardgames.db"))
        {
            Console.WriteLine("Помилка: Базу даних не знайдено. Спочатку запусти EF Core застосунок.");
            return;
        }

        while (true)
        {
            Console.WriteLine("\n=== Board Games Analytics (Dapper) ===");
            Console.WriteLine("1. Всі сесії");
            Console.WriteLine("2. Топ-3 гри за годинами");
            Console.WriteLine("3. ТОП учасники за годинами");
            Console.WriteLine("4. Загальна статистика (можна вказати період)");
            Console.WriteLine("0. Вихід");
            Console.Write("Вибір: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ShowAllSessions(); break;
                case "2": Top3GamesByHours(); break;
                case "3": TopMembersByHours(); break;
                case "4": ShowStatistics(); break;
                case "0": return;
                default: Console.WriteLine("Невірний вибір."); break;
            }
        }
    }

    static void ShowAllSessions()
    {
        using var db = GetConnection();
        var sql = @"SELECT s.Date, s.DurationMinutes, g.Title AS Game, m.FullName AS Member
                    FROM Sessions s
                    JOIN Games g ON s.GameId = g.Id
                    JOIN Members m ON s.MemberId = m.Id
                    ORDER BY s.Date DESC";

        var sessions = db.Query(sql);

        Console.WriteLine("\n=== Усі сесії ===");
        foreach (var s in sessions)
        {
            Console.WriteLine($"{((DateTime)s.Date).ToShortDateString()} | {s.Game} | {s.Member} | {s.DurationMinutes} хв");
        }
    }

    static void Top3GamesByHours()
    {
        using var db = GetConnection();
        var sql = @"SELECT g.Title, SUM(s.DurationMinutes) / 60.0 AS Hours
                    FROM Sessions s
                    JOIN Games g ON s.GameId = g.Id
                    GROUP BY g.Title
                    ORDER BY Hours DESC
                    LIMIT 3";

        var games = db.Query(sql);

        Console.WriteLine("\n=== ТОП-3 гри за годинами ===");
        foreach (var g in games)
        {
            Console.WriteLine($"{g.Title} — {g.Hours:F1} год");
        }
    }

    static void TopMembersByHours()
    {
        using var db = GetConnection();
        var sql = @"SELECT m.FullName, SUM(s.DurationMinutes) / 60.0 AS Hours
                    FROM Sessions s
                    JOIN Members m ON s.MemberId = m.Id
                    GROUP BY m.FullName
                    ORDER BY Hours DESC";

        var members = db.Query(sql);

        Console.WriteLine("\n=== ТОП учасники за ігровими годинами ===");
        foreach (var m in members)
        {
            Console.WriteLine($"{m.FullName} — {m.Hours:F1} год");
        }
    }

    static void ShowStatistics()
    {
        Console.Write("З дати (yyyy-mm-dd) або Enter: ");
        string fromInput = Console.ReadLine();
        Console.Write("До дати (yyyy-mm-dd) або Enter: ");
        string toInput = Console.ReadLine();

        string sql = @"SELECT COUNT(*) AS Count, SUM(DurationMinutes) AS Total
                       FROM Sessions
                       WHERE 1=1";

        var parameters = new DynamicParameters();

        if (DateTime.TryParse(fromInput, out DateTime from))
        {
            sql += " AND Date >= @From";
            parameters.Add("From", from);
        }

        if (DateTime.TryParse(toInput, out DateTime to))
        {
            sql += " AND Date <= @To";
            parameters.Add("To", to);
        }

        using var db = GetConnection();
        var result = db.QueryFirst(sql, parameters);

        int count = result.Count ?? 0;
        int total = result.Total ?? 0;

        Console.WriteLine($"\nКількість сесій: {count}");
        Console.WriteLine($"Загальна тривалість: {total} хв ({(total / 60.0):F1} год)");
    }
}
