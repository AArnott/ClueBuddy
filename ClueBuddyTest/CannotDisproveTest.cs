using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddyTest {
	[TestClass]
	public class CannotDisproveTest : ClueTestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod()]
		public void CannotDisproveConstructorTest() {
			var target = new CannotDisprove(disprovingPlayer, suggestion);
			Assert.AreSame(disprovingPlayer, target.Player);
			Assert.AreSame(suggestion, target.Suspicion);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CannotDisproveConstructorWithNullPlayerTest() {
			new CannotDisprove(null, suggestion);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CannotDisproveConstructorWithNullSuspicionTest() {
			new CannotDisprove(disprovingPlayer, null);
		}

		[TestMethod()]
		public void GetConstraintsTest() {
			var target = new CannotDisprove(disprovingPlayer, suggestion);
			var constraints = target.GetConstraints(nodes);
			Assert.AreEqual(1, constraints.Count());
			var c = constraints.First() as SelectionCountConstraint;
			Assert.IsNotNull(c);
			Assert.AreEqual(0, c.Max);
			Assert.AreEqual(suggestion.Cards.Count(), c.Nodes.Count());
			// make sure that all nodes tying disprovingPlayer to the cards in suggestion
			// are part of the constraint.
			var tyingNodes = nodes.Where(n => n.CardHolder == disprovingPlayer && suggestion.Cards.Contains(n.Card));
			Assert.IsTrue(tyingNodes.All(n => c.Nodes.Contains(n)));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetContraintsNullNodesTest() {
			new CannotDisprove(disprovingPlayer, suggestion).GetConstraints(null).Count();
		}

		/// <summary>
		/// Tests behavior when a set of nodes are provided that do not contain all the nodes needed
		/// to fill the constraint.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))] 
		public void GetContraintsMissingNodesTest() {
			new CannotDisprove(disprovingPlayer, suggestion).GetConstraints(nodes.Where((n, i) => i == 0)).Count();
		}
	}
}
