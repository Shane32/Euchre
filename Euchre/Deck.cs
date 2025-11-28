using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Euchre
{
    public class Deck
    {
        private Stack<Card> Pile;

        private Deck() { }
        public static Deck ShuffleNew()
        {
            return ShuffleNew(new Random(Game.GetRngSeed()));
        }
        public static Deck ShuffleNew(Random rng)
        {
            var newPile = new List<Card>();
            for (int suit = 0; suit < 4; suit++)
            {
                //9, 10, 11 (J), 12 (Q), 13 (K), 14 (A)
                for (int number = 9; number < 15; number++)
                {
                    newPile.Add(new Card(number, (Suit)suit));
                }
            }
            var shuffledPile = new List<Card>();
            while (newPile.Count > 0)
            {
                var randomIndex = rng.Next(0, newPile.Count);
                shuffledPile.Add(newPile[randomIndex]);
                newPile.RemoveAt(randomIndex);
            }
            return new Deck() { Pile = new Stack<Card>(shuffledPile) };
        }
        
        public Card PullCard()
        {
            return Pile.Pop();
        }
    }
}
