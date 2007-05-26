using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class GameTest : TestBase {
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void StartWithNoPlayersTest() {
			Game g = Game.GreatDetective;
			g.Start();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void StartWithPlayersWithoutHandSizesTest() {
			Game g = Game.GreatDetective;
			g.Players.AddRange(players);
			g.Start();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void StartWithoutHandSizesAppropriatelySized() {
			Game g = Game.GreatDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			Debug.Assert(players[0].CardsHeldCount > 1);
			players[0].CardsHeldCount--;
			g.Start();
		}

		[TestMethod]
		public void StartPresetGameTest() {
			Game g = StartPresetGame();
			Assert.IsNotNull(g.CaseFile);
			Assert.AreEqual((g.Players.Count + 1) * g.Cards.Count(), g.Nodes.Count());
			foreach (Player p in g.Players) {
				Assert.AreSame(g, p.Game);
				Assert.AreEqual(g.Cards.Count(), g.Nodes.Where(n => n.CardHolder == p).Count());
			}
			foreach (Card c in g.Cards)
				Assert.AreEqual(g.Players.Count + 1, g.Nodes.Where(n => n.Card == c).Count());
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void AddPlayersAfterGameStartTest() {
			Game g = StartPresetGame();
			g.Players.Add(new Player("some new player"));
		}

		[TestMethod]
		public void CluesTest() {
			Game newGame = Game.GreatDetective;
			Assert.IsNull(newGame.Clues);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddNullClueTest() {
			Game g = StartPresetGame();
			g.AddClue(null);
		}

		[TestMethod]
		public void AddClueTest() {
			Game g = StartPresetGame();
			Player player = players[0];
			Assert.AreEqual(0, g.Clues.Count());
			var clue = new CannotDisprove(player, new Suspicion(g.Suspects.First(), g.Weapons.First(), g.Locations.First()));
			g.AddClue(clue);
			CollectionAssert.AreEquivalent(new Clue[] { clue }, g.Clues.ToArray());
			// So the clue is added to the list of clues.  Now see that the appropriate nodes were affected.
			foreach (Node node in g.Nodes.Where(n => n.CardHolder == player && clue.Suspicion.Cards.Contains(n.Card)))
				Assert.IsFalse(node.IsSelected.Value);
		}

		[TestMethod]
		public void CaseFileSetCardDoesNotViolateConstraintsTest() {
			Game g = StartPresetGame();
			g.CaseFile.set(g.Weapons.First());
			CompositeConstraint cc = new CompositeConstraint(g.Constraints);
			Assert.IsTrue(cc.IsSatisfiable);
		}

		[TestMethod]
		public void AddSeveralCluesTest() {
			Game g = StartPresetGame();
			Suspicion s = new Suspicion(g.Suspects.First(), g.Weapons.First(), g.Locations.First());
			Debug.WriteLine("Adding that no one has these cards: " + s.Suspect.ToString() + ", " + s.Weapon.ToString() + ", " + s.Location.ToString());
			foreach (Player p in g.Players) {
				Clue clue = new CannotDisprove(p, s);
				g.AddClue(clue);
			}
			Assert.AreSame(s.Suspect, g.CaseFile.Suspect);
			Assert.AreSame(s.Weapon, g.CaseFile.Weapon);
			Assert.AreSame(s.Location, g.CaseFile.Location);
		}

		[TestMethod]
		public void CaseFileTest() {
			Game g = Game.GreatDetective;
			Assert.IsNull(g.CaseFile);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void AssignApproximatePlayerHandSizesBeforeAddingPlayersTest() {
			Game.GreatDetective.AssignApproximatePlayerHandSizes();
		}

		[TestMethod]
		public void AssignApproximatePlayerHandSizesTest() {
			Game g = Game.GreatDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			// Make sure that all cards (except 3 for the Case File) are distributed as evenly as possible.
			int expectedDistributedCards = g.Cards.Count() - CaseFile.CardsInCaseFile;
			int cardsPerPlayer = expectedDistributedCards / players.Length; // +1 for some players perhaps
			foreach (Player p in g.Players)
				Assert.IsTrue(Math.Abs(p.CardsHeldCount - cardsPerPlayer) <= 1);
		}

		[TestMethod]
		public void AnalysisDepthTest() {
			Game g = Game.GreatDetective;
			Assert.IsTrue(g.AnalysisDepth >= 0);
			int originalDepth = g.AnalysisDepth;
			g.AnalysisDepth++;
			Assert.AreEqual(originalDepth + 1, g.AnalysisDepth);
			g.AnalysisDepth = 0;
			Assert.AreEqual(0, g.AnalysisDepth);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void AnalysisDepthNegativeTest() {
			Game.GreatDetective.AnalysisDepth = -1;
		}
	}
}
