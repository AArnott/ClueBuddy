using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest
{
	[TestClass]
	public class PlayerTest :TestBase{
		[TestMethod]
		public void PlayerConstructorTest() {
			string name = "name";
			Player target = new Player(name);
			Assert.AreEqual(name, target.Name);
			Assert.IsNull(target.Game);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PlayerConstructorNullNameTest() {
			new Player(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PlayerConstructorEmptyNameTest() {
			new Player(string.Empty);
		}

		[TestMethod]
		public void NameTest() {
			Player target = new Player("a");
			target.Name = "b";
			Assert.AreEqual("b", target.Name);
		}

		[TestMethod]
		public void CardsHeldCountTest() {
			Player target = players[0];
			Assert.AreEqual(0, target.CardsHeldCount);
			target.CardsHeldCount = 5;
			Assert.AreEqual(5, target.CardsHeldCount);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CardsHeldCountCannotSetDuringGameTest() {
			Game game = StartPresetGame();
			players[0].CardsHeldCount++;
		}

		[TestMethod]
		public void ToStringTest() {
			Assert.AreEqual(players[0].Name, players[0].ToString());
		}
	}
}
