using System;
using System.Collections.Generic;
using System.Linq;

namespace Euchre
{
    public class TestGame : Game
    {
        public TestGame(Player team1Player1, Player team1Player2, Player team2Player1, Player team2Player2, int seed)
            : base(team1Player1, team1Player2, team2Player1, team2Player2, seed)
        {
        }

        public override Player PickRandomDealer()
        {
            // Always set player 1 (Players[0]) as dealer
            return Players[0];
        }

        public override void Deal()
        {
            // Initialize all AIs
            for (int i = 0; i < 4; i++)
            {
                Players[i].NewDeal(this);
            }

            // Create a list to track jacks
            Jacks = new List<Card>();

            // Define the specific cards for player 1 (dealer, Players[0]) and player 3 (Players[2])
            // Player 1 should have King of Diamonds flipped up (will be the revealed card)
            // Player 3 (team 1 player 2, Players[2]) should have: A♦, Q♦, A♠, K♠, 9♠
            
            var player3Cards = new List<Card>
            {
                new Card(14, Suit.Diamonds), // A♦
                new Card(12, Suit.Diamonds), // Q♦
                new Card(14, Suit.Spades),   // A♠
                new Card(13, Suit.Spades),   // K♠
                new Card(9, Suit.Spades)     // 9♠
            };

            // King of Diamonds will be the revealed card
            RevealedCard = new Card(13, Suit.Diamonds);

            // Create a full deck and remove the cards we've already assigned
            var allCards = new List<Card>();
            for (int suit = 0; suit < 4; suit++)
            {
                for (int number = 9; number <= 14; number++)
                {
                    allCards.Add(new Card(number, (Suit)suit));
                }
            }

            // Remove assigned cards from available pool
            var assignedCards = new HashSet<Card>(player3Cards) { RevealedCard };
            var availableCards = allCards.Where(c => !assignedCards.Any(ac => ac.Number == c.Number && ac.Suit == c.Suit)).ToList();

            // Shuffle available cards using the game's RNG
            for (int i = availableCards.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                var temp = availableCards[i];
                availableCards[i] = availableCards[j];
                availableCards[j] = temp;
            }

            // Deal cards to all players
            int availableIndex = 0;
            
            // Deal 5 cards to each player
            for (int cardNum = 0; cardNum < 5; cardNum++)
            {
                for (int playerNum = 0; playerNum < 4; playerNum++)
                {
                    Card card;
                    
                    if (playerNum == 2) // Player 3 (team 1 player 2)
                    {
                        card = player3Cards[cardNum];
                    }
                    else
                    {
                        card = availableCards[availableIndex++];
                    }

                    if (card.Number == 11) Jacks.Add(card);
                    Players[playerNum].GiveCard(card);
                }
            }

            // Track jacks in the revealed card
            if (RevealedCard.Number == 11) Jacks.Add(RevealedCard);
        }
    }
}