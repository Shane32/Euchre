using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Euchre
{
    public class Card : IEquatable<Card>
    {
        public int Number; //11 = J, 12 = Q, 13 = K, 14 = Ace (although 1-8 are not used), 15 = left, 16 = right
        public Suit Suit;

        public Card(int number, Suit suit)
        {
            Number = number;
            Suit = suit;
        }

        public static bool operator ==(Card card1, Card card2) {
            return (object.ReferenceEquals(card1, card2) || (!object.ReferenceEquals(card1, null) && !object.ReferenceEquals(card2, null) && card1.Number == card2.Number && card1.Suit == card2.Suit));
        }

        public static bool operator !=(Card card1, Card card2)
        {
            return !(card1 == card2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Card);
        }

        public bool Equals(Card other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)Suit * 20 + Number;
        }
    }
    public enum Suit
    {
        Hearts = 0,
        Spades = 1,
        Clubs = 2,
        Diamonds = 3,
    }
}
