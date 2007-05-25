using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class SuspicionTest : TestBase {
		[TestMethod]
		public void CardsTest() {
			var w = new Weapon("weapon");
			var s = new Suspect("suspect");
			var l = new Location("location");
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
			new Suspicion(null, new Weapon("weapon"), new Location("location"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SuspicionConstructorNullWeaponTest() {
			new Suspicion(new Suspect("suspect"), null, new Location("location"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SuspicionConstructorNullLocationTest() {
			new Suspicion(new Suspect("suspect"), new Weapon("weapon"), null);
		}
	}
}
