using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MroczwareFramework.Extensions
{
    public class SqlFile
    {
        public string Path { get; set; } = null!;
        public Match Match { get; set; } = null!;
    }
    internal static class AddMroczwareMigrationsExitension
    {
        public static void AddMroczwareMigrations(this IServiceCollection services, Assembly assembly, string connectionString)
        {
            string baseDirectory = Path.GetDirectoryName(assembly.Location)
                ?? throw new Exception("Nie znaleziono scieżki projektu");

            string migrationsFolder = Path.Combine(baseDirectory, "Migrations");

            if (!Directory.Exists(migrationsFolder))
                throw new Exception($"Folder '{migrationsFolder}' nie istnieje.");

            // Pobierz wszystkie pliki .sql z folderu
            IEnumerable<SqlFile> sqlFiles = Directory.GetFiles(migrationsFolder, "*.sql")
                .Select(filePath => new SqlFile()
                {
                    Path = filePath,
                    Match = Regex.Match(Path.GetFileName(filePath), @"^(?<date>\d{8})_(?<number>\d{3})_.*")
                });

            List<SqlFile> invalidFiles = sqlFiles.Where(x => !x.Match.Success).ToList();

            if (invalidFiles.Count != 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (SqlFile invalidFile in invalidFiles)
                    Console.WriteLine($"Transakcja wycofana z powodu nazyw nie zgodnej z konwencja: {invalidFile.Path}");

                Console.ResetColor();
                throw new Exception("Znaleziono pliki z niezgodna nazwa");
            }
                
            string[] validSqlFiles = sqlFiles
                  .Where(x => x.Match.Success)
                  .OrderByDescending(x => DateTime.ParseExact(x.Match.Groups["date"].Value, "yyyyMMdd", null))
                  .ThenByDescending(x => int.Parse(x.Match.Groups["number"].Value))
                  .Select(x => x.Path)
                  .ToArray();

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Połączono z bazą danych.");

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (SqlFile filePath in sqlFiles)
                        {
                            try
                            {
                                Console.WriteLine($"Wykonywanie pliku: {Path.GetFileName(filePath.Path)}");
                                using SqlCommand command = new(File.ReadAllText(filePath.Path), connection, transaction);
                                command.ExecuteNonQuery();
                                Console.WriteLine($"Pomyślnie wykonano: {Path.GetFileName(filePath.Path)}");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Błąd podczas wykonywania pliku {Path.GetFileName(filePath.Path)}: {ex.Message}");
                                Console.ResetColor();
                                throw;
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine("Wszystkie operacje zakończone sukcesem. Transakcja zatwierdzona.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Transakcja wycofana z powodu błędu: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                Console.ResetColor();
                connection.Close();
            }
            Console.WriteLine("Proces zakończony.");
        }
    }
}
