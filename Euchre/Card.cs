using System;
using System.Collections.Generic;
using System.Text;

namespace Euchre
{
    class Card
    {
        public int Number; //11 = J, 12 = Q, 13 = K, 14 = Ace (although 1-8 are not used), 15 = left, 16 = right
        public Suit Suit;

        public Card(int number, Suit suit)
        {
            Number = number;
            Suit = suit;
        }

    }
    enum Suit
    {
        Hearts = 0,
        Spades = 1,
        Clubs = 2,
        Diamonds = 3,
    }
}
