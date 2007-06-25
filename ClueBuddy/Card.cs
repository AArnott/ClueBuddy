using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	[Serializable]
	public class Card {
		protected Card(string name) {
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this.Name = name;
		}

		public static IEnumerable<Card> Generate(IEnumerable<Suspect> suspects, IEnumerable<Weapon> weapons, IEnumerable<Place> places) {
			foreach (Card c in suspects) yield return c;
			foreach (Card c in weapons) yield return c;
			foreach (Card c in places) yield return c;
		}

		string name;
		/// <summary>
		/// The name of the weapon, place or suspect.
		/// </summary>
		public string Name {
			get { return name; }
			private set { name = value; }
		}

		public override string ToString() {
			return Name;
		}
	}
	[Serializable]
	public class Weapon : Card {
		public Weapon(string name) : base(name) { }

		public static IEnumerable<Weapon> Generate(params string[] names) {
			return from name in names select new Weapon(name);
		}
	}
	[Serializable]
	public class Place : Card {
		public Place(string name) : base(name) { }

		public static IEnumerable<Place> Generate(params string[] names) {
			return from name in names select new Place(name);
		}
	}
	[Serializable]
	public class Suspect : Card {
		public Suspect(string name) : base(name) { }

		public static IEnumerable<Suspect> Generate(params string[] names) {
			return from name in names select new Suspect(name);
		}
	}
}
