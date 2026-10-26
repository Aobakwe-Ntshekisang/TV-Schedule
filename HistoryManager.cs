using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

public class HistoryManager
{
    // Create database and table
    public void CreateDatabase()
    {
        using var connection = new SqliteConnection("Data Source=history.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT,
                ShowName TEXT,
                Category TEXT,
                EpisodeLength INTEGER
            )";

        command.ExecuteNonQuery();
        Console.WriteLine("✅ Database ready!");
    }

    // Save weekly schedule to database
    public void SaveSchedule(SortedDictionary<DayOfWeek, Show> weeklySchedule, Dictionary<DayOfWeek, string> categories)
    {
        using var connection = new SqliteConnection("Data Source=history.db");
        connection.Open();

        foreach (var entry in weeklySchedule)
        {
            string category = categories.ContainsKey(entry.Key) ? categories[entry.Key] : "Unknown";

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO History (Date, ShowName, Category, EpisodeLength) 
                VALUES ($date, $showName, $category, $episodeLength)";

            command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$showName", entry.Value.Name);
            command.Parameters.AddWithValue("$category", category);
            command.Parameters.AddWithValue("$episodeLength", entry.Value.EpisodeLength);

            command.ExecuteNonQuery();
        }

        Console.WriteLine("✅ Schedule saved to history!");
    }

    // Check if a show aired recently
    public bool HasRecentlyAired(string showName, int weeks)
    {
        using var connection = new SqliteConnection("Data Source=history.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*) FROM History 
            WHERE ShowName = $showName 
            AND Date >= $cutoff";

        command.Parameters.AddWithValue("$showName", showName);
        command.Parameters.AddWithValue("$cutoff", DateTime.Now.AddDays(-weeks * 7).ToString("yyyy-MM-dd"));

        long count = (long)command.ExecuteScalar();
        return count > 0;
    }

    // Warn user about repeats
    public void CheckRepeats(SortedDictionary<DayOfWeek, Show> weeklySchedule, int weeks)
    {
        Console.WriteLine("\n=== Checking for Repeats ===");
        bool foundRepeat = false;

        foreach (var entry in weeklySchedule)
        {
            if (HasRecentlyAired(entry.Value.Name, weeks))
            {
                Console.WriteLine($"⚠️ Warning: {entry.Value.Name} aired within the last {weeks} weeks!");
                foundRepeat = true;
            }
        }

        if (!foundRepeat)
            Console.WriteLine("✅ No repeats found!");
    }

    // View history
    public void ViewHistory()
    {
        using var connection = new SqliteConnection("Data Source=history.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM History ORDER BY Date DESC";

        var reader = command.ExecuteReader();

        Console.WriteLine("\n=== Schedule History ===");
        while (reader.Read())
        {
            Console.WriteLine($"{reader["Date"]} → {reader["ShowName"]} ({reader["Category"]}, {reader["EpisodeLength"]} mins)");
        }
    }
}