using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public partial class Game {
		public static Game GreatDetective {
			get {
				return new Game("Great Detective", new GameRules() {
									DisprovalEndsTurn = false
								}, Card.Generate(
									Suspect.Generate("Brunette", "Gray", "Green", "Mustard", "Peach", "Peacock", "Plum", "Rose", "Scarlet", "White"),
									Weapon.Generate("Knife", "Candlestick", "Horseshoe", "Lead pipe", "Poison", "Revolver", "Rope", "Wrench"),
									Place.Generate("Billiard room", "Carriage House", "Conservatory", "Courtyard", "Dining room", "Drawing room", "Fountain", "Gazebo", "Kitchen", "Library", "Studio", "Trophy room")
								)
				  );
			}
		}
		public static Game Simpsons {
			get {
				return new Game("Simpsons", new GameRules() {
									DisprovalEndsTurn = true
								}, Card.Generate(
									Suspect.Generate("Green", "Mustard", "Peacock", "Plum", "Scarlet", "White"),
									Weapon.Generate("Simpson house", "Frying dutchman", "Androids dungeon", "Burns manor", "Krusty loo studios", "Barneys Bowl o rama", "Kwik e mart", "Nuclear power plant", "Springfield retirement castle"),
									Place.Generate("Poison donut", "Plutonium rod", "Saxophone", "Slingshot", "Necklace", "Extend-o-glove")
								)
				);
			}
		}
	}
}
