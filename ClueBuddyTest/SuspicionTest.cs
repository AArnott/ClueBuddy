namespace ClueBuddyTest {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class SuspicionTest : TestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod]
		public void CardsTest() {
			var w = new Weapon("weapon");
			var s = new Suspect("suspect");
			var l = new Place("location");
			Suspicion target = new Suspicion(s, w, l);
			List<Card> cards = target.Cards.ToList();
			CollectionAssert.Contains(cards, w);
			CollectionAssert.Contains(cards, s);
			CollectionAssert.Contains(cards, l);
			Assert.AreEqual(3, cards.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SuspicionConstructorNullSuspectTest() {
			new Suspicion(null, new Weapon("weapon"), new Place("location"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SuspicionConstructorNullWeaponTest() {
			new Suspicion(new Suspect("suspect"), null, new Place("location"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SuspicionConstructorNullLocationTest() {
			new Suspicion(new Suspect("suspect"), new Weapon("weapon"), null);
		}
	}
}
