using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class NodeTest : TestBase {
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NodeConstructorNullPlayerTest() {
			new Node(null, new Suspect("test"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NodeConstructorNullCardTest() {
			new Node(new Player("test"), null);
		}

		[TestMethod]
		public void NodeInitializationAndPropertyTest() {
			Player p = new Player("test");
			Card c = new Suspect("test");
			Node n = new Node(p, c);
			Assert.AreSame(c, n.Card);
			Assert.AreSame(p, n.CardHolder);
			Assert.IsFalse(n.IsSelected.HasValue, "A newly created node should not have a set IsSelected property.");
		}

		[TestMethod]
		public void IsSelectedSetToTrueTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			Assert.IsFalse(n.IsSelected.HasValue, "A newly created node should not have a set IsSelected property.");
			n.IsSelected = true;
			Assert.IsTrue(n.IsSelected.Value);
		}

		[TestMethod]
		public void IsSelectedSetToFalseTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			Assert.IsFalse(n.IsSelected.HasValue, "A newly created node should not have a set IsSelected property.");
			n.IsSelected = false;
			Assert.IsFalse(n.IsSelected.Value);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void IsSelectedCannotChangeKnownStateTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			Assert.IsFalse(n.IsSelected.HasValue, "A newly created node should not have a set IsSelected property.");
			n.IsSelected = false;
			n.IsSelected = true;
		}

		[TestMethod]
		public void IsSelectedCanRestateKnownStateTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			Assert.IsFalse(n.IsSelected.HasValue, "A newly created node should not have a set IsSelected property.");
			n.IsSelected = false;
			n.IsSelected = false;
		}

		[TestMethod]
		public void PropertyChangedTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			bool changed = false;
			n.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler
									   ((sender, args) => {
											Assert.AreEqual("IsSelected", args.PropertyName);
											changed = true;
										});
			n.IsSelected = false;
			Assert.IsTrue(changed);
		}

		[TestMethod]
		public void PropertyChangedDuringSimulationTest() {
			Node n = new Node(new Player("test"), new Suspect("test"));
			n.PushSimulation();
			bool changed = false;
			n.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler
									   ((sender, args) => {
											Assert.AreEqual("IsSelected", args.PropertyName);
											changed = true;
										});
			n.IsSelected = false;
			Assert.IsFalse(changed);
			n.PopSimulation();
			Assert.IsFalse(changed);
		}

		[TestMethod]
		public void ToStringTest() {
			Node n = new Node(new Player("testPlayer"), new Suspect("testSuspect"));
			Assert.AreEqual("(testPlayer, testSuspect) = ?", n.ToString());
			n.IsSelected = true;
			Assert.AreEqual("(testPlayer, testSuspect) = True", n.ToString());
		}
	}
}
