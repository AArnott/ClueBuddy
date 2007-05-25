using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class Card {
		protected Card(string name) {
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this.Name = name;
		}

		public static IEnumerable<Card> Generate(IEnumerable<Suspect> suspects, IEnumerable<Weapon> weapons, IEnumerable<Location> locations) {
			foreach (Card c in suspects) yield return c;
			foreach (Card c in weapons) yield return c;
			foreach (Card c in locations) yield return c;
		}

		private string name;
		/// <summary>
		/// The name of the weapon, location or suspect.
		/// </summary>
		public string Name {
			get { return name; }
			private set { name = value; }
		}

		public override string ToString() {
			return Name;
		}
	}
	public class Weapon : Card {
		public Weapon(string name) : base(name) { }

		public static IEnumerable<Weapon> Generate(params string[] names) {
			return from name in names select new Weapon(name);
		}
	}
	public class Location : Card {
		public Location(string name) : base(name) { }

		public static IEnumerable<Location> Generate(params string[] names) {
			return from name in names select new Location(name);
		}
	}
	public class Suspect : Card {
		public Suspect(string name) : base(name) { }

		public static IEnumerable<Suspect> Generate(params string[] names) {
			return from name in names select new Suspect(name);
		}
	}
}
