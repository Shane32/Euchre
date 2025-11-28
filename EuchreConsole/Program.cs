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
            // This method tests a specific Euchre scenario to determine the value of bidding diamonds
            // in a particular hand configuration. The scenario is as follows:
            //
            // FIXED HAND SETUP:
            // - Player 1 (dealer, team 1): Has King of Diamonds flipped up as the revealed card
            // - Player 2 (team 2): Random cards dealt according to seed
            // - Player 3 (team 1 partner): Has A♦, Q♦, A♠, K♠, 9♠ (strong hand with diamonds and spades)
            // - Player 4 (team 2): Random cards dealt according to seed
            //
            // TEST CONFIGURATIONS:
            // We run 100,000 hands with the same seeds in two different configurations:
            //
            // Configuration 1 (Always Bid): Player 3 uses AlwaysBidDiamonds strategy
            //   - Will always order up diamonds when possible (not alone)
            //   - Tests the scenario where player 3 aggressively bids with their strong diamond hand
            //
            // Configuration 2 (Never Bid): Player 3 uses NeverBid strategy
            //   - Will never bid, allowing other players to make the decision
            //   - Tests the scenario where player 3 passes despite having a strong hand
            //
            // FILTERING:
            // We exclude hands where player 2 (opponent) bids diamonds, as this would interfere
            // with our test of whether player 3 should bid. This ensures we're comparing scenarios
            // where the bidding decision is primarily in player 3's hands.
            //
            // METRICS:
            // For each configuration, we track:
            // - Number of hands recorded (excluding filtered hands)
            // - Average points scored by team 1 (player 1 & 3)
            // - Average points scored by team 2 (player 2 & 4)
            //
            // This allows us to determine if player 3's aggressive bidding strategy with this
            // particular hand is statistically beneficial compared to passing.
            
            Console.WriteLine("Euchre AI - Testing Bidding Strategies");

            try
            {
                var count = 100000;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine($"Playing {count} hands with different bidding strategies...");

                // Stats tracking
                int alwaysBidGamesRecorded = 0;
                int neverBidGamesRecorded = 0;
                double alwaysBidTeam1TotalPoints = 0;
                double alwaysBidTeam2TotalPoints = 0;
                double neverBidTeam1TotalPoints = 0;
                double neverBidTeam2TotalPoints = 0;

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
                    do
                    {
                        gameAlways.Next();
                        // If we had to re-deal, break and skip this game
                        if (gameAlways.Phase == GamePhase.Deal && gameAlways.Dealer != gameAlways.Players[0])
                        {
                            break;
                        }
                    } while (gameAlways.Phase != GamePhase.GamePlay && gameAlways.Phase != GamePhase.GameOver);

                    if (//gameAlways.Phase == GamePhase.GamePlay &&
                        !(gameAlways.Bidder == gameAlways.Players[1] && gameAlways.Bid.Suit == Suit.Diamonds))
                    {
                        // Play the hand to completion
                        while (gameAlways.Phase == GamePhase.GamePlay || gameAlways.Phase == GamePhase.TrickOver)
                        {
                            gameAlways.Next();
                        }
                        
                        // Record the points scored by both teams
                        alwaysBidTeam1TotalPoints += gameAlways.Teams[0].Score;
                        alwaysBidTeam2TotalPoints += gameAlways.Teams[1].Score;
                        alwaysBidGamesRecorded++;
                    }

                    // Configuration 2: NeverBid for player 3 (team 1 player 2)
                    var player1n = new Euchre.Shane.JumpingPC();
                    var player2n = new Euchre.Shane.NeverBid(); // Team 1 Player 2 (Players[2])
                    var player3n = new Euchre.Shane.JumpingPC();
                    var player4n = new Euchre.Shane.JumpingPC();
                    var gameNever = new Euchre.TestGame(player1n, player2n, player3n, player4n, seed);
                    
                    // Play until bidding is done
                    do
                    {
                        gameNever.Next();
                        // If we had to re-deal, break and skip this game
                        if (gameNever.Phase == GamePhase.Deal && gameNever.Dealer != gameNever.Players[0])
                            break;
                    } while (gameNever.Phase != GamePhase.GamePlay && gameNever.Phase != GamePhase.GameOver);

                    // Only record if player 2 (Players[1]) did NOT bid in diamonds
                    if (//gameNever.Phase == GamePhase.GamePlay &&
                        !(gameNever.Bidder == gameNever.Players[1] && gameNever.Bid.Suit == Suit.Diamonds))
                    {
                        // Play the hand to completion
                        while (gameNever.Phase == GamePhase.GamePlay || gameNever.Phase == GamePhase.TrickOver)
                        {
                            gameNever.Next();
                        }
                        
                        // Record the points scored by both teams
                        neverBidTeam1TotalPoints += gameNever.Teams[0].Score;
                        neverBidTeam2TotalPoints += gameNever.Teams[1].Score;
                        neverBidGamesRecorded++;
                    }
                }

                timer.Stop();

                Console.WriteLine($"\nResults after {count} hands:");
                Console.WriteLine($"Always Bid Configuration:");
                Console.WriteLine($"  Games recorded: {alwaysBidGamesRecorded}");
                Console.WriteLine($"  Team 1 average points: {(alwaysBidGamesRecorded > 0 ? alwaysBidTeam1TotalPoints / alwaysBidGamesRecorded : 0):F2}");
                Console.WriteLine($"  Team 2 average points: {(alwaysBidGamesRecorded > 0 ? alwaysBidTeam2TotalPoints / alwaysBidGamesRecorded : 0):F2}");
                Console.WriteLine($"\nNever Bid Configuration:");
                Console.WriteLine($"  Games recorded: {neverBidGamesRecorded}");
                Console.WriteLine($"  Team 1 average points: {(neverBidGamesRecorded > 0 ? neverBidTeam1TotalPoints / neverBidGamesRecorded : 0):F2}");
                Console.WriteLine($"  Team 2 average points: {(neverBidGamesRecorded > 0 ? neverBidTeam2TotalPoints / neverBidGamesRecorded : 0):F2}");
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
