using System;
using System.IO;
using System.Collections.Generic;

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

        // Step 4 — Fixed vs Alternating
        // Display series list
        Console.WriteLine("=== Your Series ===");
        int counter = 1;
        foreach (Show show in seriesShows)
        {
            Console.WriteLine($"{counter}. {show.Name} ({show.EpisodeLength} mins)");
            counter++;
        }

        // Tell user how many series they have
        Console.WriteLine($"\nYou have {seriesShows.Count} series.");
        Console.WriteLine("How many do you want as fixed? (1-2 recommended):");
        int fixedCount = int.Parse(Console.ReadLine());

        // Validate
        if (fixedCount > seriesShows.Count)
        {
            Console.WriteLine($"You only have {seriesShows.Count} series! Setting fixed to {seriesShows.Count}");
            fixedCount = seriesShows.Count;
        }

        // User picks
        Console.WriteLine($"Pick {fixedCount} fixed series (enter numbers separated by comma):");
        string input = Console.ReadLine();
        string[] picked = input.Split(',');

        List<Show> fixedSeries = new List<Show>();
        foreach (string part in picked)
        {
            int index = int.Parse(part.Trim()) - 1;
            fixedSeries.Add(seriesShows[index]);
        }

        // Build alternating list
        List<Show> alternatingSeries = new List<Show>();
        foreach (Show show in seriesShows)
        {
            if (!fixedSeries.Contains(show))
                alternatingSeries.Add(show);
        }

        // Assign to weekly schedule
        SortedDictionary<DayOfWeek, Show> weeklySchedule = new SortedDictionary<DayOfWeek, Show>();

        // Fixed series
        weeklySchedule[DayOfWeek.Monday] = fixedSeries[0];
        if (fixedSeries.Count > 1)
            weeklySchedule[DayOfWeek.Tuesday] = fixedSeries[1];

        // Alternating series
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

        // Assign movies
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
    }
}

public class Show
{
    public string? Name { get; set; }
    public int EpisodeLength { get; set; }
}