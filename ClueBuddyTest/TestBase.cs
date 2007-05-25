using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	public class TestBase {

		private TestContext testContextInstance;

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		protected Player[] players;

		protected Game StartPresetGame() {
			Game g = PreparePresetGame();
			g.Start();
			return g;
		}

		protected Game PreparePresetGame() {
			Game g = Game.GreatDetective;
			g.Players.AddRange(players);
			g.AssignApproximatePlayerHandSizes();
			return g;
		}


		[TestInitialize]
		public virtual void Setup() {
			players = new Player[] { 
							   new Player("Player 1"), 
							   new Player("Player 2"), 
							   new Player("Player 3"), 
							   new Player("Player 4") 
						   };
		}
		[TestCleanup]
		public virtual void Cleanup() { }
	}
}
