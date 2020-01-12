using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euchre
{
    public class DavidAI : Player
    {
        public int SkillLevel { get; set; }
        public int WinTotal { get; set; }

        public DavidAI() : base("DavidAI") { }

        public override Bid GetBid()
        {
            if (Game.Phase == GamePhase.BidRound1)
            {
                //DECISION MAKING
                //bid if it has at least 2 of that suit (not counting left bauer)

                //IDEAS
                //if left and right + ace of any suit = order
                //if right and ace + ace of any suit = order
                //if left and ace and king + any other card of suit = order

                List<Card> trumpCards = new List<Card>();
                List<Card> notTrumpCards = new List<Card>();

                foreach (Card card in Cards)
                {
                    if (card.Suit == Game.RevealedCard.Suit) trumpCards.Add(card);
                    else notTrumpCards.Add(card);
                }

                //if we have 5 of trump
                if (trumpCards.Count == 5)
                {
                    return new Bid(false, Game.RevealedCard.Suit);
                }

                //if we have 4 of trump
                if (trumpCards.Count == 4)
                {
                    //remaining card is a king or higher
                    if (notTrumpCards.Any(x=> x.Number >= 13)) return new Bid(false, Game.RevealedCard.Suit);
                }

                //if we have 3 of trump
                if (trumpCards.Count == 3)
                {
                    //if all cards are queen or higher
                    if (trumpCards.All(x => x.Number >= 12))
                    {
                        //remaining non trump cards are kings or higher
                        if (notTrumpCards.Any(x => x.Number >= 13)) return new Bid(false, Game.RevealedCard.Suit);
                    }
                }

                //if we have 2 of trump
                if (trumpCards.Count == 2)
                {
                    //
                }

                //if we have 1 of trump
                if (trumpCards.Count == 1)
                {
                    //
                }

                //if we have 0 of trump
                if (trumpCards.Count == 0)
                {
                    //
                }
            }
            else
            {
                //try all the other suits
                for (int suit = 0; suit < 4; suit++)
                {
                    if ((Suit)suit != Game.RevealedCard.Suit)
                    {
                        //bid if there are at least 2 of that suit (not counting left bauer)
                        if (Cards.Count(x => x.Suit == (Suit)suit) >= 2)
                        {
                            return new Bid(false, (Suit)suit);
                        }

                    }
                }
            }
            return null;
        }

        public override Card PickUpCard(Card card)
        {
            //who needs to pick up a card anyway?
            return card;
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

        public override void TrickFinished(Player takenBy, Team teamTakenBy)
        {
            throw new NotImplementedException();
        }

        public override void HandFinished()
        {
            throw new NotImplementedException();
        }

        public override void StartRound()
        {
            throw new NotImplementedException();
        }

        public override void BiddingFinished()
        {
            throw new NotImplementedException();
        }
    }
}
