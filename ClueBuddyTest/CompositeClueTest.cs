using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class CompositeClueTest : TestBase {
		[TestMethod]
		public void GetConstraintsTest() {
			Game g = StartPresetGame();
			CompositeClue cc = new CompositeClue();
			cc.Player = g.Players[0];
			cc.Suspicion = new Suspicion(g.Suspects.First(), g.Weapons.First(), g.Places.First());
			Assert.AreEqual(0, cc.GetConstraints(g.Nodes).Count());
			cc.Responses[g.Players[1]].Disproved = false;
			var constraints = cc.GetConstraints(g.Nodes);
			Assert.AreEqual(1, constraints.Count());
			SelectionCountConstraint firstConstraint = (SelectionCountConstraint)constraints.First();
			Assert.IsTrue((!firstConstraint.SelectionState && firstConstraint.Min > 0) ||
				(firstConstraint.SelectionState && firstConstraint.Max == 0));
			cc.Responses[g.Players[2]].Disproved = true;
			// constraints = cc.GetConstraints(g.Nodes); // this is unnecessary since it's a delayed evaluation
			Assert.AreEqual(2, constraints.Count());
			foreach (SelectionCountConstraint constraint in constraints) {
				bool isDisprovingPlayer = constraint.Nodes.Any(n => ((Node)n).CardHolder == g.Players[2]);
				if (isDisprovingPlayer) {
					Assert.IsTrue(constraint.SelectionState && constraint.Min > 0);
				} else {
					Assert.IsTrue((!constraint.SelectionState && constraint.Min > 0) ||
						(constraint.SelectionState && constraint.Max == 0));
				}
			}
		}
	}
}
