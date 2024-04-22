using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using DataBaseQuiz.Scripts;
using System.Runtime.Remoting.Proxies;

namespace DataBaseQuiz
{
    internal class Program
    {
        private static IRepository postRep;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            postRep = new PostgresRep();
            postRep.Init();

            StartGame();

            Console.ReadKey();
        }

        private static List<string> usernames = new List<string>();
        private static int currentUserIndex = 0;

        private static void StartGame()
        {
            Console.WriteLine("Velkommen til vores QUIZZZZ");

            Console.WriteLine("Start med at skrive hver spillers brugernavne. Hvis I har nok spillere, skal i blot skrive intet for at gå videre.");

            int playerCount = 1;
            while (playerCount <= 4)
            {
                Console.WriteLine($"Skriv spiller {playerCount}'s brugernavn:");
                string username = Console.ReadLine();

                if (username == "" && playerCount > 1) break;
                
                playerCount++;

                usernames.Add(username);
                postRep.AddUser(username);
            }

            while (true) //Base loop
            {
                Console.Clear();

                postRep.ShowUsers();
                
                Console.WriteLine($"\nSpiller {usernames[currentUserIndex]}'s tur:");

                string selectedCategory = UserSelectCategory();
                
                int selectedQuestionId = UserSelectQuestion(selectedCategory);

                int selectedAnswerId = UserSelectAnswer(selectedQuestionId);

                postRep.CheckCorrectAnswer(selectedAnswerId, selectedQuestionId, usernames[currentUserIndex]);

                Console.WriteLine("Tryk en knap for at starte næste runde");
                Console.ReadKey(true);

                currentUserIndex = currentUserIndex < usernames.Count - 1 ? currentUserIndex + 1 : 0;
            }

        }

        private static string UserSelectCategory()
        {
            List<string> categoryNames = postRep.GetCategoryNames();

            string writeBeforeCheck = "Vælg en kategori ved at skrive dens tilsvarende nummer:";

            return postRep.SelectFromList(writeBeforeCheck, categoryNames);
        }


        private static int UserSelectQuestion(string selectedCategory)
        {
            List<int> questionIds = postRep.GetQuestions(selectedCategory);

            while(questionIds.Count == 0)
            {
                Console.Clear();

                Console.WriteLine("Du har valgt en kategori med ingen spørgsmål, prøv igen...\n");

                string newselectedCategory = UserSelectCategory();
                questionIds = postRep.GetQuestions(newselectedCategory);
            }


            string writeBeforeCheck = "Vælg et spørgsmål ved at skrive deres tilsvarende nummer til deres sværhedsgrad:";

            return postRep.SelectFromList(writeBeforeCheck, questionIds);
        }
        
        private static int UserSelectAnswer(int selectedQuestionId)
        {
            List<int> answerIds = postRep.GetAnswers(selectedQuestionId);

            string writeBeforeCheck = "Vælg et svar ved at skrive dets tilsvarende nummer:";

            return postRep.SelectFromList(writeBeforeCheck, answerIds);
        }

    }



   
    
}
