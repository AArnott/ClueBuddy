﻿namespace ClueBuddyTest {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using NerdBank.Algorithms.NodeConstraintSelection;

	static partial class Extensions {
		public static void disproved(this Player player, Suspicion suggestion) {
			player.Game.Clues.Add(new Disproved(player, suggestion));
		}

		public static void disproved(this Player player, params Card[] cards) {
			player.Game.Clues.Add(new DisprovedAnyCards(player, cards));
		}

		public static void see_card(this Player player, params Card[] cards) {
			foreach (Card card in cards) {
				player.Game.Clues.Add(new SpyCard(player, card));
			}
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
		protected Player[] players;

		private Game loadGameVariety(string name) {
			string fileName = Path.Combine(this.TestContext.TestDeploymentDir, name + "." + GameVariety.DefaultFileExtension);
			using (Stream s = new FileStream(fileName, FileMode.Open)) {
				return GameVariety.LoadFrom(s).Initialize();
			}
		}

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		protected Game MasterDetective {
			get { return loadGameVariety("Master Detective"); }
		}

		protected Game Simpsons {
			get { return loadGameVariety("Simpsons"); }
		}

		public virtual void Setup() {
			players = new Player[] { 
			                       	new Player("Player 1"), 
			                       	new Player("Player 2"), 
			                       	new Player("Player 3"), 
			                       	new Player("Player 4") 
			                       };
		}

		protected virtual Game StartPresetGame() {
			Game g = PreparePresetGame();
			g.Start();
			return g;
		}

		protected virtual Game PreparePresetGame() {
			Game g = MasterDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			return g;
		}
	}
}
