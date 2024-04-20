﻿using System;
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
            postRep = new PostgresRep();
            postRep.Init();

            StartGame();

            Console.ReadKey();
        }

        private static List<string> usernames = new List<string>();

        private static void StartGame()
        {
            Console.WriteLine("Welcome to the QUIZZZZ");

            Console.WriteLine("Start by writing usernames for the players, when you are done just press Enter with nothing written");

            int playerCount = 1;
            while (playerCount <= 4)
            {
                Console.WriteLine($"Write player {playerCount} username:");
                string username = Console.ReadLine();

                if (username == "") break;
                
                playerCount++;

                usernames.Add(username);
                postRep.AddUser(username);
            }
            
            postRep.ShowUsers();


            int currentUserIndex = 0;
            while (true) //Base loop
            {
                Console.WriteLine($"\nPlayer {usernames[currentUserIndex]}'s turn:");

                UserSelectCategory();
                
                UserSelectQuestion(usernames[currentUserIndex]);

                UserSelectAnswer(usernames[currentUserIndex]);

                Console.ReadKey(true);
                currentUserIndex = currentUserIndex < usernames.Count - 1 ? currentUserIndex + 1 : 0;
            }

        }

        private static string UserSelectCategory()
        {
            List<string> categories = postRep.GetCategories();

            return postRep.SelectFromCategories(categories);
        }


        private static int UserSelectQuestion(string currentUsername)
        {
            postRep.GetQuestions();

            return 0;
        }

        private static int UserSelectAnswer(string currentUsername)
        {
            postRep.GetAnswers();

            return 0;
        }

    }



   
    
}
