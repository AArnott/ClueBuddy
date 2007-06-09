﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class FullGameTest {
		Game game;
		Player interactivePlayer;
		CompositeClue cc;

		[TestInitialize]
		public void Setup() {
			game = null;
			interactivePlayer = null;
		}

		void setupCards(Player player, params string[] cardNames) {
			foreach (string cardName in cardNames) {
				game.Clues.Add(new SpyCard(player, find(cardName)));
			}
		}
		void setupCards(params string[] cardNames) {
			setupCards(interactivePlayer, cardNames);
		}
		Card find(string cardName) {
			try {
				return game.Cards.Where(c => c.Name == cardName).First();
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException("cardName", cardName, "Card does not exist.");
			}
		}
		IEnumerable<Card> find(params string[] cardNames) {
			return game.Cards.Where(c => cardNames.Contains(c.Name));
		}
		Suspicion suggest(Player suggestingPlayer, string place, string suspect, string weapon) {
			newCC(suggestingPlayer);
			return cc.Suspicion = new Suspicion((Suspect)find(suspect), (Weapon)find(weapon), (Place)find(place));
		}
		CompositeClue newCC(Player suggestingPlayer) {
			return cc = new CompositeClue() { Player = suggestingPlayer };
		}
		void disproved(Player disprovingPlayer) {
			disproved(disprovingPlayer, (Card)null);
		}
		void disproved(Player disprovingPlayer, Card cardShown) {
			cc.Responses[disprovingPlayer].Disproved = true;
			cc.Responses[disprovingPlayer].Alabi = cardShown;
		}
		void disproved(Player disprovingPlayer, string cardShown) {
			disproved(disprovingPlayer, find(cardShown));
		}
		void cannot_disprove(params Player[] disprovingPlayers) {
			foreach (Player disprovingPlayer in disprovingPlayers) {
				cc.Responses[disprovingPlayer].Disproved = false;
			}
		}
		void spy(Player player, string cardShown) {
			player.Game.Clues.Add(new SpyCard(player, find(cardShown)));
		}
		bool? node(ICardHolder holder, string cardName) {
			return game.Nodes.Where(n => n.CardHolder == holder && n.Card.Name == cardName).First().IsSelected;
		}
		bool? node(ICardHolder holder, Card card) {
			return game.Nodes.Where(n => n.CardHolder == holder && n.Card == card).First().IsSelected;
		}

		[TestMethod]
		public void Wrigleys() {
			game = Game.Simpsons;

			Player andrew, cheryl, jeff, julia;
			game.Players.AddRange(new Player[] {
				andrew = interactivePlayer = new Player("Andrew") { CardsHeldCount = 4 },
				cheryl = new Player("Cheryl") { CardsHeldCount = 5 },
				jeff = new Player("Jeff") { CardsHeldCount = 4 },
				julia = new Player("Julia") { CardsHeldCount = 5 },
			});
			game.Start();
			setupCards("Extend-o-glove", "Plum", "Kwik e mart", "Mustard");

			suggest(jeff, "Springfield retirement castle", "Peacock", "Extend-o-glove");
			disproved(julia);
			game.Clues.Add(cc);

			suggest(julia, "Androids dungeon", "Peacock", "Necklace");
			disproved(cheryl);
			game.Clues.Add(cc);

			suggest(cheryl, "Kwik e mart", "Mustard", "Necklace");
			cannot_disprove(jeff);
			disproved(julia);
			game.Clues.Add(cc);

			suggest(andrew, "Androids dungeon", "White", "Slingshot");
			disproved(cheryl, "Androids dungeon");
			game.Clues.Add(cc);

			suggest(andrew, "Kwik e mart", "Scarlet", "Slingshot");
			disproved(jeff, "Slingshot");
			game.Clues.Add(cc);

			suggest(jeff, "Simpson house", "Scarlet", "Plutonium rod");
			disproved(julia);
			game.Clues.Add(cc);

			suggest(andrew, "Krusty loo studios", "Green", "Poison donut");
			cannot_disprove(cheryl);
			disproved(jeff, "Krusty loo studios");
			game.Clues.Add(cc);

			suggest(cheryl, "Kwik e mart", "White", "Plutonium rod");
			cannot_disprove(jeff);
			cannot_disprove(julia);
			game.Clues.Add(cc);

			suggest(jeff, "Krusty loo studios", "Scarlet", "Plutonium rod");
			cannot_disprove(cheryl);
			game.Clues.Add(cc);

			suggest(andrew, "Barneys Bowl o rama", "Green", "Saxophone");
			cannot_disprove(cheryl);
			disproved(jeff, "Saxophone");
			game.Clues.Add(cc);
			
			suggest(cheryl, "Nuclear power plant", "Mustard", "Plutonium rod");
			cannot_disprove(julia);
			game.Clues.Add(cc);
			
			suggest(jeff, "Nuclear power plant", "Scarlet", "Plutonium rod");
			disproved(cheryl);
			game.Clues.Add(cc);

			suggest(julia, "Springfield retirement castle", "White", "Slingshot");
			cannot_disprove(cheryl);
			game.Clues.Add(cc);
			
			suggest(cheryl, "Barneys Bowl o rama", "Plum", "Saxophone");
			cannot_disprove(julia);
			game.Clues.Add(cc);
			
			suggest(andrew, "Burns manor", "Green", "Poison donut");
			disproved(cheryl, "Burns manor");
			game.Clues.Add(cc);
			
			suggest(cheryl, "Barneys Bowl o rama", "Scarlet", "Plutonium rod");
			cannot_disprove(jeff);
			game.Clues.Add(cc);

			foreach (Card card in game.Cards) {
				switch (card.Name) {
					case "Barneys Bowl o rama":
					case "White":
					case "Plutonium rod":
						Assert.IsTrue(node(game.CaseFile, card).Value);
						break;
					default:
						Assert.IsFalse(node(game.CaseFile, card).Value);
						break;
				}
			}
		}

		[TestMethod]
		public void Webbs() {
			game = Game.GreatDetective;

			Player andrew, cheryl, rebecca, dan, table;
			game.Players.AddRange(new Player[] {
				andrew = interactivePlayer = new Player("Andrew") { CardsHeldCount = 6 },
				cheryl = new Player("Cheryl") { CardsHeldCount = 6 },
				rebecca = new Player("Rebecca") { CardsHeldCount = 6 },
				dan = new Player("Dan") { CardsHeldCount = 6 },
				table = new Player("Table") { CardsHeldCount = 3 },
			});
			game.Start();
			setupCards(andrew, "White", "Brunette", "Wrench", "Drawing room", "Rose", "Rope");
			setupCards(table, "Library", "Studio", "Fountain");

			suggest(andrew, "Courtyard", "Peacock", "Candlestick");
			disproved(rebecca, "Peacock");
			disproved(dan, "Courtyard");
			cannot_disprove(cheryl);
			game.Clues.Add(cc);

			suggest(rebecca, "Dining room", "Rose", "Rope");
			cannot_disprove(dan);
			disproved(cheryl);
			game.Clues.Add(cc);

			suggest(cheryl, "Trophy room", "Rose", "Rope");
			cannot_disprove(rebecca);
			cannot_disprove(dan);
			game.Clues.Add(cc);

			suggest(andrew, "Gazebo", "Green", "Horseshoe");
			disproved(rebecca, "Horseshoe");
			cannot_disprove(dan);
			disproved(cheryl, "Gazebo");
			game.Clues.Add(cc);

			suggest(dan, "Billiard room", "Gray", "Knife");
			disproved(cheryl);
			cannot_disprove(rebecca);
			game.Clues.Add(cc);

			spy(rebecca, "Poison");

			suggest(andrew, "Trophy room", "Peach", "Lead pipe");
			disproved(rebecca, "Peach");
			cannot_disprove(dan);
			cannot_disprove(cheryl);
			game.Clues.Add(cc);

			suggest(rebecca, "Kitchen", "White", "Revolver");
			disproved(dan);
			disproved(cheryl);
			game.Clues.Add(cc);

			suggest(cheryl, "Trophy room", "Scarlet", "Wrench");
			cannot_disprove(rebecca);
			cannot_disprove(dan);
			game.Clues.Add(cc);

			suggest(andrew, "Kitchen", "Mustard", "Candlestick");
			cannot_disprove(rebecca);
			disproved(dan, "Mustard");
			cannot_disprove(cheryl);
			game.Clues.Add(cc);

			suggest(rebecca, "Drawing room", "Scarlet", "Rope");
			disproved(cheryl);
			game.Clues.Add(cc);

			suggest(dan, "Courtyard", "Plum", "Wrench");
			cannot_disprove(cheryl);
			disproved(rebecca);
			game.Clues.Add(cc);

			suggest(andrew, "Dining room", "Gray", "Lead pipe");
			disproved(rebecca, "Lead pipe");
			disproved(dan, "Gray");
			disproved(cheryl, "Dining room");
			game.Clues.Add(cc);

			foreach (Card card in game.Cards) {
				switch (card.Name) {
					case "Trophy room":
					case "Green":
						Assert.IsTrue(node(game.CaseFile, card).Value);
						break;
					case "Knife":
					case "Candlestick":
						Assert.IsFalse(node(game.CaseFile, card).HasValue);
						break;
					default:
						Assert.IsFalse(node(game.CaseFile, card).Value);
						break;
				}
			}
		}

	}
}