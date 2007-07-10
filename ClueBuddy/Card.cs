using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ClueBuddy {
	[Serializable]
	public class Card {
		public Card() { }
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
		[XmlAttribute]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override string ToString() {
			return Name;
		}
	}
	[Serializable]
	public class Weapon : Card {
		public Weapon() { }
		public Weapon(string name) : base(name) { }

		public static IEnumerable<Weapon> Generate(params string[] names) {
			return from name in names select new Weapon(name);
		}
	}
	[Serializable]
	public class Place : Card {
		public Place() { }
		public Place(string name) : base(name) { }

		public static IEnumerable<Place> Generate(params string[] names) {
			return from name in names select new Place(name);
		}
	}
	[Serializable]
	public class Suspect : Card {
		public Suspect() { }
		public Suspect(string name) : base(name) { }

		StandardSuspect wellKnownSuspect = StandardSuspect.Other;
		[XmlAttribute]
		public StandardSuspect WellKnownSuspect {
			get {
				if (wellKnownSuspect == StandardSuspect.Other && Name != null) {
					foreach (string standardSuspect in Enum.GetNames(typeof(StandardSuspect))) {
						if (Name.IndexOf(standardSuspect) >= 0) {
							wellKnownSuspect = (StandardSuspect)Enum.Parse(typeof(StandardSuspect), standardSuspect);
							break;
						}
					}
				}
				return wellKnownSuspect;
			}
			set { wellKnownSuspect = value; }
		}

		SuspectGender gender = SuspectGender.Undetermined;
		[XmlAttribute]
		public SuspectGender Gender {
			get {
				if (gender == SuspectGender.Undetermined) {
					switch (WellKnownSuspect) {
						case StandardSuspect.Brunette:
						case StandardSuspect.Green:
						case StandardSuspect.Grey:
						case StandardSuspect.Mustard:
						case StandardSuspect.Plum:
							gender = SuspectGender.Male;
							break;
						case StandardSuspect.Peach:
						case StandardSuspect.Peacock:
						case StandardSuspect.Rose:
						case StandardSuspect.Scarlet:
						case StandardSuspect.White:
							gender = SuspectGender.Female;
							break;
					}
				}
				return gender;
			}
			set { gender = value; }
		}

		public static IEnumerable<Suspect> Generate(params string[] names) {
			return from name in names select new Suspect(name);
		}

		public enum SuspectGender {
			Undetermined,
			Male,
			Female
		}

		public enum StandardSuspect {
			Other,
			Peach,
			Rose,
			Scarlet,
			White,
			Peacock,
			Green,
			Plum,
			Mustard,
			Brunette,
			Grey,
		}
	}
}
