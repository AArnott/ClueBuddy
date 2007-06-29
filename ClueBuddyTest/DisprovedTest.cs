using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class DisprovedTest : ClueTestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod]
		public void DisprovedConstructorTest() {
			Disproved target = new Disproved(disprovingPlayer, suggestion);
			Assert.AreSame(suggestion, target.Suspicion);
			Assert.AreSame(disprovingPlayer, target.Player);
			Assert.IsNull(target.CardShown);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DisprovedConstructorWithNullPlayerTest() {
			new Disproved(null, suggestion);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DisprovedConstructorWithNullSuspicionTest() {
			new Disproved(disprovingPlayer, null);
		}

		[TestMethod]
		public void DisprovedConstructorWithCardTest() {
			Disproved target = new Disproved(disprovingPlayer, suggestion, cardShown);
			Assert.AreSame(suggestion, target.Suspicion);
			Assert.AreSame(disprovingPlayer, target.Player);
			Assert.AreSame(cardShown, target.CardShown);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void DisprovedConstructorWithForeignCardTest() {
			Player disprovingPlayer = new Player("opponent");
			Suspicion suggestion = new Suspicion(new Suspect("test"), new Weapon("test"), new Place("test"));
			Card cardShown = new Suspect("some other suspect");
			new Disproved(disprovingPlayer, suggestion, cardShown);
		}

		[TestMethod]
		public void GetConstraintsWithoutCardTest() {
			Disproved target = new Disproved(disprovingPlayer, suggestion);
			var constraints = target.GetConstraints(nodes);
			Assert.AreEqual(1, constraints.Count());
			var c = constraints.First() as SelectionCountConstraint;
			Assert.IsNotNull(c);
			CollectionAssert.AllItemsAreUnique(c.Nodes.ToList());
			Assert.AreEqual(3, c.Nodes.Count());
			Assert.AreEqual(1, c.Min);
			Assert.AreEqual(3, c.Max);
			Assert.IsTrue(c.SelectionState);
		}

		[TestMethod]
		public void GetConstraintsWithCardTest() {
			Disproved target = new Disproved(disprovingPlayer, suggestion, cardShown);
			var constraints = target.GetConstraints(nodes);
			Assert.AreEqual(1, constraints.Count());
			var c = constraints.First() as SelectionCountConstraint;
			Assert.IsNotNull(c);
			Assert.AreEqual(1, c.Nodes.Count());
			Assert.AreSame((from n in nodes where n.Card == cardShown select n).First(), c.Nodes.First());
			Assert.AreEqual(1, c.Min);
			Assert.AreEqual(1, c.Max);
			Assert.IsTrue(c.SelectionState);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetContraintsNullNodesTest() {
			new Disproved(disprovingPlayer, suggestion).GetConstraints(null).Count();
		}

		/// <summary>
		/// Tests behavior when a set of nodes are provided that do not contain all the nodes needed
		/// to fill the constraint.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetContraintsMissingNodesTest() {
			new Disproved(disprovingPlayer, suggestion).GetConstraints(nodes.Where((n, i) => i == 0)).Count();
		}
	}
}
