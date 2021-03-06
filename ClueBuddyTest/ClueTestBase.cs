﻿namespace ClueBuddyTest {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using ClueBuddy;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class ClueTestBase : TestBase {
		protected Player disprovingPlayer = new Player("opponent");
		protected Suspicion suggestion = new Suspicion(new Suspect("test"), new Weapon("test"), new Place("test"));
		protected Card cardShown;
		internal List<Node> nodes = new List<Node>(3);

		public override void Setup() {
			base.Setup();
			cardShown = suggestion.Place;
			nodes.Clear();
			nodes.AddRange(from c in suggestion.Cards
						   select new Node(disprovingPlayer, c));
		}

	}
}
