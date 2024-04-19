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

        private string connectionString = "Host=localhost;Username=postgres;Password=;DataBase=quizGame";
        private NpgsqlDataSource dateSource;

        private enum Categories
        {
            LoveCraft,
            DataBaser,
            Henrettelsesmetoder,
            Koreansk,
            Superhelte
        }

        public void Init()
        {
            dateSource = NpgsqlDataSource.Create(connectionString);


            GenTabels();
            GenerateCategories();
        }


        private void GenTabels()
        {
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS users (" +
                "username VARCHAR(30) PRIMARY KEY," +
                "total_score INT DEFAULT 0" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS categories (" +
                "name VARCHAR(30) PRIMARY KEY," +
                "description VARCHAR(30) NOT NULL" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS answers (" +
                "answer_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "description VARCHAR(30) NOT NULL " +
                ");"
            ).ExecuteNonQuery();


            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS questions (" +
                "question_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "correct_answer_id INT, " +
                "difficulty INT DEFAULT 1, " +
                "description VARCHAR(30) NOT NULL, " +
                "FOREIGN KEY (correct_answer_id) REFERENCES answers(answer_id)" +
                ");"
            ).ExecuteNonQuery();
            

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS cat_has_questions (" +
                "category_name VARCHAR(30)," +
                "question_id INT," +
                "FOREIGN KEY (category_name) REFERENCES categories(name)," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS question_has_answers (" +
                "question_id INT," +
                "answer_id INT," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)," +
                "FOREIGN KEY (answer_id) REFERENCES answers(answer_id)" +
                ");"
            ).ExecuteNonQuery();

            TruncateUsers();
        }

        private void AddToCategory(Categories categoryName, string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO categories (name, description) VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(categoryName.ToString());
            cmd.Parameters.AddWithValue(description);
            cmd.ExecuteNonQuery();
        }

        private void TruncateUsers()
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"TRUNCATE TABLE users;");
            cmd.ExecuteNonQuery();
        }

        private void GenerateCategories()
        {
            AddToCategory(Categories.LoveCraft, "Noget");
            AddToCategory(Categories.DataBaser, "Noget");
            AddToCategory(Categories.Henrettelsesmetoder, "Noget");
            AddToCategory(Categories.Koreansk, "Noget");
            AddToCategory(Categories.Superhelte, "Noget");
        }

        private void GenerateQuestions()
        {
            //
            CreateQuestion(Categories.LoveCraft, "A dragon ball q", 5, "Noget her", new string[] {"not this", "also not here"});
        }

        private void CreateQuestion(Categories category, string description, int difficulty, string correct_answer_description, string[] wrong_answers_description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO questions (description, difficulty) VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(description);
            cmd.Parameters.AddWithValue(difficulty);
            cmd.ExecuteNonQuery();
        }

        private void GenerateAnswer(string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO answers (description) VALUES ($1);");
            cmd.Parameters.AddWithValue(description);
            cmd.ExecuteNonQuery();
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
