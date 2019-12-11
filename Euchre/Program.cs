using System;

namespace Euchre
{
    class Program
    {
        static void Main(string[] args)
        {
            //hacker voice: i'm in
            Console.WriteLine("Hello World!");

            try
            {
                var player1 = new StupidAI();
                var player2 = new StupidAI();
                var player3 = new StupidAI();
                var player4 = new StupidAI();
                var game = new Game(player1, player2, player3, player4);
                game.PlayGame();
                Console.WriteLine($"winner: {((game.Teams[0].Score >= 10) ? "team 1" : "team 2")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Press ANY KEY to exit");
            //Read key my worst enemy
            Console.ReadKey();
        }
    }
}
