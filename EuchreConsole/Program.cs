using System;
using Euchre;

namespace EuchreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var card1 = new Card(1, Suit.Clubs);
            //var card2 = new Card(1, Suit.Clubs);
            //Console.WriteLine(card1 == card2);
            //Console.WriteLine(card1.Equals(card2));
            //Console.WriteLine(card1.Equals((object)card2));
            //Console.WriteLine(object.Equals(card1, card2));
            
            //for (var i = 0; i < 7; i++)
            //    System.Threading.Tasks.Task.Run(() =>
            //    {
            //        Main2(null);
            //    });
            Main3(args);
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
                var player1 = new Euchre.Shane.JumpingPC();
                var player2 = new Euchre.Shane.JumpingPC();
                var player3 = new StupidAI();
                var player4 = new StupidAI();
                //var game = new Game(player1, player2, player3, player4);
                //game.PlayGame();
                //Console.WriteLine($"winner: {((game.Teams[0].Score >= 10) ? "team 1" : "team 2")}");
                var count = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine($"Playing {count} games of Euchre...");
                var team1Wins = PlayGames(player1, player2, player3, player4, count, out var lostGameSeed);
                Console.WriteLine($"After {count} games, team A won {team1Wins} times - approximately {(double)team1Wins / count:0.00%} - in {timer.ElapsedMilliseconds / 1000.0} seconds (about {timer.ElapsedMilliseconds / (double)count:0.00} ms per game)");
                Console.WriteLine($"The first losing game was number {lostGameSeed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Main3(string[] args)
        {
            Console.WriteLine("Euchre AI - Testing Bidding Strategies");

            try
            {
                var count = 10000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine($"Playing {count} hands with different bidding strategies...");

                // Stats tracking
                int alwaysBidGamesRecorded = 0;
                int neverBidGamesRecorded = 0;
                double alwaysBidTotalPoints = 0;
                double neverBidTotalPoints = 0;

                var rng = new Random(Game.GetRngSeed());

                for (int i = 0; i < count; i++)
                {
                    var seed = rng.Next();

                    // Configuration 1: AlwaysBidDiamonds for player 3 (team 1 player 2)
                    var player1a = new Euchre.Shane.JumpingPC();
                    var player2a = new Euchre.Shane.AlwaysBidDiamonds(); // Team 1 Player 2 (Players[2])
                    var player3a = new Euchre.Shane.JumpingPC();
                    var player4a = new Euchre.Shane.JumpingPC();
                    var gameAlways = new Euchre.TestGame(player1a, player2a, player3a, player4a, seed);
                    
                    // Play until bidding is done
                    while (gameAlways.Phase != GamePhase.GamePlay && gameAlways.Phase != GamePhase.Deal)
                    {
                        gameAlways.Next();
                        // If we had to re-deal, break and skip this game
                        if (gameAlways.Phase == GamePhase.Deal && gameAlways.Dealer != gameAlways.Players[0])
                            break;
                    }

                    // Only record if player 2 (Players[1]) did NOT bid in diamonds
                    bool player2BidDiamonds = gameAlways.Phase == GamePhase.GamePlay &&
                                              gameAlways.Bidder == gameAlways.Players[1] &&
                                              gameAlways.Bid.Suit == Suit.Diamonds;
                    
                    if (gameAlways.Phase == GamePhase.GamePlay && !player2BidDiamonds)
                    {
                        // Play the hand to completion
                        while (gameAlways.Phase == GamePhase.GamePlay || gameAlways.Phase == GamePhase.TrickOver)
                        {
                            gameAlways.Next();
                        }
                        
                        // Record the points scored by team 1
                        alwaysBidTotalPoints += gameAlways.Teams[0].Score;
                        alwaysBidGamesRecorded++;
                    }

                    // Configuration 2: NeverBid for player 3 (team 1 player 2)
                    var player1n = new Euchre.Shane.JumpingPC();
                    var player2n = new Euchre.Shane.NeverBid(); // Team 1 Player 2 (Players[2])
                    var player3n = new Euchre.Shane.JumpingPC();
                    var player4n = new Euchre.Shane.JumpingPC();
                    var gameNever = new Euchre.TestGame(player1n, player2n, player3n, player4n, seed);
                    
                    // Play until bidding is done
                    while (gameNever.Phase != GamePhase.GamePlay && gameNever.Phase != GamePhase.Deal)
                    {
                        gameNever.Next();
                        // If we had to re-deal, break and skip this game
                        if (gameNever.Phase == GamePhase.Deal && gameNever.Dealer != gameNever.Players[0])
                            break;
                    }

                    // Only record if player 2 (Players[1]) did NOT bid in diamonds
                    player2BidDiamonds = gameNever.Phase == GamePhase.GamePlay &&
                                         gameNever.Bidder == gameNever.Players[1] &&
                                         gameNever.Bid.Suit == Suit.Diamonds;
                    
                    if (gameNever.Phase == GamePhase.GamePlay && !player2BidDiamonds)
                    {
                        // Play the hand to completion
                        while (gameNever.Phase == GamePhase.GamePlay || gameNever.Phase == GamePhase.TrickOver)
                        {
                            gameNever.Next();
                        }
                        
                        // Record the points scored by team 1
                        neverBidTotalPoints += gameNever.Teams[0].Score;
                        neverBidGamesRecorded++;
                    }
                }

                timer.Stop();

                Console.WriteLine($"\nResults after {count} hands:");
                Console.WriteLine($"Always Bid Configuration:");
                Console.WriteLine($"  Games recorded: {alwaysBidGamesRecorded}");
                Console.WriteLine($"  Average points: {(alwaysBidGamesRecorded > 0 ? alwaysBidTotalPoints / alwaysBidGamesRecorded : 0):F2}");
                Console.WriteLine($"\nNever Bid Configuration:");
                Console.WriteLine($"  Games recorded: {neverBidGamesRecorded}");
                Console.WriteLine($"  Average points: {(neverBidGamesRecorded > 0 ? neverBidTotalPoints / neverBidGamesRecorded : 0):F2}");
                Console.WriteLine($"\nTotal time: {timer.ElapsedMilliseconds / 1000.0:F2} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static int PlayGames(Player team1Player1, Player team1Player2, Player team2Player1, Player team2Player2, int count, out int lostGameSeed)
        {
            int team1Wins = 0;
            int team2Wins = 0;
            var rng = new Random(Game.GetRngSeed());
            lostGameSeed = 0;
            for (int i = 0; i < count; i++)
            {
                var seed = rng.Next();
                var game = new Game(team1Player1, team1Player2, team2Player1, team2Player2, seed);
                game.PlayGame();
                var team1Won = game.Teams[0].Score >= 10;
                if (team1Won) team1Wins++; else { team2Wins++; lostGameSeed = seed; }
            }
            return team1Wins;
        }
    }
}
