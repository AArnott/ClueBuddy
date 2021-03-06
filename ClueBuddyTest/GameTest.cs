﻿//-----------------------------------------------------------------------
// <copyright file="GameTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddyTest {
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using NerdBank.Algorithms.NodeConstraintSelection;

	[TestClass]
	public class GameTest : TestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod]
		public void CardAssignmentsAcceptable() {
			Game g = MasterDetective;
			g.Players.AddRange(players);
			Assert.IsFalse(g.CardAssignmentsAcceptable);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void StartWithoutHandSizesAppropriatelySized() {
			Game g = MasterDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			players[0].CardsHeldCount--;
			Assert.IsFalse(g.CardAssignmentsAcceptable);
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
			{
				Assert.AreEqual(g.Players.Count + 1, g.Nodes.Where(n => n.Card == c).Count());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void AddPlayersAfterGameStartTest() {
			Game g = StartPresetGame();
			g.Players.Add(new Player("some new player"));
		}

		[TestMethod]
		public void CluesTest() {
			Game newGame = MasterDetective;
			Assert.IsNull(newGame.Clues);
		}

		[TestMethod]
		public void AddClueTest() {
			Game g = StartPresetGame();
			Player player = players[0];
			Assert.AreEqual(0, g.Clues.Count);
			var clue = new CannotDisprove(player, new Suspicion(g.Suspects.First(), g.Weapons.First(), g.Places.First()));
			g.Clues.Add(clue);
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
			g.AutoAnalysis = false;
			Suspicion s = new Suspicion(g.Suspects.First(), g.Weapons.First(), g.Places.First());
			//Debug.WriteLine("Adding that no one has these cards: " + s.Suspect.ToString() + ", " + s.Weapon.ToString() + ", " + s.Place.ToString());
			foreach (Player p in g.Players) {
				Clue clue = new CannotDisprove(p, s);
				g.Clues.Add(clue);
			}

			Assert.AreSame(s.Suspect, g.CaseFile.Suspect);
			Assert.AreSame(s.Weapon, g.CaseFile.Weapon);
			Assert.AreSame(s.Place, g.CaseFile.Place);
		}

		[TestMethod]
		public void CaseFileTest() {
			Game g = MasterDetective;
			Assert.IsNull(g.CaseFile);
		}

		[TestMethod]
		public void AssignApproximatePlayerHandSizesTest() {
			Game g = MasterDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			// Make sure that all cards (except 3 for the Case File) are distributed as evenly as possible.
			int expectedDistributedCards = g.Cards.Count() - CaseFile.CardsInCaseFile;
			int cardsPerPlayer = expectedDistributedCards / players.Length; // +1 for some players perhaps
			foreach (Player p in g.Players)
				Assert.IsTrue(Math.Abs(p.CardsHeldCount - cardsPerPlayer) <= 1);
		}

		[TestMethod]
		public void PlayersInOrderAfterTest() {
			Game g = StartPresetGame();
			CollectionAssert.AreEquivalent(new Player[] { players[1], players[2], players[3] }, g.PlayersInOrderAfter(players[0]).ToArray());
			CollectionAssert.AreEquivalent(new Player[] { players[2], players[3], players[0] }, g.PlayersInOrderAfter(players[1]).ToArray());
			CollectionAssert.AreEquivalent(new Player[] { players[3], players[0], players[1] }, g.PlayersInOrderAfter(players[2]).ToArray());
			CollectionAssert.AreEquivalent(new Player[] { players[0], players[1], players[2] }, g.PlayersInOrderAfter(players[3]).ToArray());
		}

		[TestMethod]
		public void SerializableTest() {
			Game game = StartPresetGame();
			game.Clues.Add(new CompositeClue() {
				Player = game.Players[0],
				Suspicion = new Suspicion(game.Suspects.First(), game.Weapons.First(), game.Places.First())
			});
			game.Clues.Add(new SpyCard(game.Players[1], game.Weapons.First()));
			TestSerialize(this.TestContext, game);
		}

		internal static void TestSerialize(TestContext context, Game game) {
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			formatter.Serialize(stream, game);
			stream.Position = 0;
			Game restoredGame;
			restoredGame = (Game)formatter.Deserialize(stream);
			Assert.AreEqual(game.Players.Count, restoredGame.Players.Count);
			Assert.AreEqual(game.Clues.Count, restoredGame.Clues.Count);

			foreach (Node n1 in game.Nodes) {
				Node n2 = restoredGame.Nodes.First(
					n => n.Card.Name == n1.Card.Name &&
					((n.CardHolder is CaseFile && n1.CardHolder is CaseFile) ||
					(n.CardHolder is Player && n1.CardHolder is Player && ((Player)n.CardHolder).Name == ((Player)n1.CardHolder).Name)));
				Assert.AreEqual(n1.IsSelected, n2.IsSelected);
			}
			Assert.AreEqual(game.Constraints.Count, restoredGame.Constraints.Count);
		}
	}
}
