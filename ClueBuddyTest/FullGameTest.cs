//-----------------------------------------------------------------------
// <copyright file="FullGameTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddyTest {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class FullGameTest : TestBase {
		#region Constants and Fields

		private CompositeClue cc;

		private Game game;

		private Player interactivePlayer;

		#endregion

		#region Public Methods

		[TestMethod]
		public void Hancocks() {
			this.game = this.MasterDetective;
			this.game.AutoAnalysis = false; // speeds up test if we cal Analyze just once.

			Player andrew, cheryl, sarah, sheldon;
			this.game.Players.AddRange(new[] {
			                                 	andrew = this.interactivePlayer = new Player("Andrew") { CardsHeldCount = 7 },
			                                 	cheryl = new Player("Cheryl") { CardsHeldCount = 7 },
			                                 	sarah = new Player("Sarah") { CardsHeldCount = 6 },
			                                 	sheldon = new Player("Sheldon") { CardsHeldCount = 7 },
			                                 });
			this.game.Start();
			setupCards(andrew, "Poison", "Lead pipe", "Horseshoe", "Conservatory", "Rose", "Grey", "Mustard");

			this.suggest(andrew, "Courtyard", "Brunette", "Candlestick");
			this.disproved(cheryl, "Brunette");
			this.cannot_disprove(sarah);
			this.cannot_disprove(sheldon);
			this.game.Clues.Add(this.cc);

			this.suggest(sarah, "Trophy room", "Mustard", "Poison");
			this.cannot_disprove(sheldon);
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Courtyard", "Peach", "Horseshoe");
			this.disproved(andrew);
			this.disproved(cheryl);
			this.cannot_disprove(sarah);
			this.game.Clues.Add(this.cc);

			this.suggest(sarah, "Kitchen", "Green", "Knife");
			this.disproved(sheldon);
			this.cannot_disprove(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Gazebo", "Brunette", "Poison");
			this.disproved(andrew);
			this.disproved(cheryl);
			this.cannot_disprove(sarah);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Kitchen", "Peach", "Candlestick");
			this.disproved(cheryl, "Candlestick");
			this.cannot_disprove(sarah);
			this.disproved(sheldon, "Kitchen");
			this.game.Clues.Add(this.cc);

			this.suggest(sarah, "Library", "Grey", "Lead pipe");
			this.cannot_disprove(sheldon);
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.spy(cheryl, "Studio");
			this.spy(cheryl, "Drawing room");
			this.spy(cheryl, "Brunette");
			this.spy(cheryl, "Courtyard");
			this.spy(cheryl, "Scarlet");
			this.spy(cheryl, "Dining room");
			this.spy(cheryl, "Candlestick");

			this.suggest(sheldon, "Library", "Mustard", "Lead pipe");
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.disproved(sarah);
			this.game.Clues.Add(this.cc);

			this.spy(sarah, "Carriage House");

			this.suggest(sarah, "Gazebo", "Rose", "Revolver");
			this.disproved(sheldon);
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Billiard room", "Peach", "Revolver");
			this.cannot_disprove(andrew);
			this.cannot_disprove(cheryl);
			this.disproved(sarah);
			this.game.Clues.Add(this.cc);

			this.spy(sheldon, "Peach");

			this.suggest(andrew, "Trophy room", "White", "Rope");
			this.cannot_disprove(cheryl);
			this.disproved(sarah, "Rope");
			this.cannot_disprove(sheldon);
			this.game.Clues.Add(this.cc);

			this.suggest(sarah, "Trophy room", "Peach", "Poison");
			this.disproved(sheldon);
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Library", "Peach", "Revolver");
			this.cannot_disprove(andrew);
			this.cannot_disprove(cheryl);
			this.disproved(sarah);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Fountain", "Plum", "Wrench");
			this.cannot_disprove(cheryl);
			this.disproved(sarah, "Fountain");
			this.disproved(sheldon, "Plum");
			this.game.Clues.Add(this.cc);

			this.spy(sheldon, "Knife");

			this.suggest(sarah, "Conservatory", "Green", "Horseshoe");
			this.cannot_disprove(sheldon);
			this.disproved(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Courtyard", "Rose", "Lead pipe");
			this.disproved(andrew);
			this.disproved(cheryl);
			this.cannot_disprove(sarah);
			this.game.Clues.Add(this.cc);

			this.suggest(sarah, "Trophy room", "Plum", "Wrench");
			this.disproved(sheldon);
			this.cannot_disprove(andrew);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(sheldon, "Trophy room", "Green", "Revolver");
			this.cannot_disprove(andrew);
			this.cannot_disprove(cheryl);
			this.cannot_disprove(sarah);
			this.game.Clues.Add(this.cc);

			this.game.Analyze();

			foreach (Card card in this.game.Cards) {
				switch (card.Name) {
					case "Mr. Green":
					case "Trophy room":
						Assert.IsTrue(node(this.game.CaseFile, card).Value);
						break;
					case "Revolver":
					case "Wrench":
						Assert.IsFalse(node(this.game.CaseFile, card).HasValue);
						break;
					default:
						Assert.IsFalse(node(this.game.CaseFile, card).Value);
						break;
				}
			}

			GameTest.TestSerialize(this.TestContext, this.game);
		}

		[TestInitialize]
		public override void Setup() {
			base.Setup();
			this.game = null;
			this.interactivePlayer = null;
		}

		[TestMethod]
		public void Webbs() {
			this.game = this.MasterDetective;
			this.game.AutoAnalysis = false; // speeds up test if we call Analyze just once.

			Player andrew, cheryl, rebecca, dan, table;
			this.game.Players.AddRange(new[] {
			                                 	andrew = this.interactivePlayer = new Player("Andrew") { CardsHeldCount = 6 }, 
			                                 	cheryl = new Player("Cheryl") { CardsHeldCount = 6 }, 
			                                 	rebecca = new Player("Rebecca") { CardsHeldCount = 6 }, 
			                                 	dan = new Player("Dan") { CardsHeldCount = 6 }, 
			                                 	table = new Player("Table") { CardsHeldCount = 3 }, 
			                                 });
			this.game.Start();
			this.setupCards(andrew, "White", "Brunette", "Wrench", "Drawing room", "Rose", "Rope");
			this.setupCards(table, "Library", "Studio", "Fountain");

			this.suggest(andrew, "Courtyard", "Peacock", "Candlestick");
			this.disproved(rebecca, "Peacock");
			this.disproved(dan, "Courtyard");
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(rebecca, "Dining room", "Rose", "Rope");
			this.cannot_disprove(dan);
			this.disproved(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Trophy room", "Rose", "Rope");
			this.cannot_disprove(rebecca);
			this.cannot_disprove(dan);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Gazebo", "Green", "Horseshoe");
			this.disproved(rebecca, "Horseshoe");
			this.cannot_disprove(dan);
			this.disproved(cheryl, "Gazebo");
			this.game.Clues.Add(this.cc);

			this.suggest(dan, "Billiard room", "Grey", "Knife");
			this.disproved(cheryl);
			this.cannot_disprove(rebecca);
			this.game.Clues.Add(this.cc);

			this.spy(rebecca, "Poison");

			this.suggest(andrew, "Trophy room", "Peach", "Lead pipe");
			this.disproved(rebecca, "Peach");
			this.cannot_disprove(dan);
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(rebecca, "Kitchen", "White", "Revolver");
			this.disproved(dan);
			this.disproved(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Trophy room", "Scarlet", "Wrench");
			this.cannot_disprove(rebecca);
			this.cannot_disprove(dan);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Kitchen", "Mustard", "Candlestick");
			this.cannot_disprove(rebecca);
			this.disproved(dan, "Mustard");
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(rebecca, "Drawing room", "Scarlet", "Rope");
			this.disproved(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(dan, "Courtyard", "Plum", "Wrench");
			this.cannot_disprove(cheryl);
			this.disproved(rebecca);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Dining room", "Grey", "Lead pipe");
			this.disproved(rebecca, "Lead pipe");
			this.disproved(dan, "Grey");
			this.disproved(cheryl, "Dining room");
			this.game.Clues.Add(this.cc);

			this.game.Analyze();

			foreach (Card card in this.game.Cards) {
				switch (card.Name) {
					case "Trophy room":
					case "Mr. Green":
						Assert.IsTrue(node(this.game.CaseFile, card).Value);
						break;
					case "Knife":
					case "Candlestick":
						Assert.IsFalse(node(this.game.CaseFile, card).HasValue);
						break;
					default:
						Assert.IsFalse(node(this.game.CaseFile, card).Value);
						break;
				}
			}

			GameTest.TestSerialize(this.TestContext, this.game);
		}

		[TestMethod]
		public void Wrigleys() {
			this.game = this.Simpsons;
			this.game.AutoAnalysis = false;

			Player andrew, cheryl, jeff, julia;
			this.game.Players.AddRange(new[] {
			                                 	andrew = this.interactivePlayer = new Player("Andrew") { CardsHeldCount = 4 }, 
			                                 	cheryl = new Player("Cheryl") { CardsHeldCount = 5 }, 
			                                 	jeff = new Player("Jeff") { CardsHeldCount = 4 }, 
			                                 	julia = new Player("Julia") { CardsHeldCount = 5 }, 
			                                 });
			this.game.Start();
			this.setupCards("Extend-o-glove", "Plum", "Kwik e mart", "Mustard");

			this.suggest(jeff, "Springfield retirement castle", "Peacock", "Extend-o-glove");
			this.disproved(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(julia, "Androids dungeon", "Peacock", "Necklace");
			this.disproved(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Kwik e mart", "Mustard", "Necklace");
			this.cannot_disprove(jeff);
			this.disproved(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Androids dungeon", "White", "Slingshot");
			this.disproved(cheryl, "Androids dungeon");
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Kwik e mart", "Scarlet", "Slingshot");
			this.disproved(jeff, "Slingshot");
			this.game.Clues.Add(this.cc);

			this.suggest(jeff, "Simpson house", "Scarlet", "Plutonium rod");
			this.disproved(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Krusty loo studios", "Green", "Poison donut");
			this.cannot_disprove(cheryl);
			this.disproved(jeff, "Krusty loo studios");
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Kwik e mart", "White", "Plutonium rod");
			this.cannot_disprove(jeff);
			this.cannot_disprove(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(jeff, "Krusty loo studios", "Scarlet", "Plutonium rod");
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Barneys Bowl o rama", "Green", "Saxophone");
			this.cannot_disprove(cheryl);
			this.disproved(jeff, "Saxophone");
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Nuclear power plant", "Mustard", "Plutonium rod");
			this.cannot_disprove(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(jeff, "Nuclear power plant", "Scarlet", "Plutonium rod");
			this.disproved(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(julia, "Springfield retirement castle", "White", "Slingshot");
			this.cannot_disprove(cheryl);
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Barneys Bowl o rama", "Plum", "Saxophone");
			this.cannot_disprove(julia);
			this.game.Clues.Add(this.cc);

			this.suggest(andrew, "Burns manor", "Green", "Poison donut");
			this.disproved(cheryl, "Burns manor");
			this.game.Clues.Add(this.cc);

			this.suggest(cheryl, "Barneys Bowl o rama", "Scarlet", "Plutonium rod");
			this.cannot_disprove(jeff);
			this.game.Clues.Add(this.cc);

			this.game.Analyze();

			foreach (Card card in this.game.Cards) {
				switch (card.Name) {
					case "Barneys Bowl o rama":
					case "Mrs. White":
					case "Plutonium rod":
						Assert.IsTrue(node(this.game.CaseFile, card).Value);
						break;
					default:
						Assert.IsFalse(node(this.game.CaseFile, card).Value);
						break;
				}
			}

			GameTest.TestSerialize(this.TestContext, this.game);
		}

		#endregion

		#region Methods

		void cannot_disprove(params Player[] disprovingPlayers) {
			foreach (Player disprovingPlayer in disprovingPlayers) {
				this.cc.Responses[disprovingPlayer].Disproved = false;
			}
		}

		void disproved(Player disprovingPlayer) {
			this.disproved(disprovingPlayer, (Card)null);
		}

		void disproved(Player disprovingPlayer, Card cardShown) {
			this.cc.Responses[disprovingPlayer].Disproved = true;
			this.cc.Responses[disprovingPlayer].Alabi = cardShown;
		}

		void disproved(Player disprovingPlayer, string cardShown) {
			disproved(disprovingPlayer, this.find(cardShown));
		}

		Card find(string cardName) {
			try {
				return this.game.Cards.Where(c => c.Name.IndexOf(cardName) >= 0).First();
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException("cardName", cardName, "Card does not exist.");
			}
		}
		IEnumerable<Card> find(params string[] cardNames) {
			return this.game.Cards.Where(c => cardNames.Contains(c.Name));
		}

		CompositeClue newCC(Player suggestingPlayer) {
			return this.cc = new CompositeClue { Player = suggestingPlayer };
		}

		bool? node(ICardHolder holder, string cardName) {
			return this.game.Nodes.Where(n => n.CardHolder == holder && n.Card.Name == cardName).First().IsSelected;
		}
		bool? node(ICardHolder holder, Card card) {
			return this.game.Nodes.Where(n => n.CardHolder == holder && n.Card == card).First().IsSelected;
		}

		void setupCards(Player player, params string[] cardNames) {
			foreach (string cardName in cardNames) {
				this.game.Clues.Add(new SpyCard(player, this.find(cardName)));
			}
		}
		void setupCards(params string[] cardNames) {
			this.setupCards(this.interactivePlayer, cardNames);
		}

		void spy(Player player, string cardShown) {
			player.Game.Clues.Add(new SpyCard(player, this.find(cardShown)));
		}

		Suspicion suggest(Player suggestingPlayer, string place, string suspect, string weapon) {
			this.newCC(suggestingPlayer);
			return this.cc.Suspicion = new Suspicion((Suspect)this.find(suspect), (Weapon)this.find(weapon), (Place)this.find(place));
		}

		#endregion
	}
}
