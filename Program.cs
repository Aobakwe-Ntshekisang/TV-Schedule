using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class TvSchedule
{
    static void Main(string[] args)
    {
        string file = @"C:\Users\Aobakwe Ntshekisang\Documents\TvSchedule\shows.txt";

        if (!File.Exists(file))
        {
            Console.WriteLine("File not found!");
            return;
        }

        // Initialize history manager
        HistoryManager historyManager = new HistoryManager();
        historyManager.CreateDatabase();

        // Step 1 & 2 — Parse and categorize
        Dictionary<string, List<Show>> schedule = new Dictionary<string, List<Show>>();
        List<string> knownCategories = new List<string> { "Movie", "Series", "Anime", "Cartoon" };
        string currentCategory = "";
        string[] lines = File.ReadAllLines(file);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("#"))
            {
                currentCategory = trimmed.Replace("#", "").Trim();
                schedule[currentCategory] = new List<Show>();
            }
            else if (knownCategories.Contains(trimmed))
            {
                currentCategory = trimmed;
                schedule[currentCategory] = new List<Show>();
            }
            else if (trimmed != "")
            {
                if (currentCategory != "")
                {
                    string[] parts = trimmed.Split(',');
                    if (parts.Length >= 2)
                    {
                        Show myShow = new Show()
                        {
                            Name = parts[0].Trim(),
                            EpisodeLength = int.Parse(parts[1].Trim())
                        };
                        schedule[currentCategory].Add(myShow);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: '{trimmed}' has no episode length, skipping!");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: '{trimmed}' has no category, skipping!");
                }
            }
        }

        // Step 3 — Separate series and movies
        List<Show> seriesShows = new List<Show>();
        List<Show> movieShows = new List<Show>();

        foreach (var category in schedule)
        {
            if (category.Key != "Movie")
                seriesShows.AddRange(category.Value);
            else if (category.Key == "Movie")
                movieShows.AddRange(category.Value);
        }

        // Main menu loop
        while (true)
        {
            Console.WriteLine("\n=== TV Schedule Manager ===");
            Console.WriteLine("1. Generate weekly schedule");
            Console.WriteLine("2. Add a show");
            Console.WriteLine("3. Update a show");
            Console.WriteLine("4. Delete a show");
            Console.WriteLine("5. View history");
            Console.WriteLine("6. Exit");
            Console.Write("Enter your selection: ");
            string selection = Console.ReadLine();

            switch (selection)
            {
                case "1":
                    GenerateSchedule(seriesShows, movieShows, schedule, historyManager);
                    break;
                case "2":
                    // Add a show (coming soon)
                    Console.WriteLine("Coming soon!");
                    break;
                case "3":
                    // Update a show (coming soon)
                    Console.WriteLine("Coming soon!");
                    break;
                case "4":
                    // Delete a show (coming soon)
                    Console.WriteLine("Coming soon!");
                    break;
                case "5":
                    historyManager.ViewHistory();
                    break;
                case "6":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid selection, try again!");
                    break;
            }
        }
    }

    static void GenerateSchedule(List<Show> seriesShows, List<Show> movieShows, Dictionary<string, List<Show>> schedule, HistoryManager historyManager)
    {
        // Step 4 — Fixed vs Alternating
        Console.WriteLine("=== Your Series ===");
        int counter = 1;
        foreach (Show show in seriesShows)
        {
            Console.WriteLine($"{counter}. {show.Name} ({show.EpisodeLength} mins)");
            counter++;
        }

        Console.WriteLine($"\nYou have {seriesShows.Count} series.");
        Console.WriteLine("How many do you want as fixed? (1-2 recommended):");
        int fixedCount = int.Parse(Console.ReadLine());

        if (fixedCount > seriesShows.Count)
        {
            Console.WriteLine($"You only have {seriesShows.Count} series! Setting fixed to {seriesShows.Count}");
            fixedCount = seriesShows.Count;
        }

        Console.WriteLine($"Pick {fixedCount} fixed series (enter numbers separated by comma):");
        string input = Console.ReadLine();
        string[] picked = input.Split(',');

        List<Show> fixedSeries = new List<Show>();
        foreach (string part in picked)
        {
            int index = int.Parse(part.Trim()) - 1;
            fixedSeries.Add(seriesShows[index]);
        }

        List<Show> alternatingSeries = new List<Show>();
        foreach (Show show in seriesShows)
        {
            if (!fixedSeries.Contains(show))
                alternatingSeries.Add(show);
        }

        SortedDictionary<DayOfWeek, Show> weeklySchedule = new SortedDictionary<DayOfWeek, Show>();

        weeklySchedule[DayOfWeek.Monday] = fixedSeries[0];
        if (fixedSeries.Count > 1)
            weeklySchedule[DayOfWeek.Tuesday] = fixedSeries[1];

        Random random = new Random();

        if (alternatingSeries.Count > 0)
        {
            int randomIndex = random.Next(0, alternatingSeries.Count);
            weeklySchedule[DayOfWeek.Wednesday] = alternatingSeries[randomIndex];
            alternatingSeries.RemoveAt(randomIndex);
        }

        if (alternatingSeries.Count > 0)
        {
            int randomIndex = random.Next(0, alternatingSeries.Count);
            weeklySchedule[DayOfWeek.Thursday] = alternatingSeries[randomIndex];
            alternatingSeries.RemoveAt(randomIndex);
        }

        int movieIndex = 0;
        List<DayOfWeek> movieDays = new List<DayOfWeek>
        {
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        foreach (DayOfWeek day in movieDays)
        {
            if (movieIndex < movieShows.Count)
            {
                weeklySchedule[day] = movieShows[movieIndex];
                movieIndex++;
            }
        }

        // Print weekly schedule
        Console.WriteLine("\n=== This Week's TV Schedule ===");
        foreach (var entry in weeklySchedule)
        {
            Console.WriteLine($"{entry.Key} → {entry.Value.Name} ({entry.Value.EpisodeLength} mins)");
        }

        // Step 5 — History
        Console.WriteLine("\nHow many weeks before a show can repeat? (default: 4):");
        string weeksInput = Console.ReadLine();
        int weeks = string.IsNullOrEmpty(weeksInput) ? 4 : int.Parse(weeksInput);

        historyManager.CheckRepeats(weeklySchedule, weeks);

        // Build category map
        Dictionary<DayOfWeek, string> categoryMap = new Dictionary<DayOfWeek, string>();
        foreach (var entry in weeklySchedule)
        {
            foreach (var cat in schedule)
            {
                if (cat.Value.Contains(entry.Value))
                    categoryMap[entry.Key] = cat.Key;
            }
        }

        historyManager.SaveSchedule(weeklySchedule, categoryMap);
    }
}