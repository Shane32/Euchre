using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euchre
{
    class StupidAI : Player
    {
        public int SkillLevel { get; set; }
        public int WinTotal { get; set; }

        public StupidAI() : base("StupidAI") { }

        public override Bid GetBid()
        {
            //bid if it has at least 2 of that suit (not counting left bauer)
            if (Cards.Count(x => x.Suit == Game.RevealedCard.Suit) >= 2)
            {
                return new Bid(false, Game.RevealedCard.Suit);
            }
            return null;
        }

        public override Card GetCard()
        {
            //play the first card that's a legal play
            if (Game.CardsInPlay.Count == 0) //leading
            {
                //grab first card in hand
                var card = Cards[0];
                Cards.RemoveAt(0);
                return card;
            }
            else
            {
                //grab first hand that is in the led suit
                var suitLed = Game.CardsInPlay[0].Card.Suit;
                var card = Cards.FirstOrDefault(x => x.Suit == suitLed);
                //if no cards in the led suit, pick the first card in hand
                if (card == null) card = Cards[0];
                Cards.Remove(card);
                return card;
            }
        }
    }
}
