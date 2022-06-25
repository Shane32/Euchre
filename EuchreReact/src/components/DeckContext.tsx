import React, { useState } from "react";
import { CardType } from "./Card";

const enum CardSuit {
    Spades = "s",
    Diamonds = "d",
    Hearts = "h",
    Clubs = "c",
}

const enum CardNumber {
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
}

interface ICardInfo {
    type: CardType;
    suit: CardSuit;
    pile: number;
    pileIndex: number;
    back: boolean;
}

interface IDeckContext {
    cards: ICardInfo[],
    setCard: (index: number, pile?: number, pileIndex?: number, back?: boolean) => void,
    setCards: (cards: Array<{ index: number, pile?: number, pileIndex?: number, back?: boolean }>) => void,
    reset: () => void,
}

const defaultContext: IDeckContext = {
    cards: [],
    setCard: () => { },
    setCards: () => { },
    reset: () => { },
};

const DeckContext = React.createContext<IDeckContext>(defaultContext);

export default DeckContext;

interface ICardState {
    pile: number,
    pileIndex: number,
    back: boolean,
}

interface IState {
    cardStates: { [index: number]: ICardState },
}

const defaultState: IState = {
    cardStates: {}
};

function suitForCard(card: CardType) {
    return card[1] as ("s" | "d" | "h" | "c") as CardSuit;
}

const numberCardTranslation = {
    "A": 1,
    "2": 2,
    "3": 3,
    "4": 4,
    "5": 5,
    "6": 6,
    "7": 7,
    "8": 8,
    "9": 9,
    "T": 10,
    "J": 11,
    "Q": 12,
    "K": 13,
};

function numberForCard(card: CardType) {
    return numberCardTranslation[card[0] as ("A" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" | "T" | "J" | "Q" | "K")];
}

export const TableController = (props: { deck: CardType[], children: any }) => {
    const [state, setState] = useState<IState>(defaultState);
    const cards: ICardInfo[] = props.deck.map((card, index) => {
        const cardState = state.cardStates[index];
        return {
            type: card,
            suit: suitForCard(card),
            number: numberForCard(card),
            pile: cardState?.pile ?? 0,
            pileIndex: cardState?.pileIndex ?? 0,
            back: cardState?.back ?? true,
        };
    });
    const context: IDeckContext = {
        cards: cards,
        setCard: (index, pile, pileIndex, back) => {
            setState(oldState => {
                const oldCard = oldState.cardStates[index] ?? {
                    pile: 0,
                    pileIndex: 0,
                    back: true,
                };
                if ((oldCard.pile === pile || pile === undefined) &&
                    (oldCard.pileIndex === pileIndex || pileIndex === undefined) &&
                    (oldCard.back === back || back === undefined))
                    return oldState;
                const newCardStates = {
                    ...oldState.cardStates,
                    [index]: {
                        pile: pile ?? oldCard.pile,
                        pileIndex: pileIndex ?? oldCard.pileIndex,
                        back: back ?? oldCard.back,
                    },
                };
                return {
                    ...oldState,
                    cardStates: newCardStates,
                };
            });
        },
        setCards: (cardsToSet) => {
            setState(oldState => {
                if (cardsToSet.length === 0)
                    return oldState;
                const newCardStates = { ...oldState.cardStates };
                for (let i = 0; i < cardsToSet.length; i++) {
                    const cardToSet = cardsToSet[i];
                    const oldCardState = newCardStates[cardToSet.index];
                    const newCardState = {
                        pile: cardToSet.pile ?? oldCardState.pile,
                        pileIndex: cardToSet.pileIndex ?? oldCardState.pileIndex,
                        back: cardToSet.back ?? oldCardState.back,
                    };
                    newCardStates[cardToSet.index] = newCardState;
                }
                return {
                    ...oldState,
                    cardStates: newCardStates,
                };
            });
        },
        reset: () => {
            setState(defaultState);
        },
    };
    return <DeckContext.Provider value={context}>{props.children}</DeckContext.Provider>;
}


