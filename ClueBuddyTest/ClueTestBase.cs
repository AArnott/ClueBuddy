using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClueBuddy;

namespace ClueBuddyTest {
	public class ClueTestBase : TestBase {
		protected Player disprovingPlayer = new Player("opponent");
		protected Suspicion suggestion = new Suspicion(new Suspect("test"), new Weapon("test"), new Location("test"));
		protected Card cardShown;
		internal List<Node> nodes = new List<Node>(3);

		public override void Setup() {
			cardShown = suggestion.Location;
			nodes.Clear();
			nodes.AddRange(from c in suggestion.Cards
						   select new Node(disprovingPlayer, c));
		}

	}
}
