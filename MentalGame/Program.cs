using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.CancellationToken;

namespace MentalGame
{
    class Program
    {
        static void Main(string[] args)
        {

            Stopwatch stopwatch = new();

            TimeSpan timeLimit = TimeSpan.FromSeconds(5);

            var scoreManager = new ScoreManager();

            void Rounds()
            {
                int[] numberOfQuestions = new int[3];

                for (int i = 0; i < numberOfQuestions.Length; i++)
                { 
                    int actualAnswer = Questions();
                    //Reset and start the stopwatch for the current question 
                    stopwatch.Reset();
                    stopwatch.Start();
                    
                    string? userAnswer = ReadAnswer(stopwatch, timeLimit);
                    
                    ValidateAnswer(actualAnswer, userAnswer, scoreManager);

                    if (i < numberOfQuestions.Length - 1)
                    {
                        Console.WriteLine("\nNext Question\n");     
                    }
                
                }  
            }
            
            Rounds();
            

            while (true)
            {
                Console.Write("\nDo you want to proceed to the next round?[y/n]: ");
                string? proceed = Console.ReadLine()?.Trim().ToLower();
                
                switch (proceed)
                {
                    case "y":
                        Rounds();
                        break;
                    case "n":
                        Console.WriteLine("I hope I sharpened yor brain.");
                        Console.WriteLine($"\nTotal score: {scoreManager.GetScore()} pts.");
                        return;
                    default:
                        Console.WriteLine("Enter either y or n");
                        continue;
                }
            }
        }

        static int Questions()
        {
            Random rnd = new Random();
            int firstNumber = rnd.Next(1, 13);
            int secondNumber = rnd.Next(1, 13);

            Console.WriteLine($"What is {firstNumber} X {secondNumber}?");
            return firstNumber * secondNumber;
        }

        static string? ReadAnswer(Stopwatch stopwatch, TimeSpan timeLimit)
        {
            using var cts = new CancellationTokenSource(timeLimit);
            
            try
            {
                Console.Write("Enter your answer: ");
                
                // Create a task that reads input
                var inputTask = Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            return Console.ReadLine();
                        }
                        Thread.Sleep(50); // Small delay to prevent CPU spinning
                    }
                    return string.Empty;
                }, cts.Token);

                // Wait for the input or timeout
                if (inputTask.Wait(timeLimit))
                {
                    stopwatch.Stop();
                    string? input = inputTask.Result;
                    
                    if (stopwatch.Elapsed > timeLimit)
                    {
                        Console.WriteLine("\nGotta be fast, son.");
                        return string.Empty;
                    }
                    return input;
                }
                else
                {
                    Console.WriteLine("\nGotta be fast, son.");
                    return string.Empty;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nGotta be fast, son.");
                return string.Empty;
            }
            catch (FormatException)
            {
                Console.WriteLine("Please enter numbers only.");
                return string.Empty;
            }
        }

        static void ValidateAnswer(int actualAnswer, string userAnswer, ScoreManager scoreManager)
        {
            if (string.IsNullOrEmpty(userAnswer))
            {
                scoreManager.SubtractPoints(1);
                Console.WriteLine("On, no. Minus a point");
            }
            else if (int.TryParse(userAnswer, out int userAnswerInt) && userAnswerInt == actualAnswer)
            {
                scoreManager.AddPoints(3);
                Console.WriteLine("Plus 3 points");
            }
            else
            {
                scoreManager.SubtractPoints(1);
                Console.WriteLine("Wrong answer. Minus a point");
            }
        }

        class ScoreManager
        {
            private int _score;
            
            public static ScoreManager scoreManager { get; set; }

            public void AddPoints(int points)
            {
                _score += points;
            }

            public void SubtractPoints(int points)
            {
                _score -= points;
            }

            public int GetScore()
            {
                return _score;
            }
        }
    
        
    }
}
