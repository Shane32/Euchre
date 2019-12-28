using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euchre
{
    class ShaneAI : Player
    {
        public ShaneAI() : base("StupidAI") { }

        private Player Partner;

        private static readonly Suit[] AllSuits = new[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Hearts };

        public override void StartRound()
        {
            if (Game.Teams[0].Players[0] == this)
                Partner = Game.Teams[0].Players[1];
            else if (Game.Teams[0].Players[1] == this)
                Partner = Game.Teams[0].Players[0];
            else if (Game.Teams[1].Players[0] == this)
                Partner = Game.Teams[1].Players[1];
            else
                Partner = Game.Teams[1].Players[0];
        }

        public override Bid GetBid()
        {
            if (Game.Phase == GamePhase.BidRound1)
            {
                var trump = Game.RevealedCard.Suit;
                int score;
                if (Game.Dealer == this)
                {
                    score = CalculatePickupHandScore(trump);
                } 
                else if (Game.Dealer == Partner)
                {
                    score = CalculateHandScore(trump, Cards);
                    score += Game.RevealedCard.Number == 11 ? 7 : 3;
                }
                else
                {
                    score = CalculateHandScore(trump, Cards);
                    score -= 5;
                }
                if (score >= 40)
                {
                    return new Bid(true, trump);
                }
                else if (score >= 20)
                {
                    return new Bid(false, trump);
                }
            }
            else
            {
                var otherSuits = AllSuits.Where(x => x != Game.RevealedCard.Suit);
                var suitScores = otherSuits.Select(x => new { Suit = x, Score = CalculateHandScore(x, Cards) + (x == OffSuit(x) ? 3 : 0) }).OrderByDescending(x => x.Score);
                foreach(var suit in suitScores)
                {
                    if (suit.Score >= 40)
                    {
                        return new Bid(true, suit.Suit);
                    }
                    else if (suit.Score >= 20)
                    {
                        return new Bid(false, suit.Suit);
                    }
                }
            }
            return null;
        }

        private int CalculatePickupHandScore(Suit trump)
        {
            var discard = PickCardToDiscard(trump);
            var cards = SimulatePickup(Game.RevealedCard, discard);
            return CalculateHandScore(trump, cards);
        }

        private IEnumerable<Card> SimulatePickup(Card card, Card discard)
        {
            return Cards.Where(x => x != discard).Append(card);
        }

        private Card PickCardToDiscard(Suit trump)
        {
            var transformedCards = Cards.Select(x => TransformCard(x, trump));
            var trumpNum = transformedCards.Count(x => x.Suit == trump);
            int shortSuitIfNum;
            if (trumpNum <= 2)
            {
                //if you will have 3 trump or less, discard a K to short-suit over not
                shortSuitIfNum = 13;
            }
            else
            {
                //if you will have 4/5 trump, keep the K and hope the A is buried
                shortSuitIfNum = 12;
            }
            var discard = Cards.Where(x => x.Suit != trump).GroupBy(x => x.Suit).Where(x => x.Count() == 1).Select(x => x.First()).Where(x => x.Number <= shortSuitIfNum).OrderBy(x => x.Number).FirstOrDefault();
            if (discard == null) discard = Cards.Where(x => x.Suit != trump).OrderBy(x => x.Number).FirstOrDefault();
            if (discard == null) discard = Cards.OrderBy(x => x.Number).First();
            return discard;
        }

        private int CalculateHandScore(Suit trump, IEnumerable<Card> cards)
        {
            int value = 0;
            var sortedCards = cards.Select(x => TransformCard(x, trump)).OrderByDescending(x => x.Number).GroupBy(x => x.Suit).ToList();
            foreach (var suitGroup in sortedCards) {
                var suit = suitGroup.Key;
                var highestCard = suit == trump ? 16 : 14;
                foreach (var card in suitGroup)
                {
                    var number = card.Number;
                    if (number == highestCard)
                    {
                        value += suit == trump ? 10 : 5;
                        if (suit == trump) highestCard -= 1;
                    }
                    else if (suit == trump)
                    {
                        value += 5;
                    }
                }
            }
            return value;
        }

        private static Card TransformCard(Card card, Suit trump)
        {
            var offSuit = OffSuit(trump);
            if (card.Number == 11)
            {
                var suit = card.Suit;
                if (suit == trump) return new Card(16, trump);
                if (suit == offSuit) return new Card(15, trump);
            }
            return card;
        }

        private static Suit OffSuit(Suit trump)
        {
            return trump switch
            {
                Suit.Clubs => Suit.Spades,
                Suit.Spades => Suit.Clubs,
                Suit.Hearts => Suit.Diamonds,
                Suit.Diamonds => Suit.Hearts,
                _ => throw new ArgumentOutOfRangeException(nameof(trump))
            };
        }

        public override void PickUpCard(Card card)
        {
            var discard = PickCardToDiscard(Game.Bid.Suit);
            Cards.Remove(discard);
            Cards.Add(card);
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

        private Card Play(Card card)
        {
            Cards.Remove(card);
            return card;
        }
    }
}
