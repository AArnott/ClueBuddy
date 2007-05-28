using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	[TestClass]
	public class CaseFileTest : TestBase {
		[TestMethod]
		public void ToStringTest() {
			Game game = StartPresetGame();
			CaseFile target = game.CaseFile;
			Assert.AreEqual("CaseFile", target.ToString());
		}

		[TestMethod]
		public void WeaponTest() {
			Game game = StartPresetGame();
			CaseFile target = game.CaseFile;
			Assert.IsNull(target.Weapon);
			var c = game.Weapons.First();
			game.Nodes.Where(n => n.Card == c && n.CardHolder == target).First().IsSelected = true;
			Assert.AreSame(c, target.Weapon);
		}

		[TestMethod]
		public void SuspectTest() {
			Game game = StartPresetGame();
			CaseFile target = game.CaseFile;
			Assert.IsNull(target.Suspect);
			var c = game.Suspects.First();
			game.Nodes.Where(n => n.Card == c && n.CardHolder == target).First().IsSelected = true;
			Assert.AreSame(c, target.Suspect);
		}

		[TestMethod]
		public void LocationTest() {
			Game game = StartPresetGame();
			CaseFile target = game.CaseFile;
			Assert.IsNull(target.Place);
			var c = game.Places.First();
			game.Nodes.Where(n => n.Card == c && n.CardHolder == target).First().IsSelected = true;
			Assert.AreSame(c, target.Place);
		}

		[TestMethod]
		public void CardsHeldCountTest() {
			Game game = StartPresetGame();
			CaseFile target = game.CaseFile;
			Assert.AreEqual(3, target.CardsHeldCount);
		}
	}
}
