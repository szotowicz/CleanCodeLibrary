using Library.Activities;
using Library.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    class Program
    {
        private static IActivitiesManager _activitiesManager;
        private static List<string> _availableActivities;
        private static readonly IBookCatalogValidator _bookCatalogValidator = new BookCatalogValidator();

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (_bookCatalogValidator.IsValid(args[0]))
                {
                    _activitiesManager = new ActivitiesManager(args[0]);
                    _availableActivities = _activitiesManager.GetAvailableActivities().ToList();
                    Console.WriteLine(">>> Welcome to \"CleanCode\" Library! <<<");

                    string userAnswer = "";
                    do
                    {
                        Console.WriteLine("\nChoose what you want to do:");
                        for (int i = 0; i < _availableActivities.Count; i++)
                        {
                            Console.WriteLine($" {i + 1}: {_availableActivities[i]}");
                        }
                        Console.WriteLine(" Q: Exit");

                        Console.Write("\nCommand: ");
                        userAnswer = Console.ReadLine();

                    } while (HandleRequests(userAnswer));
                }
                else
                {
                    Console.WriteLine("[ERROR] The specified book catalog is invalid. Available formats: .xml or .json");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Book catalog has not been provided. Available formats: .xml or .json");
            }
        }

        private static bool HandleRequests(string userAnswer)
        {
            try
            {
                if (userAnswer.ToLower() != "q")
                {
                    int activityId = int.Parse(userAnswer);
                    if (activityId < 1 || activityId > _availableActivities.Count)
                    {
                        throw new Exception();
                    }

                    Console.WriteLine($"\n{_activitiesManager.PerformAction(activityId - 1)}");
                }
                else
                {
                    Console.WriteLine("\nThank you for using our services. See you later.");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n[Warning] The activity was not found. Try again.");
            }
            return true;
        }
    }
}