//-----------------------------------------------------------------------
// <copyright file="CardTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddyTest {
	using System;
	using ClueBuddy;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass()]
	public class CardTest : TestBase {
		[TestInitialize]
		public override void Setup() {
			base.Setup();
		}

		[TestMethod]
		public void EqualsTest() {
			Card c = new Suspect("test");
			Assert.IsTrue(c.Equals(c));
		}

		[TestMethod]
		public void ToStringTest() {
			Card c = new Suspect("testSuspect");
			Assert.AreEqual(c.Name, c.ToString());
		}

		[TestMethod]
		public void LocationConstructorTest() {
			const string name = "Test";
			Card c = new Place(name);
			Assert.AreEqual(name, c.Name);
		}

		[TestMethod]
		public void WeaponConstructorTest() {
			const string name = "Test";
			Card c = new Weapon(name);
			Assert.AreEqual(name, c.Name);
		}

		[TestMethod]
		public void SuspectConstructorTest() {
			const string name = "Test";
			Card c = new Suspect(name);
			Assert.AreEqual(name, c.Name);
		}
	}
}
