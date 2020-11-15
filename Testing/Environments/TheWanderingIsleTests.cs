﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    public class TheWanderingIsleTests : BaseTest
    {

        #region TheWanderingIsleHelperFunctions

        protected TurnTakerController isle { get { return FindEnvironment(); } }

        private bool IsTeryxInPlay(TurnTakerController ttc)
        {
            var cardsInPlay = ttc.TurnTaker.GetAllCards().Where(c => c.IsInPlay && this.IsTeryx(c));
            var numCardsInPlay = cardsInPlay.Count();

            return numCardsInPlay > 0;
        }
        private bool IsTeryx(Card card)
        {
            return card.Identifier == "Teryx";
        }

        #endregion

        [Test()]
        public void TestTheWanderingIsleWorks()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

        }


        [Test()]
        public void TestDecklist()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card card = GetCard("Teryx");
            AssertIsTarget(card, 50);
            AssertCardHasKeyword(card, "living island", false);

            card = GetCard("AncientParasite");
            AssertIsTarget(card, 20);
            AssertCardHasKeyword(card, "creature", false);

            card = GetCard("BarnacleHydra");
            AssertIsTarget(card, 6);
            AssertCardHasKeyword(card, "creature", false);

            card = GetCard("TimedDetonator");
            AssertIsTarget(card, 8);
            AssertCardHasKeyword(card, "device", false);

        }

        [Test()]
        public void TestTeryxIndestructible()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            Card teryx = PutIntoPlay("Teryx");
            AssertInPlayArea(env, teryx);

            //teryx is indestructible, so shouldn't be destroyed
            DestroyCard(teryx, baron.CharacterCard);
            AssertInPlayArea(env, teryx);

            //but not immune to damage
            QuickHPStorage(teryx);
            DealDamage(baron.CharacterCard, teryx, 3, DamageType.Cold);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTeryxGameOver()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            Card teryx = PutIntoPlay("Teryx");

            //If this card reaches 0HP, the heroes lose.
            DealDamage(baron.CharacterCard, teryx, 50, DamageType.Melee, true);

            AssertGameOver(EndingResult.EnvironmentDefeat);
        }

        [Test()]
        public void TestTeryxVillainDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            Card teryx = PutIntoPlay("Teryx");

            //At the end of the environment turn, the villain target with the highest HP deals Teryx {H + 2} energy damage.
            //H = 3, should be dealt 5 damage
            GoToStartOfTurn(isle);
            QuickHPStorage(teryx, baron.CharacterCard, ra.CharacterCard);
            GoToEndOfTurn(isle);
            QuickHPCheck(-5, 0, 0);
        }

        [Test()]
        public void TestTeryxHeroDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            Card teryx = PutIntoPlay("Teryx");

            //set hp lower so something to gain
            SetHitPoints(teryx, 30);

            //Whenever a hero target would deal damage to Teryx, Teryx Instead regains that much HP.
            QuickHPStorage(teryx, baron.CharacterCard, ra.CharacterCard);
            DealDamage(ra, teryx, 5, DamageType.Fire);
            QuickHPCheck(5, 0, 0);
        }

        [Test()]
        public void TestAmphibiousAssaultPlayWith2Villains()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            QuickHPStorage(baron, ra, fanatic, haka);

            //When this card enters play, the {H - 1} villain targets with the lowest HP each deal 3 lightning damage to a different hero target.
            //H = 3, so 2 villain targets hould deal damage
            //since we aren't selecting targets, it should default to ra, then fanatic being dealt damage
            var card = PutIntoPlay("AmphibiousAssault");
            AssertInPlayArea(isle, card);

            QuickHPCheck(0, -3, -3, 0);
        }

        [Test]
        public void TestAmphibiousAssaultPlayWith1Villains()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            //destroy mdp so there is only baron blade in play
            DestroyCard(GetCardInPlay("MobileDefensePlatform"));

            QuickHPStorage(baron, ra, fanatic, haka);

            //however, there is only 1 villain target in play, so only 1 instance of damage
            //since we aren't selecting targets, it should default to ra
            var card = PutIntoPlay("AmphibiousAssault");
            AssertInPlayArea(isle, card);

            QuickHPCheck(0, -3, 0, 0);
        }

        [Test()]
        public void TestAmphibiousAssaultPlayWithNotEnoughHeros()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //incap fanatic and haka so there's not enough hero targets
            DealDamage(baron.CharacterCard, fanatic.CharacterCard, 99, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, haka.CharacterCard, 99, DamageType.Cold, true);

            QuickHPStorage(baron, ra);

            //When this card enters play, the {H - 1} villain targets with the lowest HP each deal 3 lightning damage to a different hero target.
            //H = 3, so 2 villain targets hould deal damage
            //since we aren't selecting targets, it should default to ra, then fanatic being dealt damage
            var card = PutIntoPlay("AmphibiousAssault");
            AssertInPlayArea(isle, card);

            QuickHPCheck(0, -3);
        }

        [Test()]
        public void TestAmphibiousAssaultStartOfTurnHeroCardsPlayed()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack villain deck to not play hasten doom
            var topCard = PutOnDeck("MobileDefensePlatform");

            //don't play a hero card
            GoToPlayCardPhase(ra);

            GoToPlayCardPhase(haka);

            var card = PutIntoPlay("AmphibiousAssault");
            AssertInPlayArea(isle, card);
            AssertNotInPlay(topCard);

            //At the start of the environment turn, if any hero cards were played this round, play the top card of the villain deck. Then, destroy this card.
            GoToStartOfTurn(isle);

            AssertNotInPlay(topCard);
            AssertInTrash(card);
        }

        [Test()]
        public void TestAmphibiousAssaultStartOfTurnNoHeroCardsPlayed()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack villain deck to not play hasten doom
            var topCard = PutOnDeck("MobileDefensePlatform");

            //play a hero card
            GoToPlayCardPhase(ra);
            var random = GetCardFromHand(ra);
            PlayCard(random);
            GoToPlayCardPhase(haka);

            var card = PutIntoPlay("AmphibiousAssault");
            AssertInPlayArea(isle, card);
            AssertNotInPlay(topCard);

            //At the start of the environment turn, if any hero cards were played this round, play the top card of the villain deck. Then, destroy this card.
            GoToStartOfTurn(isle);

            AssertInPlayArea(baron, topCard);
            AssertInTrash(card);
        }

        [Test()]
        public void TestAncientParasiteHeroDamageMoveCard()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card parasite = PutIntoPlay("AncientParasite");
            AssertInPlayArea(isle, parasite);

            //Whenever this card is dealt damage by a hero target, move it next to that target.
            DealDamage(ra, parasite, 5, DamageType.Fire);
            AssertNextToCard(parasite, ra.CharacterCard);

            //still next too
            DealDamage(ra, parasite, 1, DamageType.Fire);
            AssertNextToCard(parasite, ra.CharacterCard);

            //different hero
            DealDamage(fanatic, parasite, 1, DamageType.Fire);
            AssertNextToCard(parasite, fanatic.CharacterCard);
        }

        [Test()]
        public void TestAncientParasiteVillainDamageDontMoveCard()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card parasite = PutIntoPlay("AncientParasite");
            AssertInPlayArea(isle, parasite);

            //Whenever this card is dealt damage by a hero target, move it next to that target.
            DealDamage(baron, parasite, 5, DamageType.Fire);
            //didn't move
            AssertInPlayArea(isle, parasite);
        }

        [Test()]
        public void TestAncientParasiteStartOfTurnNextToTarget()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card teryx = PutIntoPlay("Teryx");
            Card parasite = PutIntoPlay("AncientParasite");

            //move next to ra
            DealDamage(ra, parasite, 5, DamageType.Fire);
            AssertNextToCard(parasite, ra.CharacterCard);

            //At the start of the environment turn, if this card is next to a target, it deals that target {H} toxic damage and moves back to the environment play area. 
            //H is 3, so 3 damage should be dealt
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, haka.CharacterCard, parasite, teryx);

            GoToStartOfTurn(isle);

            QuickHPCheck(0, -3, 0, 0, 0, 0);
            AssertNotNextToCard(parasite, ra.CharacterCard);
            AssertInPlayArea(isle, parasite);
        }

        [Test()]
        public void TestAncientParasiteStartOfTurnNotNextToTarget()
        {
            SetupGameController("BaronBlade", "Ra", "Fanatic", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card teryx = PutIntoPlay("Teryx");
            Card parasite = PutIntoPlay("AncientParasite");
            AssertInPlayArea(isle, teryx);
            AssertInPlayArea(isle, parasite);

            //Otherwise it deals Teryx {H + 2} toxic damage.
            //H is 3, so 5 damage should be dealt
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, haka.CharacterCard, parasite, teryx);

            GoToStartOfTurn(isle);
            QuickHPCheck(0, 0, 0, 0, 0, -5);

            AssertInPlayArea(isle, teryx);
            AssertInPlayArea(isle, parasite);
        }

        [Test()]
        public void TestAncientParasiteCardNextToDestroyed()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card decoy = PutIntoPlay("DecoyProjection");
            Card parasite = PutIntoPlay("AncientParasite");

            DealDamage(decoy, parasite, 5, DamageType.Fire);
            AssertNextToCard(parasite, decoy);

            DestroyCard(decoy);

            AssertInPlayArea(visionary, parasite);
        }

        [Test()]
        public void TestExposedLifeforcePlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            QuickShuffleStorage(isle);
            // When this card enters play, search the environment deck and trash for Teryx and put it into play, then shuffle the deck.
            PutIntoPlay("ExposedLifeforce");

            //teryx should now be in play
            AssertIsInPlay("Teryx");

            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestExposedLifeforceIncreaseDamageVillain()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //Increase damage dealt by villain cards by 1.
            PutIntoPlay("ExposedLifeforce");

            QuickHPStorage(ra);
            DealDamage(baron, ra, 4, DamageType.Melee);

            //damge should be increase, so will be 5
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestExposedLifeforceDontIncreaseDamageHero()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //Increase damage dealt by villain cards by 1.
            PutIntoPlay("ExposedLifeforce");

            QuickHPStorage(haka);
            DealDamage(ra, haka, 4, DamageType.Fire);

            //damge should be not increase, so will be 4
            QuickHPCheck(-4);


        }

        [Test()]
        public void TestExposedLifeforceDestroyWhen10()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card teryx = PutIntoPlay("Teryx");

            //set hitpoints so there is room to gain
            SetHitPoints(teryx, 30);

            Card card = PutIntoPlay("ExposedLifeforce");
            AssertInPlayArea(isle, card);

            //Destroy this card if Teryx regains 10HP in a single round.
            GainHP(teryx, 10);

            AssertInTrash(card);
        }

        [Test()]
        public void TestExposedLifeforceNoDestroyWhenLessThan10()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card teryx = PutIntoPlay("Teryx");

            //set hitpoints so there is room to gain
            SetHitPoints(teryx, 30);

            Card card = PutIntoPlay("ExposedLifeforce");
            AssertInPlayArea(isle, card);

            //Destroy this card if Teryx regains 10HP in a single round.
            GainHP(teryx, 9);

            AssertInPlayArea(isle, card);
        }

        [Test()]
        public void TestBarnacleHydraDestroyedTeryxInPlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //put teryx into play
            Card teryx = PutIntoPlay("Teryx");
            AssertInPlayArea(isle, teryx);

            Card hydra = PutIntoPlay("BarnacleHydra");
            AssertInPlayArea(isle, hydra);

            //When this card is destroyed, it deals Teryx {H} toxic damage.
            //H=3, so 3 damage should be dealt
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard, teryx);
            DestroyCard(hydra, baron.CharacterCard);
            QuickHPCheck(0, 0, 0, 0, -3);
        }

        [Test()]
        public void TestBarnacleHydraDestroyedNoTeryxInPlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card hydra = PutIntoPlay("BarnacleHydra");
            AssertInPlayArea(isle, hydra);

            //When this card is destroyed, it deals Teryx {H} toxic damage.
            //teryx is not in play so no damage should be dealt, essentially checking for no crash
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard);
            DestroyCard(hydra, baron.CharacterCard);
            QuickHPCheck(0, 0, 0, 0);

            AssertNotGameOver();
        }

        [Test()]
        public void TestBarnacleHydraEndOfTurnNoSubmerge()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            Card hydra = PutIntoPlay("BarnacleHydra");

            //set hitpoints so have room to gain things
            //this also checks to make sure that damage being dealt ignores hydra
            SetHitPoints(hydra, 1);
            SetHitPoints(haka, 5); //ensure haka is the lowest target

            //At the end of the environment turn, this card deals the non-environment target with the lowest HP 2 projectile damage. Then, if Submerge is in play, this card regains 6HP
            //Submerge is not in play
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard, hydra);
            GoToEndOfTurn(isle);
            QuickHPCheck(0, 0, 0, -2, 0);
        }

        [Test()]
        public void TestBarnacleHydraEndOfTurnSubmerge()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            GoToStartOfTurn(isle);

            Card hydra = PutIntoPlay("BarnacleHydra");
            //submerge reduces all damage by 2
            PutIntoPlay("Submerge");

            //set hitpoints so have room to gain things
            //this also checks to make sure that damage being dealt ignores hydra
            SetHitPoints(hydra, 1);
            SetHitPoints(haka, 5); //ensure haka is the lowest target

            //At the end of the environment turn, this card deals the non-environment target with the lowest HP 2 projectile damage. Then, if Submerge is in play, this card regains 6HP
            //Submerge is in play
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard, hydra);
            GoToEndOfTurn(isle);
            QuickHPCheck(0, 0, 0, 0, 5);
            AssertIsAtMaxHP(hydra);
        }

        [Test()]
        public void TestIslandquakeDestroyed()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            PutIntoPlay("Teryx");
            Card teryx = GetCardInPlay("Teryx");

            //set hitpoints so there is room to gain
            SetHitPoints(teryx, 30);

            PutIntoPlay("Islandquake");

            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);

            //Then, this card is destroyed.
            GoToStartOfTurn(isle);

            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //this card should have destroyed itself
            Assert.AreEqual(numCardsInEnvironmentPlayBefore - 1, numCardsInEnvironmentPlayAfter, "Number of environment cards in play don't match");
        }

        [Test()]
        public void TestIslandquakeStartOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //destroy mdp so baron blade is not immune to damage
            DestroyCard(GetCardInPlay("MobileDefensePlatform"));

            PutIntoPlay("Teryx");
            Card teryx = GetCardInPlay("Teryx");

            //set hitpoints so there is room to gain
            SetHitPoints(teryx, 30);

            GoToPlayCardPhase(ra);
            //ra deals damage to teryx to cause hp gain and to be immune to islandquake damage
            DealDamage(ra.CharacterCard, teryx, 5, DamageType.Fire);

            PutIntoPlay("Islandquake");


            //At the start of the environment turn, this card deals each target other than Teryx 4 sonic damage. Hero targets which caused Teryx to regain HP since the end of the last environment turn are immune to this damage.
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard, teryx);
            GoToStartOfTurn(isle);
            QuickHPCheck(-4, 0, -4, -4, 0);
        }

        [Test()]
        public void TestSongOfTheDeepPlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck to reduce variance
            PutOnDeck("Teryx");

            //When this card enters play, play the top card of the environment deck.

            int numCardsInEnvironmentDeckBefore = GetNumberOfCardsInDeck(isle);
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            //Play song of the deep
            PutIntoPlay("SongOfTheDeep");

            int numCardsInEnvironmentDeckAfter = GetNumberOfCardsInDeck(isle);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);


            //should be 2 fewer cards in the deck, one for song of the deep, 1 for top card of the deck
            Assert.AreEqual(numCardsInEnvironmentDeckBefore - 2, numCardsInEnvironmentDeckAfter, "The number of cards in the environment deck don't match.");
            //should be 2 more cards in play, one for song of the deep, 1 for top card of deck
            Assert.AreEqual(numCardsInEnvironmentPlayBefore + 2, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play area don't match.");

        }

        [Test()]
        public void TestSongOfTheDeepStartOfTurnNoDestroy()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            PutOnDeck("AncientParasite");
            PutIntoPlay("Teryx");
            //Play song of the deep
            PutIntoPlay("SongOfTheDeep");

            //collect the appropriate values for all hands
            GoToEndOfTurn(haka);
            //At the start of the environment turn, if Teryx is in play, each player may draw a card. Then, if there are at least 2 creatures in play, destroy this card.
            QuickHandStorage(ra, visionary, haka);
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            //setting all players to draw a card
            DecisionYesNo = true;
            GoToStartOfTurn(isle);
            QuickHandCheck(1, 1, 1);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //since no creatures in play, should not be destroyed
            Assert.AreEqual(numCardsInEnvironmentPlayBefore, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play area don't match.");
        }

        [Test()]
        public void TestSongOfTheDeepStartOfTurnDestroyWithTeryx()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck for less variance
            PutOnDeck("BarnacleHydra");

            //Play song of the deep
            PutIntoPlay("SongOfTheDeep");
            PutIntoPlay("BarnacleHydra");
            PutIntoPlay("AncientParasite");
            PutIntoPlay("Teryx");

            //collect the appropriate values for all hands
            GoToEndOfTurn(haka);            //At the start of the environment turn, if Teryx is in play, each player may draw a card. Then, if there are at least 2 creatures in play, destroy this card.
            QuickHandStorage(ra, visionary, haka);
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            //setting all players to draw a card
            DecisionYesNo = true;
            GoToStartOfTurn(isle);
            QuickHandCheck(1, 1, 1);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //since 2 creatures in play, 1 destroyed
            Assert.AreEqual(numCardsInEnvironmentPlayBefore - 1, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play area don't match.");
        }

        [Test()]
        public void TestSongOfTheDeepStartOfTurnWithoutTeryx()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            PutOnDeck("AncientParasite");
            //Play song of the deep
            PutIntoPlay("SongOfTheDeep");
            PutIntoPlay("BarnacleHydra");
            PutIntoPlay("AncientParasite");

            //collect the appropriate values for all hands
            GoToEndOfTurn(haka);
            //At the start of the environment turn, if Teryx is in play, each player may draw a card. Then, if there are at least 2 creatures in play, destroy this card.
            QuickHandStorage(ra, visionary, haka);
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            //setting all players to draw a card
            DecisionYesNo = true;
            GoToStartOfTurn(isle);
            //since teryx is not in play, nothing will be drawn
            QuickHandCheck(0, 0, 0);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //since no creatures in play, should not be destroyed
            Assert.AreEqual(numCardsInEnvironmentPlayBefore, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play area don't match.");
        }

        [Test()]
        public void TestSubmergePlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            // When this card enters play, search the environment deck and trash for Teryx and put it into play, then shuffle the deck.
            PutIntoPlay("Submerge");

            //teryx should now be in play
            Assert.IsTrue(this.IsTeryxInPlay(isle), "Teryx is not in play");

        }

        [Test()]
        public void TestSubmergeReduceDamage()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            PutIntoPlay("Submerge");
            //Reduce all damage dealt by 2

            QuickHPStorage(ra);
            DealDamage(baron, ra, 5, DamageType.Lightning);
            //since 5 damage dealt - 2 = should be 3 less now
            QuickHPCheck(-3);

        }

        [Test()]
        public void TestSubmergeStartOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            PutIntoPlay("Submerge");
            //At the start of the environment turn, this card is destroyed.

            int numCardsInPlayBefore = GetNumberOfCardsInPlay(isle);
            GoToStartOfTurn(isle);
            int numCardsInPlayAfter = GetNumberOfCardsInPlay(isle);

            //Submerge should have destroyed itself so 1 fewer env cards in play
            Assert.AreEqual(numCardsInPlayBefore - 1, numCardsInPlayAfter, "The number of environment cards in play don't match");
        }
        [Test()]
        public void TestThroughTheHurricaneTargetIsPlayed()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            SetHitPoints(ra.CharacterCard, 20);
            SetHitPoints(visionary.CharacterCard, 18);
            SetHitPoints(haka.CharacterCard, 28);

            // Whenever a target enters play, this card deals { H - 1}lightning damage to the target with the third highest HP.
            // 3rd highest hp is ra
            //H =3, 3-1 = 2
            PutIntoPlay("ThroughTheHurricane");

            QuickHPStorage(ra);
            PlayCard("DecoyProjection");
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestThroughTheHurricaneStartOfTurnPlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck for less variance
            PutOnDeck("Teryx");
            PutOnDeck("AncientParasite");

            PutIntoPlay("ThroughTheHurricane");

            //At the start of the environment turn, you may play the top 2 cards of the environment deck. If you do, this card is destroyed.
            DecisionYesNo = true;
            int numCardsInEnvironmentDeckBefore = GetNumberOfCardsInDeck(isle);
            GoToStartOfTurn(isle);
            int numCardsInEnvironmentDeckAfter = GetNumberOfCardsInDeck(isle);

            //should be 2 cards played, so 2 fewer cards in deck
            Assert.AreEqual(numCardsInEnvironmentDeckBefore - 2, numCardsInEnvironmentDeckAfter, "The number of cards in the environment deck do not match.");
        }

        [Test()]
        public void TestThroughTheHurricaneStartOfTurnDestroy()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck for less variance
            PutOnDeck("Teryx");
            PutOnDeck("AncientParasite");

            PutIntoPlay("ThroughTheHurricane");

            //At the start of the environment turn, you may play the top 2 cards of the environment deck. If you do, this card is destroyed.
            DecisionYesNo = true;
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            GoToStartOfTurn(isle);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //should be 2 cards played, but this card destroyed, so 1 more
            Assert.AreEqual(numCardsInEnvironmentPlayBefore + 1, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play do not match.");
        }

        [Test()]
        public void TestTimedDetonatorPlay()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck to reduce variance
            PutOnDeck("Teryx");

            //When this card enters play, play the top card of the environment deck.

            int numCardsInEnvironmentDeckBefore = GetNumberOfCardsInDeck(isle);
            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            //Play timed detonator
            PutIntoPlay("TimedDetonator");

            int numCardsInEnvironmentDeckAfter = GetNumberOfCardsInDeck(isle);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);


            //should be 2 fewer cards in the deck, one for timed detonator, 1 for top card of the deck
            Assert.AreEqual(numCardsInEnvironmentDeckBefore - 2, numCardsInEnvironmentDeckAfter, "The number of cards in the environment deck don't match.");
            //should be 2 more cards in play, one for 1 for timed detonator, 1 for top card of deck
            Assert.AreEqual(numCardsInEnvironmentPlayBefore + 2, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play area don't match.");

        }
        [Test()]
        public void TestTimedDetonatorStartofTurnDamage()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();

            //stack deck for less variance
            PutOnDeck("BarnacleHydra");

            //play teryx
            PutIntoPlay("Teryx");
            Card teryx = GetCardInPlay("Teryx");

            //Play timed detonator
            PutIntoPlay("TimedDetonator");

            //pause before environment to collect effects
            GoToEndOfTurn(haka);

            //At the start of the environment turn, this card deals Teryx 10 fire damage and each hero target {H - 2} fire damage. Then, this card is destroyed.
            //H = 3, so H-2 = 1
            QuickHPStorage(teryx, ra.CharacterCard, visionary.CharacterCard, haka.CharacterCard);
            GoToStartOfTurn(isle);
            QuickHPCheck(-10, -1, -1, -1);
        }

        [Test()]
        public void TestTimedDetonatorStartOfTurnDestroy()
        {
            SetupGameController("BaronBlade", "Ra", "TheVisionary", "Haka", "Cauldron.TheWanderingIsle");
            StartGame();
            //stack deck for less variance
            PutOnDeck("BarnacleHydra");

            //play timed detonator
            PutIntoPlay("TimedDetonator");

            //At the start of the environment turn, this card deals Teryx 10 fire damage and each hero target {H - 2} fire damage. Then, this card is destroyed.

            int numCardsInEnvironmentPlayBefore = GetNumberOfCardsInPlay(isle);
            GoToStartOfTurn(isle);
            int numCardsInEnvironmentPlayAfter = GetNumberOfCardsInPlay(isle);

            //this card should be destroyed, so 1 less
            Assert.AreEqual(numCardsInEnvironmentPlayBefore - 1, numCardsInEnvironmentPlayAfter, "The number of cards in the environment play do not match.");
        }

    }
}
