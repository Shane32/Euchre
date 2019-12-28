using System;
using System.Collections.Generic;
using System.Text;

namespace Euchre
{
    abstract class Player
    {
        public readonly string Name;
        protected List<Card> Cards { get; private set; }
        protected Game Game { get; private set; }

        public Player(string name)
        {
            Name = name;
        }

        public void NewDeal(Game game)
        {
            Game = game;
            Cards = new List<Card>();
        }
        public void GiveCard(Card card)
        {
            Cards.Add(card);
        }

        public abstract Bid GetBid();

        public abstract void PickUpCard(Card card);

        public abstract Card GetCard();

        public virtual void StartRound() { }
    }

    class Bid
    {
        public readonly bool Alone;
        public readonly Suit Suit;

        public Bid(bool alone, Suit suit)
        {
            Alone = alone;
            Suit = suit;
        }
    }
}
