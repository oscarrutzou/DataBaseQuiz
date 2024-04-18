using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DataBaseQuiz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PostgresRep postRep = new PostgresRep();
            postRep.Init();

            Console.ReadKey();
        }
    }

    public class User
    {
        public string username;
        public int total_score;
    }

    public interface IRepository
    {
        void Init();
        void AddUser(string username);
        User ReturnUser(string username);
    }

    public class PostgresRep : IRepository
    {

        string connection = "";

        public void Init()
        {


            GenTabel();
            CascadeDelTabel();
        }


        private void GenTabel()
        {
            //Generate if not exíst
        }

        private void CascadeDelTabel()
        {
            //Player tabel
        }

        private void GenerateCategories()
        {
            /*
            ----------------------------
            -                          -
            ----------------------------
            */
        }

        private void GenerateQuestions()
        {
            //
            CreateQuestion("A dragon ball q", 5, "Noge her", new string[] {"not this", "also not here"});
        }

        private void CreateQuestion(string description, int difficulty, string correct_answer_description, string[] wrong_answers_description)
        {

        }


        public User ReturnUser(string username)
        {
            //Går ind og finder den i databasen
            return null;
        }

        public void AddUser(string username)
        {
            //Add to database users
        }


   
    }

    
}
