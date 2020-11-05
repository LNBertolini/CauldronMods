﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using Cauldron.LadyOfTheWood;
using System;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller.TheArgentAdept;

namespace CauldronTests
{
    [TestFixture()]
    public class LadyOfTheWoodTests : BaseTest
    {
        #region LadyOfTheWoodHelperFunctions
        protected HeroTurnTakerController ladyOfTheWood { get { return FindHero("LadyOfTheWood"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(ladyOfTheWood.CharacterCard, 1);
            DealDamage(villain, ladyOfTheWood, 2, DamageType.Melee);
        }

        protected void AssertNumberOfSeasonsInHand(TurnTakerController ttc, int number)
        {
            var cardsInHand = ttc.TurnTaker.GetAllCards().Where(c => c.IsInHand && this.IsSeason(c));
            var actual = cardsInHand.Count();
            Assert.AreEqual(number, actual, String.Format("{0} should have had {1} cards in hand, but actually had {2}: {3}", ttc.Name, number, actual, cardsInHand.Select(c => c.Title).ToCommaList()));
        }
        protected void AssertNumberOfSeasonsInPLay(TurnTakerController ttc, int number)
        {
            var cardsInPlay = ttc.TurnTaker.GetAllCards().Where(c => c.IsInPlay && this.IsSeason(c));
            var actual = cardsInPlay.Count();
            Assert.AreEqual(number, actual, String.Format("{0} should have had {1} cards in play, but actually had {2}: {3}", ttc.Name, number, actual, cardsInPlay.Select(c => c.Title).ToCommaList()));
        }


        protected int GetNumberOfSeasonsInHand(TurnTakerController ttc)
        {
            var cardsInHand = ttc.TurnTaker.GetAllCards().Where(c => c.IsInHand && this.IsSeason(c));
            var actual = cardsInHand.Count();
            return actual;
        }
        private bool IsSeason(Card card)
        {
            return card != null && base.GameController.DoesCardContainKeyword(card, "season", false, false);
        }
        #endregion

        [Test()]
        public void TestLadyOfTheWoodLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(ladyOfTheWood);
            Assert.IsInstanceOf(typeof(LadyOfTheWoodCharacterCardController), ladyOfTheWood.CharacterCardController);

            Assert.AreEqual(22, ladyOfTheWood.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestLadyOfTheWoodInnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Select a damage type. {LadyOfTheWood} deals 1 target 1 damage of the chosen type.
            GoToUsePowerPhase(ladyOfTheWood);
            DecisionSelectDamageType = DamageType.Cold;
            DecisionSelectTarget = ra.CharacterCard;
            QuickHPStorage(ra);
            UsePower(ladyOfTheWood.CharacterCard);
            QuickHPCheck(-1);

        }

        [Test()]
        public void TestLadyOfTheWoodIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(ladyOfTheWood);

            //Select a damage type. Until the start of your next turn, increase damage dealt of the chosen type by 1.
            GoToUseIncapacitatedAbilityPhase(ladyOfTheWood);

            DecisionSelectDamageType = DamageType.Fire;
            UseIncapacitatedAbility(ladyOfTheWood, 0);

            QuickHPStorage(haka);
            DealDamage(ra, haka, 5, DamageType.Fire);

            //Damage dealt should be 5 +1 => 6
            QuickHPCheck(-6);

            GoToStartOfTurn(ladyOfTheWood);

            QuickHPStorage(haka);
            DealDamage(ra, haka, 5, DamageType.Fire);

            //start of turn has passed, trigger should be gone, 5 damage
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestLadyOfTheWoodIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(ladyOfTheWood);

            //give hake room to gain health
            SetHitPoints(haka, 20);

            //1 hero may discard a card to regain 3 HP.
            GoToUseIncapacitatedAbilityPhase(ladyOfTheWood);
            DecisionSelectTurnTaker = haka.HeroTurnTaker;
            QuickHandStorage(haka);
            QuickHPStorage(haka);
            UseIncapacitatedAbility(ladyOfTheWood, 1);
            //should have 1 less card in hand and should have gained 3 HP
            QuickHandCheck(-1);
            QuickHPCheck(3);
            
        }

        [Test()]
        public void TestLadyOfTheWoodIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(baron);
            AssertIncapacitated(ladyOfTheWood);

            PutIntoPlay("PoliceBackup");

            //Destroy an environment card.
            GoToUseIncapacitatedAbilityPhase(ladyOfTheWood);

            int numEnvironmentCardsInPlayBefore = GetNumberOfCardsInPlay(FindEnvironment());
            UseIncapacitatedAbility(ladyOfTheWood, 2);
            int numEnvironmentCardsInPlayAfter = GetNumberOfCardsInPlay(FindEnvironment());

            //1 environment card should have been destroyed, so 1 less card in play
            Assert.AreEqual(numEnvironmentCardsInPlayBefore - 1, numEnvironmentCardsInPlayAfter, "The number of environment cards in play does not match");

        }

