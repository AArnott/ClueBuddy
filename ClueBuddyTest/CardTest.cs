using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass()]
	public class CardTest : TestBase {
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorNullNameTest() {
			new Place(null);
		}

		[TestMethod]
		public void EqualsTest() {
			Card c = new Suspect("test");
			Assert.IsTrue(c.Equals(c));
		}

		[TestMethod]
		public void ToStringTest() {
			Card c = new Suspect("testSuspect");
			Assert.AreEqual(c.Name, c.ToString());
		}

		[TestMethod]
		public void LocationConstructorTest() {
			const string name = "Test";
			Card c = new Place(name);
			Assert.AreEqual(name, c.Name);
		}

		[TestMethod]
		public void WeaponConstructorTest() {
			const string name = "Test";
			Card c = new Weapon(name);
			Assert.AreEqual(name, c.Name);
		}

		[TestMethod]
		public void SuspectConstructorTest() {
			const string name = "Test";
			Card c = new Suspect(name);
			Assert.AreEqual(name, c.Name);
		}
	}
}
