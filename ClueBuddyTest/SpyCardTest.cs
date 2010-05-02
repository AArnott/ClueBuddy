namespace ClueBuddyTest {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using NerdBank.Algorithms.NodeConstraintSelection;

	[TestClass]
	public class SpyCardTest : TestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod]
		public void SpyCardConstructorTest() {
			Player playerShowingCard = new Player("player");
			Card cardSeen = new Weapon("card");
			SpyCard target = new SpyCard(playerShowingCard, cardSeen);
			Assert.AreSame(playerShowingCard, target.Player);
			Assert.AreSame(cardSeen, target.Card);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SpyCardConstructorNullPlayerTest() {
			new SpyCard(null, new Weapon("card"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SpyCardConstructorNullCardTest() {
			new SpyCard(new Player("player"), null);
		}

		[TestMethod]
		public void GetConstraintsTest() {
			Player playerShowingCard = new Player("test");
			Card cardSeen = new Weapon("test");
			Card anotherCard = new Suspect("test");
			Node[] nodes = new Node[] {
							   new Node(playerShowingCard, cardSeen),
							   new Node(playerShowingCard, anotherCard),
						   };
			SpyCard target = new SpyCard(playerShowingCard, cardSeen);
			var actual = target.GetConstraints(nodes);
			Assert.AreEqual(1, actual.Count());
			SelectionCountConstraint c = actual.First() as SelectionCountConstraint;
			Assert.IsNotNull(c);
			Assert.AreEqual(1, c.Min);
			Assert.AreEqual(1, c.Max);
			Assert.AreEqual(1, c.Nodes.Count());
			Assert.AreSame(nodes[0], c.Nodes.First());
		}

		/// <summary>
		/// Tests behavior when a set of nodes are provided that do not contain all the nodes needed
		/// to fill the constraint.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetContraintsMissingNodesTest() {
			Player playerShowingCard = new Player("test");
			Card cardSeen = new Weapon("test");
			Card anotherCard = new Suspect("test");
			Node[] nodes = new Node[] {
							   new Node(playerShowingCard, cardSeen),
							   new Node(playerShowingCard, anotherCard),
						   };
			new SpyCard(playerShowingCard, cardSeen).GetConstraints(nodes.Where((n, i) => i == 1)).Count();
		}
	}
}
