using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euchre.Shane
{
    public class JumpingPC : Player
    {
        public JumpingPC() : base("JumpingPC") { }

        private static readonly Suit[] AllSuits = new[] { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Hearts };
        private static readonly Card[] AllCardsExceptJacks;
        private static readonly Card[] AllJacks = new[] { new Card(11, Suit.Spades), new Card(11, Suit.Clubs), new Card(11, Suit.Diamonds), new Card(11, Suit.Hearts) };

        private Team Team;
        private Player Partner;
        private Suit Trump;
        private Card RevealedCard;
        private bool RevealedCardBuried;
        private bool IsBiddingTeam;
        //private int[] HighestCard = new int[4];
        private List<Card> CardsLeftToPlay = new List<Card>(24);
        //private List<Card> CardsOutThere = new List<Card>(24);
        private bool TrumpHasBeenLead = false;

        static JumpingPC()
        {
            var cards = new List<Card>(20);
            for (int suit = 0; suit < 4; suit++)
            {
                //9, 10, 11 (J), 12 (Q), 13 (K), 14 (A)
                for (int number = 9; number < 15; number++)
                {
                    if (number != 11)
                        cards.Add(new Card(number, (Suit)suit));
                }
            }
            AllCardsExceptJacks = cards.ToArray();
        }

        public override void BiddingFinished()
        {
            Trump = Game.Bid.Suit;

            IsBiddingTeam = Game.BiddingTeam == Team;
            //for (int i = 0; i < 4; i++)
            //    HighestCard[i] = i == (int)Game.Bid.Suit ? 16 : 14;
            RevealedCardBuried = Trump != RevealedCard.Suit;
            TrumpHasBeenLead = false;

            var bidOffSuit = OffSuit(Trump);
            for (int suit = 0; suit < 4; suit++)
            {
                if ((Suit)suit == Trump)
                {
                    AllJacks[suit].Suit = Trump;
                    AllJacks[suit].Number = 16;
                }
                else if ((Suit)suit == bidOffSuit)
                {
                    AllJacks[suit].Suit = Trump;
                    AllJacks[suit].Number = 15;
                }
                else
                {
                    AllJacks[suit].Suit = (Suit)suit;
                    AllJacks[suit].Number = 11;
                }
            }
            CardsLeftToPlay.Clear();
            if (RevealedCardBuried)
            {
                CardsLeftToPlay.AddRange(AllCardsExceptJacks.Concat(AllJacks).Where(x => x != RevealedCard));
            }
            else
            {
                CardsLeftToPlay.AddRange(AllCardsExceptJacks.Concat(AllJacks));
            }
            //CardsOutThere.Clear();
            //CardsOutThere.AddRange(CardsLeftToPlay.Where(card => !Cards.Any(myCard => myCard == card)));
        }

        private void CardWasPlayed(Card card)
        {
            CardsLeftToPlay.RemoveAll(x => x == card);
            //CardsOutThere.RemoveAll(x => x == card);
        }

        public override void TrickFinished(Player takenBy, Team teamTakenBy) 
        {
            bool foundMe = false;
            foreach (var play in Game.CardsInPlay)
            {
                if (play.PlayedBy == this) foundMe = true;
                if (foundMe) CardWasPlayed(play.Card);
            }
        }

        public override void HandFinished() { }

        public override void StartRound()
        {
            if (Game.Teams[0].Players[0] == this)
            {
                Partner = Game.Teams[0].Players[1];
                Team = Game.Teams[0];
            }
            else if (Game.Teams[0].Players[1] == this)
            {
                Partner = Game.Teams[0].Players[0];
                Team = Game.Teams[0];
            }
            else if (Game.Teams[1].Players[0] == this)
            {
                Partner = Game.Teams[1].Players[1];
                Team = Game.Teams[1];
            }
            else
            {
                Partner = Game.Teams[1].Players[0];
                Team = Game.Teams[1];
            }
            RevealedCard = Game.RevealedCard;
        }

        public override Bid GetBid()
        {
            if (Game.Phase == GamePhase.BidRound1)
            {
                var trump = Game.RevealedCard.Suit;
                if (!Cards.Any(x => x.Suit == trump)) return null;
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
                    score -= Game.RevealedCard.Number == 11 ? 10 : 5;
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
                var otherSuits = AllSuits.Where(suit => suit != Game.RevealedCard.Suit && Cards.Any(card => card.Suit == suit));
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
            var cards = Cards.Append(Game.RevealedCard);
            var discard = PickCardToDiscard(trump, cards);
            cards = cards.Where(x => x != discard);
            return CalculateHandScore(trump, cards);
        }

        private Card PickCardToDiscard(Suit trump, IEnumerable<Card> cards)
        {
            var transformedCards = cards.Select(x => TransformCard(x, trump));
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
            switch (trump)
            {
                case Suit.Clubs: return Suit.Spades;
                case Suit.Spades: return Suit.Clubs;
                case Suit.Hearts: return Suit.Diamonds;
                case Suit.Diamonds: return Suit.Hearts;
                default: throw new ArgumentOutOfRangeException(nameof(trump));
            }
            //return trump switch
            //{
            //    Suit.Clubs => Suit.Spades,
            //    Suit.Spades => Suit.Clubs,
            //    Suit.Hearts => Suit.Diamonds,
            //    Suit.Diamonds => Suit.Hearts,
            //    _ => throw new ArgumentOutOfRangeException(nameof(trump))
            //};
        }

        public override Card PickUpCard(Card card)
        {
            Cards.Add(card);
            var discard = PickCardToDiscard(Game.Bid.Suit, Cards);
            Cards.Remove(discard);
            return discard;
        }

        private Card Play(Card card)
        {
            if (Game.CardsInPlay.Count == 0) TrumpHasBeenLead |= card.Suit == Trump;
            Cards.Remove(card);
            return card;
        }

        //private static Card HighestCard(IEnumerable<Card> cards, Suit suit)
        //{
        //    return cards.Where(x => x.Suit == suit).OrderByDescending(x => x.Number).FirstOrDefault();
        //}

        public override Card GetCard()
        {
            //update cards in play list
            foreach (var play in Game.CardsInPlay)
            {
                CardWasPlayed(play.Card);
            }
            if (Game.CardsInPlay.Count > 0) TrumpHasBeenLead |= Game.CardsInPlay[0].Card.Suit == Trump;

            //are we leading?
            if (Game.CardsInPlay.Count == 0)
            {
                return Play(GetCardLead());
            }
            else
            {
                return Play(GetCardNotLead(Game.CardsInPlay[0].Card.Suit));
            }
        }

        private Card GetCardNotLead(Suit ledSuit)
        {
            if (Cards.Count == 1) return Cards[0]; //if only holding one card, play it
            var validCards = Cards.Where(x => x.Suit == ledSuit);
            var canFollowSuit = true;
            switch (validCards.Count())
            {
                case 0: validCards = Cards; canFollowSuit = false; break;
                case 1: return validCards.First(); //only one valid play
            }

            bool isFirstTrick = Game.TricksTaken[0] + Game.TricksTaken[1] == 0;
            bool isTrumpLed = ledSuit == Trump;
            bool partnerPlayed = Game.CardsInPlay.Any(x => x.PlayedBy == Partner);
            bool isLastToPlay = Game.CardsInPlay.Count == 3;
            bool partnerHasTrick = partnerPlayed && Game.CardsInPlay.OrderByDescending(x => x.Card.Suit == Trump ? 10 + x.Card.Number : x.Card.Suit == ledSuit ? x.Card.Number : 0).First().PlayedBy == Partner;
            var partnerCard = Game.CardsInPlay.Where(x => x.PlayedBy == Partner).FirstOrDefault().Card;
            var bestCardPlayed = BestCardPlayed();
            bool tryToBeatTrick = true;
            if (partnerHasTrick && isLastToPlay)
            {
                tryToBeatTrick = false;
            }
            else if (!isTrumpLed)
            {
                //if partner played ace, then don't try to beat trick
                if (partnerHasTrick)
                {
                    if (partnerCard.Number == 14 || partnerCard.Suit == Trump)
                        tryToBeatTrick = false;
                }
            }

            if (tryToBeatTrick)
            {
                //never beat partner by one
                if (partnerPlayed)
                {
                    var oneBetterThanPartner = OneBetter(partnerCard);
                    validCards = validCards.Where(x => x != oneBetterThanPartner);
                }
                validCards = validCards.ToList();
                if (canFollowSuit)
                {
                    //sort cards best-first
                    validCards = validCards.OrderByDescending(x => (x.Suit == Trump ? 10 : 0) + x.Number);
                    //play best card in hand
                    var bestCardInHand = validCards.First();
                    if (IsBetter(bestCardPlayed, bestCardInHand, ledSuit)) return bestCardInHand;
                }
                else
                {
                    //sort cards worst-first
                    validCards = validCards.OrderBy(x => (x.Suit == Trump ? 10 : 0) + x.Number);
                    //grab the lowest card that beats the trick
                    var lowestCardThatIsBetter = validCards.FirstOrDefault(c => IsBetter(bestCardPlayed, c, ledSuit));
                    if (lowestCardThatIsBetter != null) return lowestCardThatIsBetter;
                }
            }
            else
            {
                validCards = validCards.ToList();
            }
            //sort cards worst-first
            validCards = validCards.OrderBy(x => (x.Suit == Trump ? 10 : 0) + x.Number);
            //pick lowest card
            if (canFollowSuit)
            {
                //must follow suit
                return validCards.First();
            }
            //if we only have trump, play the lowest trump
            if (validCards.All(x => x.Suit == Trump))
            {
                return validCards.First();
            }
            //pick from the non-trumps
            validCards = validCards.Where(x => x.Suit != Trump);
            //try to short-suit self
            var card = validCards.GroupBy(x => x.Suit).Where(x => x.Count() == 1).Select(x => x.First()).FirstOrDefault();
            //at least 2 in every suit, so just pick lowest card
            if (card == null)
                card = validCards.First();
            //play the card
            return card;
        }

        /// <summary>
        /// Determines if card2 is better than card1, considering trump and the suit led
        /// </summary>
        private bool IsBetter(Card card1, Card card2, Suit ledSuit)
        {
            if (card1.Suit == Trump)
                return (card2.Suit == Trump && card2.Number > card1.Number);
            else if (card1.Suit == ledSuit)
                return (card2.Suit == Trump || (card2.Suit == ledSuit && card2.Number > card1.Number));
            else
                throw new ArgumentOutOfRangeException(nameof(card1));
        }

        private Card BestCardPlayed()
        {
            if (Game.CardsInPlay.Count == 0) return null;
            var suitLed = Game.CardsInPlay[0].Card.Suit;
            Card card = null;
            foreach (var play in Game.CardsInPlay)
            {
                if (card == null || IsBetter(card, play.Card, suitLed)) card = play.Card;
            }
            return card;
        }
        //11 j 12 q 13 k 14 a 15 j 16 j
        private Card OneBetter(Card card)
        {
            var number = card.Number;
            var suit = card.Suit;
            while (number++ < 16)
            {
                var card2 = new Card(number, suit);
                if (CardsLeftToPlay.Contains(card2)) return card2;
            }
            return null;
        }

        private Card GetCardLead()
        {
            var leadStyles = new List<LeadStyle>(6);
            if (IsBiddingTeam && Game.Bid.Alone)
            {
                //loner gameplay
                leadStyles.Add(LeadStyle.TrumpHighestHeld);
                leadStyles.Add(LeadStyle.OffsuitHighest);
                leadStyles.Add(LeadStyle.OffsuitHighestHeld);
            }
            else if (!IsBiddingTeam)
            {
                //non-bidding team gameplay
                leadStyles.Add(LeadStyle.OffsuitAce);
                leadStyles.Add(LeadStyle.OffsuitHighest);
                leadStyles.Add(LeadStyle.OffsuitLow);
                leadStyles.Add(LeadStyle.TrumpHighest);
            }
            else //if (Game.BiddingTeam == Team)
            {
                // this player bid -- or partner bid
                if (!TrumpHasBeenLead)
                {
                    //trump hasn't been led yet
                    leadStyles.Add(LeadStyle.TrumpRight);
                    leadStyles.Add(LeadStyle.TrumpLeft);
                    leadStyles.Add(LeadStyle.TrumpLow);
                }
                leadStyles.Add(LeadStyle.OffsuitAce);
                leadStyles.Add(LeadStyle.OffsuitHighest);
                leadStyles.Add(LeadStyle.OffsuitLow);
                if (TrumpHasBeenLead)
                {
                    leadStyles.Add(LeadStyle.TrumpHighest);
                    leadStyles.Add(LeadStyle.TrumpLow);
                }
            }
            var card = GetLead(leadStyles);
            return card;
        }

        private Card GetLead(IEnumerable<LeadStyle> leadStyles)
        {
            foreach (var style in leadStyles)
            {
                Card card = null;
                switch (style)
                {
                    case LeadStyle.OffsuitAce:
                        card = Cards.FirstOrDefault(x => x.Suit != Trump && x.Number == 14);
                        break;
                    case LeadStyle.OffsuitHighest:
                        card = CardsLeftToPlay.Where(x => x.Suit != Trump).OrderByDescending(x => x.Number).GroupBy(x => x.Suit).Select(x => x.First()).Where(x => Cards.Contains(x)).OrderByDescending(x => x.Number).FirstOrDefault();
                        if (card != null) card = Cards.FirstOrDefault(x => x == card); //reference instance in Cards, rather than created instance
                        break;
                    case LeadStyle.OffsuitHighestHeld:
                        card = Cards.Where(x => x.Suit != Trump).OrderByDescending(x => x.Number).FirstOrDefault();
                        break;
                    case LeadStyle.OffsuitLow:
                        //try to short-suit first
                        card = Cards.Where(x => x.Suit != Trump).GroupBy(x => x.Suit).Where(x => x.Count() == 1).Select(x => x.First()).OrderBy(x => x.Number).FirstOrDefault();
                        //or else pick the lowest off-suit card
                        if (card == null)
                        {
                            card = Cards.Where(x => x.Suit != Trump).OrderBy(x => x.Number).FirstOrDefault();
                        }
                        break;
                    case LeadStyle.TrumpRight:
                        card = Cards.FirstOrDefault(x => x == Game.RightBauer);
                        break;
                    case LeadStyle.TrumpLeft:
                        card = Cards.FirstOrDefault(x => x == Game.LeftBauer);
                        break;
                    case LeadStyle.TrumpHighest:
                        card = CardsLeftToPlay.Where(x => x.Suit == Trump).OrderByDescending(x => x.Number).FirstOrDefault();
                        if (card != null) card = Cards.FirstOrDefault(x => x == card); //reference instance in Cards, rather than created instance
                        break;
                    case LeadStyle.TrumpHighestHeld:
                        card = Cards.Where(x => x.Suit == Trump).OrderByDescending(x => x.Number).FirstOrDefault();
                        break;
                    case LeadStyle.TrumpLow:
                        card = Cards.Where(x => x.Suit == Trump).OrderBy(x => x.Number).FirstOrDefault();
                        break;
                    case LeadStyle.Lowest:
                        card = Cards.OrderBy(x => x.Suit == Trump ? 20 + x.Number : x.Number).First();
                        break;
                    default: //lowest
                        throw new InvalidOperationException();
                }
                if (card != null) return card;
            }
            //nothing matched, so just pick lowest card
            return Cards.OrderBy(x => x.Suit == Trump ? 20 + x.Number : x.Number).First();
        }

        private enum LeadStyle
        {
            OffsuitAce,
            OffsuitHighest,
            OffsuitHighestHeld,
            OffsuitLow,
            TrumpRight,
            TrumpLeft,
            TrumpHighest,
            TrumpLow,
            TrumpHighestHeld,
            Lowest,
        }
    }
}
