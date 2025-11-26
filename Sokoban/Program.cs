namespace Sokoban;

/// <summary>
/// SOKOBAN - Kooperatív AI Puzzle Játék
/// BSc Szakdolgozat - Mesterséges Intelligencia és Ember Együttműködése
/// 
/// C# Console Application
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "SOKOBAN - AI Hint Rendszerrel";

        try
        {
            var ui = new ConsoleUI();
            ui.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
            Console.WriteLine("Nyomj egy billentyűt a kilépéshez...");
            Console.ReadKey();
        }
    }
}
