using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClueBuddy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClueBuddyTest {
	[TestClass]
	public class CompositeConstraintTest : TestBase {
		[TestMethod]
		public void IsSatisfiableTest() {
			// Design a set of five nodes: A B C D E, and constraints:
			//  * exactly one of A B C
			//  * exactly one of D E
			//  * at least one of A D
			//  * at least one of B E
			// We can deduce from this configuration that C can never be selected and leave the nodes in a solvable condition.
			INode a = new DummyNode("A"), b = new DummyNode("B"), c = new DummyNode("C"), d = new DummyNode("D"), e = new DummyNode("E");
			var constraints = new List<IConstraint>();
			constraints.Add(SelectionCountConstraint.ExactSelected(1, new INode[] { a, b, c }));
			constraints.Add(SelectionCountConstraint.ExactSelected(1, new INode[] { d, e }));
			constraints.Add(SelectionCountConstraint.MinSelected(1, new INode[] { a, d }));
			constraints.Add(SelectionCountConstraint.MinSelected(1, new INode[] { b, e }));
			CompositeConstraint cc = new CompositeConstraint(constraints);
			Debug.WriteLine("Testing with all nodes indeterminate");
			Assert.IsTrue(cc.IsSatisfiable);
			c.IsSelected = true;
			Debug.WriteLine("Testing with node C selected.");
			Assert.IsFalse(cc.IsSatisfiable);
		}
	}
}
