using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using CsvHelper;


const string INPUT_FILE_NAME = "imdb.csv";
const string OUTPUT_TEMPLATE = "template.sql";
const string OUTPUT_SCRIPT = "MoviesDb.sql";

var genres = new[] { "Action", "Adult", "Adventure", "Animation", "Biography", "Comedy", "Crime", "Documentary", "Drama", "Family", "Fantasy", "FilmNoir", "GameShow", "History", "Horror", "Music", "Musical", "Mystery", "News", "RealityTV", "Romance", "SciFi", "Short", "Sport", "TalkShow", "Thriller", "War", "Western" }
                .Select((g, i) => new Genre(Id: i+1, Name: g)).ToList();

var movies = ReadMovies(genres);
WriteSqlScript(movies, genres);

/**************************************************************************/

void WriteSqlScript(List<Movie> movies, List<Genre> genres)
{
    var sfwGenres = genres.Where(g => g.Name != "Adult").ToList();

    var genreInsertText = string.Join(",\n", sfwGenres.Select(g => $"({g.Id}, '{g.Name}')"));
    var movieInsertText = string.Join(",\n", movies.Select(m =>
        $"({m.Id}, '{m.Title.Replace("'","''")}', {m.Year}, '{m.Url}', {m.Rating}, {m.GenreId})"));
    
    var template = File.ReadAllText(OUTPUT_TEMPLATE);
    var script = template
        .Replace("--GENRE_INSERTS", genreInsertText)
        .Replace("--MOVIE_INSERTS", movieInsertText);
    File.WriteAllText(OUTPUT_SCRIPT, script);
}

List<Movie> ReadMovies(List<Genre> genres)
{
    var adultGenre = genres.First(g => g.Name == "Adult");

    using var reader = new StreamReader(INPUT_FILE_NAME);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return csv.GetRecords<dynamic>()
            .Where(r => r.type == "video.movie" && IsValidTitle(r.title))
            .Select((r, i) => new Movie(
                Id: i + 1,
                Title: ExtractTitle(r.title),
                Year: ParseInt(r.year) ?? 0,
                Url: r.url,
                Rating: ParseDecimal(r.imdbRating) ?? 0,
                GenreId: GetGenreId(r, genres) ?? 0
            ))
            .Where(m => m.GenreId != 0 && m.GenreId != adultGenre.Id &&
                        m.Rating != 0 && m.Year != 0)
            .ToList();
}

bool IsValidTitle(string title)
{
    var lower = title.ToLower();
    return new[] { "video", "tv movie" }.All(w => !lower.Contains(w)) &&
           new[] { "the", "it's", " of ", "when" }.Any(w => lower.Contains(w));
}

string ExtractTitle(string value) => Regex.Replace(value, @" \(\d+\)", "");

int? ParseInt(string value) =>
    int.TryParse(value, out var result) ? (int?)result : null;

decimal? ParseDecimal(string value) =>
    decimal.TryParse(value, out var result) ? (decimal?)result : null;

int? GetGenreId(dynamic rec, List<Genre> genres) => 
    genres.FirstOrDefault(g => ((IDictionary<string, object>)rec)[g.Name].Equals("1"))?.Id;

/**************************************************************************/

record Movie(int Id, string Title, int Year, string Url, decimal Rating, int GenreId);
record Genre(int Id, string Name);

/**************************************************************************/

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}