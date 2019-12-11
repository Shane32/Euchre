using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Euchre
{
    class Game
    {
        private Deck Deck;
        public GamePhase Phase { get; private set; }
        public readonly Team[] Teams = new Team[2];
        public Team BiddingTeam { get; private set; }
        public Bid Bid { get; private set; }
        public readonly int[] TricksTaken = new int[2];
        public Player Dealer { get; private set; }
        public Player Turn { get; private set; }
        public Player SkipPlayer { get; private set; }
        public readonly Player[] Players = new Player[4];
        public readonly List<(Card Card, Player PlayedBy)> CardsInPlay = new List<(Card Card, Player PlayedBy)>();
        public Card RevealedCard { get; private set; }
        private List<Card> Jacks;
        public Card RightBauer { get; private set; }
        public Card LeftBauer { get; private set; }
        
        public Game(Player team1Player1, Player team1Player2, Player team2Player1, Player team2Player2)
        {
            Phase = GamePhase.Deal;
            Teams[0] = new Team(team1Player1, team1Player2);
            Teams[1] = new Team(team2Player1, team2Player2);
            Players[0] = team1Player1;
            Players[1] = team2Player1;
            Players[2] = team1Player2;
            Players[3] = team2Player2;
            Dealer = PickRandomDealer();
        }

        public void PlayGame()
        {
            while (Phase != GamePhase.GameOver)
            {
                Next();
            }
        }

        public Player PickRandomDealer()
        {
            //pick a number from 0-3
            byte[] bytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            Random rnd = new Random(BitConverter.ToInt32(bytes, 0));
            int playerNum = rnd.Next(0, 4);
            return Players[playerNum];
        }

        public void Next()
        {
            switch(Phase)
            {
                case GamePhase.Deal:
                    Deal();
                    Phase = GamePhase.BidRound1;
                    Turn = GetNextPlayer(Dealer);
                    BiddingTeam = null;
                    Bid = null;
                    SkipPlayer = null;
                    break;
                case GamePhase.BidRound1:
                case GamePhase.BidRound2:
                    var bid = Turn.GetBid();
                    if (bid == null) // pass
                    {
                        if (Phase == GamePhase.BidRound1)
                        {
                            //if all 4 players passed, go to round 2
                            if (Turn == Dealer) Phase = GamePhase.BidRound2;
                            //in any case, next player's turn to bid
                            Turn = GetNextPlayer(Turn);
                        }
                        else
                        {
                            //if all 4 players passed, need to re-deal
                            if (Turn == Dealer)
                            {
                                Dealer = GetNextPlayer(Dealer);
                                Phase = GamePhase.Deal;
                                Deck = Deck.ShuffleNew();
                            }
                            else
                            {
                                //otherwise, next person gets to bid
                                Turn = GetNextPlayer(Turn);
                            }
                        }
                    }
                    else // bid
                    {
                        Bid = bid;
                        BiddingTeam = Teams[0].Players.Contains(Turn) ? Teams[0] : Teams[1];
                        Turn = GetNextPlayer(Dealer);
                        //set SkipPlayer
                        Phase = GamePhase.GamePlay;
                        CardsInPlay.Clear();
                        TricksTaken[0] = 0;
                        TricksTaken[1] = 0;
                        SetJacks();
                    }
                    break;
                case GamePhase.GamePlay:
                    //get card from player, add to CardsInPlay
                    var card = Turn.GetCard();
                    CardsInPlay.Add((card, Turn));
                    //count to see how many cards were played
                    //4 normally, but 3 when going alone
                    if (CardsInPlay.Count == (Bid.Alone ? 3 : 4))
                    {
                        //trick is over
                        //determine who won the trick
                        Player trickWinner = DetermineTrickWinner();
                        //increment the number of tricks taken for that team
                        TricksTaken[Teams[0].Players.Contains(trickWinner) ? 0 : 1] += 1;
                        //if played 5 tricks, then end of hand
                        if ((TricksTaken[0] + TricksTaken[1]) == 5)
                        {
                            //see if bidding team won or not
                            UpdateScore();
                            if (Teams[0].Score >= 10 || Teams[1].Score >= 10)
                            {
                                Phase = GamePhase.GameOver;
                            }
                            else
                            {
                                Phase = GamePhase.Deal;
                            }
                        }
                        else //still more tricks left to play
                        {
                            //winner of last trick leads next trick
                            Turn = trickWinner;
                            CardsInPlay.Clear();
                        }
                    }
                    else //not yet played a full trick
                    {
                        //next player's turn to play a card
                        Turn = GetNextPlayer(Turn);
                    }
                    break;
            }
        }

        public Player DetermineTrickWinner()
        {
            Suit suitLed = CardsInPlay[0].Card.Suit;
            //look at all of the cards in play and determine highest card
            var winningCard = CardsInPlay.OrderByDescending(c => CalculateCardValue(c.Card, suitLed)).First();

            return winningCard.PlayedBy;

        }

        public int CalculateCardValue(Card card, Suit suitLed)
        {
            int weightedNumber;
            if (card.Suit == Bid.Suit) //if trump
            {
                weightedNumber = card.Number + 10;
            }
            else if (card.Suit == suitLed) //if in the suit led
            {
                weightedNumber = card.Number;
            }
            else //if off-suit
            {
                weightedNumber = 0;
            }

            return weightedNumber;
        }

        public void UpdateScore()
        {
            var biddingTeamIndex = Teams[0] == BiddingTeam ? 0 : 1;
            var nonBiddingTeam = Teams[biddingTeamIndex ^ 1];
            var biddingTeamTricksTaken = TricksTaken[Teams[0] == BiddingTeam ? 0 : 1];
            if (Bid.Alone)
            {
                if (biddingTeamTricksTaken == 5)
                {
                    BiddingTeam.Score += 4;
                }
                else if (biddingTeamTricksTaken >= 3)
                {
                    BiddingTeam.Score += 1;
                }
                else
                {
                    nonBiddingTeam.Score += 2;
                }
            }
            else
            {
                if (biddingTeamTricksTaken == 5)
                {
                    BiddingTeam.Score += 2;
                }
                else if (biddingTeamTricksTaken >= 3)
                {
                    BiddingTeam.Score += 1;
                }
                else
                {
                    nonBiddingTeam.Score += 2;
                }
            }
        }

        public Player GetNextPlayer(Player player)
        {
            //look through players, find match, get next player
            for (int i = 0; i < 4; i++)
            {
                if (Players[i] == player)
                {
                    var ret = Players[(++i >= 4) ? 0 : i];
                    if (ret == SkipPlayer) return GetNextPlayer(ret); else return ret;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(player));
        }

        public void Deal()
        {
            //shuffle the deck
            Deck = Deck.ShuffleNew();
            //initialize all AIs
            for (int i = 0; i < 4; i++)
            {
                Players[i].NewDeal(this);
            }
            //deal the cards out, keeping track of the jacks, so they can be converted into bauers later
            Jacks = new List<Card>();
            for (int i = 0; i < 20; i++)
            {
                var card = Deck.PullCard();
                if (card.Number == 11) Jacks.Add(card);
                var playerNum = i % 4;
                Players[playerNum].GiveCard(card);
            }
            //flip up a card
            RevealedCard = Deck.PullCard();
            if (RevealedCard.Number == 11) Jacks.Add(RevealedCard);
        }

        public void SetJacks()
        {
            var trump = Bid.Suit;
            Suit leftSuit;
            switch (trump)
            {
                case Suit.Spades: leftSuit = Suit.Clubs; break;
                case Suit.Clubs: leftSuit = Suit.Spades; break;
                case Suit.Diamonds: leftSuit = Suit.Hearts; break;
                case Suit.Hearts: leftSuit = Suit.Diamonds; break;
                default: throw new InvalidOperationException("Invalid trump suit");
            }
            RightBauer = Jacks.SingleOrDefault(x => x.Suit == trump);
            LeftBauer = Jacks.SingleOrDefault(x => x.Suit == leftSuit);

            //in case the right/left are buried, check for null
            if (RightBauer != null) RightBauer.Number = 16;
            if (LeftBauer != null)
            {
                LeftBauer.Number = 15;
                LeftBauer.Suit = trump;
            }
        }
    }

    enum GamePhase
    {
        Deal,
        BidRound1,
        BidRound2,
        GamePlay,
        GameOver,
    }

    class Team
    {
        public readonly Player[] Players = new Player[2];
        public int Score;

        public Team(Player player1, Player player2)
        {
            Players[0] = player1;
            Players[1] = player2;
            Score = 0;
        }
    }
}
