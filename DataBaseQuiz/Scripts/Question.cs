namespace DataBaseQuiz.Scripts
{
    public class Question
    {
        public int question_id;
        public int difficulty;
        public string description;

        public Question(int question_id, int difficulty, string description)
        {
            this.question_id = question_id;
            this.difficulty = difficulty;
            this.description = description;
        }
    }
}