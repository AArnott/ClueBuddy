using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	static partial class Extensions {
		public static void disproved(this Player player, Suspicion suggestion) {
			player.Game.Clues.Add(new Disproved(player, suggestion));
		}
		public static void disproved(this Player player, params Card[] cards) {
			player.Game.Clues.Add(new DisprovedAnyCards(player, cards));
		}
		public static void see_card(this Player player, params Card[] cards) {
			foreach (Card card in cards)
				player.Game.Clues.Add(new SpyCard(player, card));
		}
		public static void cannot_disprove(this Player player, Suspicion suggestion) {
			player.Game.Clues.Add(new CannotDisprove(player, suggestion));
		}
		public static void cannot_disprove(this Player player, params Card[] cards) {
			player.Game.Clues.Add(new CannotDisproveAnyCards(player, cards));
		}
		public static bool? has(this ICardHolder player, Card card) {
			return player.Game.IsCardHeld(player, card);
		}
		public static bool? has_not(this ICardHolder player, Card card) {
			bool? value = player.Game.IsCardHeld(player, card);
			return value.HasValue ? !value.Value : value;
		}
		public static void set(this CaseFile caseFile, Card card) {
			caseFile.Game.Nodes.Where(n => n.CardHolder == caseFile && n.Card == card).First().IsSelected = true;
			CompositeConstraint cc = new CompositeConstraint(caseFile.Game.Constraints);
			cc.ResolvePartially();
		}
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> addition) {
			foreach (T add in addition) {
				list.Add(add);
			}
		}
	}

	public class TestBase {

		private TestContext testContextInstance;

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		protected Player[] players;

		protected virtual Game StartPresetGame() {
			Game g = PreparePresetGame();
			g.Start();
			return g;
		}

		protected virtual Game PreparePresetGame() {
			Game g = Game.GreatDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			return g;
		}


		[TestInitialize]
		public virtual void Setup() {
			players = new Player[] { 
							   new Player("Player 1"), 
							   new Player("Player 2"), 
							   new Player("Player 3"), 
							   new Player("Player 4") 
						   };
		}
		[TestCleanup]
		public virtual void Cleanup() { }
	}
}