        [Test()]
        public void TestCalmBeforeTheStormDiscard3()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            PutInHand("CalmBeforeTheStorm");
            Card calm = GetCardFromHand("CalmBeforeTheStorm");
            //Discard any number of cards.
            // Increase the next damage dealt by LadyOfTheWood by X, where X is 2 plus the number of cards discarded this way.
            GoToPlayCardPhase(ladyOfTheWood);
            QuickHandStorage(ladyOfTheWood);

            PlayCard(calm);
            //should be -5, 1 for calm before storm, and 4 others discarded
            QuickHandCheck(-5);

            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 4, DamageType.Melee);
            //since 4 cards discarded, should be +6 for  10 damage
            QuickHPCheck(-10);

        }

        [Test()]
        public void TestCalmBeforeTheStormNoDiscard()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            PutInHand("CalmBeforeTheStorm");
            Card calm = GetCardFromHand("CalmBeforeTheStorm");
            //Discard any number of cards.
            // Increase the next damage dealt by LadyOfTheWood by X, where X is 2 plus the number of cards discarded this way.
            GoToPlayCardPhase(ladyOfTheWood);
            QuickHandStorage(ladyOfTheWood);
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            PlayCard(calm);
            //should be -1, 1 for calm before storm, and 0 others discarded
            QuickHandCheck(-1);

            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 4, DamageType.Melee);
            //since 0 cards discarded, should be +2 for  6 damage
            QuickHPCheck(-6);

        }

        [Test()]
        public void TestCrownOfTheFourWinds()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DestroyCard(mdp, haka.CharacterCard);
            PutInHand("CrownOfTheFourWinds");
            Card crown = GetCardFromHand("CrownOfTheFourWinds");
            GoToPlayCardPhase(ladyOfTheWood); ;
            PlayCard(crown);
            //LadyOfTheWood deals 1 target 1 toxic damage, a second target 1 fire damage, a third target 1 lightning damage, and a fourth target 1 cold damage.
            GoToUsePowerPhase(ladyOfTheWood);
            QuickHPStorage(baron, ladyOfTheWood, ra, haka);

            UsePower(crown);
            //every target in play should get 1 damage
            QuickHPCheck(-1, -1, -1, -1);

        }

        [Test()]
        public void TestEnchantedClearing()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Reduce damage dealt to LadyOfTheWood by 1.
            PlayCard("EnchantedClearing");

            QuickHPStorage(ladyOfTheWood);
            DealDamage(baron, ladyOfTheWood, 5, DamageType.Melee);

            //reduced by 1, so should be 4
            QuickHPCheck(-4);

        }

        [Test()]
        public void TestFallDealLightning()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Whenever LadyOfTheWood deals lightning damage to a target, reduce damage dealt by that target by 1 until the start of your next turn.
            GoToPlayCardPhase(ladyOfTheWood);
            PlayCard("Fall");
           
            DealDamage(ladyOfTheWood, haka, 8, DamageType.Lightning);

            QuickHPStorage(ra);
            DealDamage(haka, ra, 5, DamageType.Projectile);
            //reduced by 1, so should be 4
            QuickHPCheck(-4);

            GoToStartOfTurn(ladyOfTheWood);
            QuickHPStorage(ra);
            DealDamage(haka, ra, 5, DamageType.Projectile);
            //no longer reduced by 1, so should be 5
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestFallDealNotLightning()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Whenever LadyOfTheWood deals lightning damage to a target, reduce damage dealt by that target by 1 until the start of your next turn.
            GoToPlayCardPhase(ladyOfTheWood);
            PlayCard("Fall");

            DealDamage(ladyOfTheWood, haka, 8, DamageType.Fire);

            QuickHPStorage(ra);
            DealDamage(haka, ra, 5, DamageType.Projectile);
            //not lightning so not reduced by 1, so should be 5
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestFireInTheCloudsOption1()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //LadyOfTheWood deals 1 target 3 fire damage or up to 3 targets 1 lightning damage each.
            GoToPlayCardPhase(ladyOfTheWood);
            //should deal 1 target 3 fire
            DecisionSelectFunction = 0;
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);
            PlayCard("FireInTheClouds");
            QuickHPCheck(-3);
            
        }

        [Test()]
        public void TestFireInTheCloudsOption2()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DestroyCard(mdp, baron.CharacterCard);
            //LadyOfTheWood deals 1 target 3 fire damage or up to 3 targets 1 lightning damage each.
            GoToPlayCardPhase(ladyOfTheWood);
            //should deal 3 target 1 lightning
            DecisionSelectFunction = 1;
            QuickHPStorage(baron, ladyOfTheWood, ra);
            PlayCard("FireInTheClouds");
            QuickHPCheck(-1, -1, -1);

        }

        [Test()]
        public void TestFrostOnThePetalsOption1()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //LadyOfTheWood deals 1 target 3 toxic damage or up to 3 targets 1 cold damage each.
            GoToPlayCardPhase(ladyOfTheWood);
            //should deal 1 target 3 fire
            DecisionSelectFunction = 0;
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);
            PlayCard("FrostOnThePetals");
            QuickHPCheck(-3);

        }

        [Test()]
        public void TestFrostOnThePetalsOption2()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DestroyCard(mdp, baron.CharacterCard);
            //LadyOfTheWood deals 1 target 3 toxic damage or up to 3 targets 1 cold damage each.
            GoToPlayCardPhase(ladyOfTheWood);
            //should deal 3 target 1 lightning
            DecisionSelectFunction = 1;
            QuickHPStorage(baron, ladyOfTheWood, ra);
            PlayCard("FrostOnThePetals");
            QuickHPCheck(-1, -1, -1);

        }

        [Test()]
        public void TestMeadowRushDraw()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //stack deck
            Card spring = GetCard("Spring");
            PutOnDeck(ladyOfTheWood, FindCardController(spring));

            //Draw a card.
            PutInHand("MeadowRush");
            Card meadow = GetCardFromHand("MeadowRush");

            //choose to not play a card
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            PlayCard(meadow);
            //spring - which was stacked on top of deck, should be now in the hand
            AssertInHand(spring);

        }

        [Test()]
        public void TestMeadowRushSearch()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            PutInHand("MeadowRush");
            Card meadow = GetCardFromHand("MeadowRush");

            //stack deck to prevent drawing a season
            PutOnDeck("FireInTheClouds");

            int numSeasonsBefore = GetNumberOfSeasonsInHand(ladyOfTheWood);

            //Search your deck for a season card, put it into your hand, then shuffle your deck.
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            PlayCard(meadow);

            AssertNumberOfSeasonsInHand(ladyOfTheWood, numSeasonsBefore + 1);

        }

        [Test()]
        public void TestMeadowRushPlay()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Put all cards in hand back on the deck
            Card card;
            for (int i = 0; i < 4; i++)
            {
                card = GetCardFromHand(ladyOfTheWood, 0);
                PutOnDeck(ladyOfTheWood, FindCardController(card));
            }

            //put "Spring" on the top of the deck, a card that will stay in play
            PutOnDeck("Spring");

            PutInHand("MeadowRush");
            Card meadow = GetCardFromHand("MeadowRush");
            //You may play a card.

            PlayCard(meadow);

            //expect just two card in play, character card and played card
            AssertNumberOfCardsInPlay(ladyOfTheWood, 2);

        }

        [Test()]
        public void TestNobilityOfDuskIncreaseOnFirstDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Once per turn when LadyOfTheWood would deal damage you may increase that damage by 2.

            PlayCard("NobilityOfDusk");
            //use effect
            DecisionYesNo = true;
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should have increased by 2, so 7
            QuickHPCheck(-7);

            //try again, this time there should be no option to increase
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should have not been increased by 2, so 5
            QuickHPCheck(-5);

            //at next turn, should reset
            GoToNextTurn();
            DecisionYesNo = true;
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should have increased by 2, so 7
            QuickHPCheck(-7);



        }

        [Test()]
        public void TestNobilityOfDuskIncreaseOnSecondDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //Once per turn when LadyOfTheWood would deal damage you may increase that damage by 2.

            PlayCard("NobilityOfDusk");
            //use effect
            DecisionsYesNo = new bool[] { false, true, true };
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should not increase, so 5
            QuickHPCheck(-5);

            //try again, this time should be increased
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should have been increased by 2, so 7
            QuickHPCheck(-7);

            //at next turn, should reset
            GoToNextTurn();
            DecisionYesNo = true;
            QuickHPStorage(haka);
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Lightning);
            //damage should have increased by 2, so 7
            QuickHPCheck(-7);

        }

        [Test()]
        public void TestRebirthPutCardsUnder()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //stack trash
            PutInTrash("Spring");
            PutInTrash("Fall");
            PutInTrash("Summer");

            Card spring = GetCardFromTrash(ladyOfTheWood,"Spring");
            Card fall = GetCardFromTrash(ladyOfTheWood, "Fall");
            Card summer = GetCardFromTrash(ladyOfTheWood, "Summer");

            //When this card enters play, put up to 3 cards from your trash beneath it.

            DecisionSelectCards = new Card[] { spring, fall, summer };
            PlayCard("Rebirth");
            Card rebirth = GetCardInPlay("Rebirth");

            //check that there are 3 cards under and that they are the cards under are correct
            AssertNumberOfCardsUnderCard(rebirth, 3);
            AssertUnderCard(rebirth, spring);
            AssertUnderCard(rebirth, fall);
            AssertUnderCard(rebirth, summer);


        }

        [Test()]
        public void TestRebirthMoveCardToHand()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            //stack trash
            PutInTrash("Spring");
            PutInTrash("Fall");
            PutInTrash("Summer");

            Card spring = GetCardFromTrash(ladyOfTheWood, "Spring");
            Card fall = GetCardFromTrash(ladyOfTheWood, "Fall");
            Card summer = GetCardFromTrash(ladyOfTheWood, "Summer");

            //Whenever LadyOfTheWood destroys a target, put a card from beneath this one into your hand.
            DecisionSelectCards = new Card[] { spring, fall, summer, summer };
            PlayCard("Rebirth");
            Card rebirth = GetCardInPlay("Rebirth");

            QuickHandStorage(ladyOfTheWood);
            //have lady of the wood destroy mdp
            DestroyCard(mdp, ladyOfTheWood.CharacterCard);
            //summer should have been moved to hand
            QuickHandCheck(1);
            AssertNumberOfCardsUnderCard(rebirth, 2);
            AssertUnderCard(rebirth, spring);
            AssertUnderCard(rebirth, fall);
            AssertInHand(summer);


        }

        [Test()]
        public void TestRebirthDestroy()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //stack trash
            PutInTrash("Spring");
            PutInTrash("Fall");
            PutInTrash("Summer");

            Card spring = GetCardFromTrash(ladyOfTheWood, "Spring");
            Card fall = GetCardFromTrash(ladyOfTheWood, "Fall");
            Card summer = GetCardFromTrash(ladyOfTheWood, "Summer");

            //When there are no cards beneath this one, destroy this card.
            DecisionSelectCards = new Card[] { spring, fall, summer };
            PlayCard("Rebirth");
            Card rebirth = GetCardInPlay("Rebirth");

            
            MoveCards(ladyOfTheWood, new Card[] { spring, fall, summer }, ladyOfTheWood.HeroTurnTaker.Hand);
            AssertNotInPlay(rebirth);
            AssertInTrash(rebirth);
        }

        [Test()]
        public void TestSerenityOfDawnDestroyCard()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            //If LadyOfTheWood deals 3 or more damage to a target, destroy this card.
            PlayCard("SerenityOfDawn");
            Card serenity = GetCardInPlay("SerenityOfDawn");

            DealDamage(ladyOfTheWood, haka, 5, DamageType.Cold);

            //since more than 3 damage has been dealt, serenity should be destroyed
            AssertNotInPlay(serenity);
            AssertInTrash(serenity);

        }

        [Test()]
        public void TestSerenityOfDawnEndOfTurnHasDealtDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();
            //At the end of your turn, if {LadyOfTheWood} dealt no damage this turn, she regains 2 HP and you may draw a card.
            GoToPlayCardPhase(ladyOfTheWood);
            PlayCard("SerenityOfDawn");
            DealDamage(ladyOfTheWood, haka, 5, DamageType.Toxic);

            QuickHPStorage(ladyOfTheWood);
            GoToEndOfTurn(ladyOfTheWood);
            //since damage was dealt, no hp should be gained
            QuickHPCheckZero();
        }

        [Test()]
        public void TestSerenityOfDawnEndOfTurnHasNotDealtDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            //set hp to have room to gain later
            SetHitPoints(ladyOfTheWood.CharacterCard, 15);
            //At the end of your turn, if {LadyOfTheWood} dealt no damage this turn, she regains 2 HP and you may draw a card.
            PlayCard("SerenityOfDawn");
            QuickHPStorage(ladyOfTheWood);
            GoToEndOfTurn(ladyOfTheWood);
            //since no damage dealt, gain 2 HP
            QuickHPCheck(2);
        }

        [Test()]
        public void TestSnowshadeGown()
        {
            SetupGameController("BaronBlade", "Cauldron.LadyOfTheWood", "Ra", "Haka", "Megalopolis");
            StartGame();

            
        }
    }
}
