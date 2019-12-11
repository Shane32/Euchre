using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Euchre
{
    class Deck
    {
        private Stack<Card> Pile;

        private Deck() { }
        public static Deck ShuffleNew()
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
            byte[] bytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            var rnd = new Random(BitConverter.ToInt32(bytes, 0));
            while (newPile.Count > 0)
            {
                var randomIndex = rnd.Next(0, newPile.Count);
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
