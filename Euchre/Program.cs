using System;

namespace Euchre
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 0; i < 7; i++)
                System.Threading.Tasks.Task.Run(() =>
                {
                    Main2(null);
                });
            Console.WriteLine("Press ANY KEY to exit");
            //Read key my worst enemy
            Console.ReadKey();
        }

        static void Main2(string[] args)
        {
            //hacker voice: i'm in
            Console.WriteLine("Euchre AI");

            try
            {
                var player1 = new StupidAI();
                var player2 = new StupidAI();
                var player3 = new StupidAI();
                var player4 = new StupidAI();
                //var game = new Game(player1, player2, player3, player4);
                //game.PlayGame();
                //Console.WriteLine($"winner: {((game.Teams[0].Score >= 10) ? "team 1" : "team 2")}");
                var count = 50000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine($"Playing {count} games of Euchre...");
                var team1Wins = PlayGames(player1, player2, player3, player4, count);
                Console.WriteLine($"After {count} games, team A won {team1Wins} times - approximately {(double)team1Wins / count:0.00%} - in {timer.ElapsedMilliseconds / 1000.0} seconds (about {timer.ElapsedMilliseconds / (double)count:0.00} ms per game)");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static int PlayGames(Player team1Player1, Player team1Player2, Player team2Player1, Player team2Player2, int count)
        {
            int team1Wins = 0;
            int team2Wins = 0;
            var rng = new Random(Game.GetRngSeed());
            for (int i = 0; i < count; i++)
            {
                var game = new Game(team1Player1, team1Player2, team2Player1, team2Player2, rng.Next());
                game.PlayGame();
                var team1Won = game.Teams[0].Score >= 10;
                if (team1Won) team1Wins++; else team2Wins++;
            }
            return team1Wins;
        }
    }
}
