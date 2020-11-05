﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Cauldron.Baccarat;

namespace CauldronTests
{
    [TestFixture()]
    public class BaccaratTests : BaseTest
    {
        #region BaccaratHelperFunctions
        protected HeroTurnTakerController baccarat { get { return FindHero("Baccarat"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(baccarat.CharacterCard, 1);
            DealDamage(villain, baccarat, 2, DamageType.Melee);
        }

        #endregion BaccaratHelperFunctions

        [Test()]
        public void TestLoadBaccarat()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baccarat);
            Assert.IsInstanceOf(typeof(BaccaratCharacterCardController), baccarat.CharacterCardController);

            Assert.AreEqual(27, baccarat.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestBaccaratInnatePowerOption1()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            //Discard the top card of your deck...
            GoToUsePowerPhase(baccarat);
            DecisionSelectFunction = 0;
            UsePower(baccarat.CharacterCard);
            Assert.AreEqual(1, GetNumberOfCardsInTrash(baccarat));
        }

        [Test()]
        public void TestBaccaratInnatePowerOption2Play2()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card hold1 = GetCard("UnderworldHoldEm", 1);
            Card hold2 = GetCard("UnderworldHoldEm", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { saint, hold1, hold2 };

            //...or put up to 2 trick cards with the same name from your trash into play.

            //In case any of these cards start in hand we want to count hand with them
            PutInHand(trashCards);
            QuickHandStorage(baccarat);
            //prep trash
            PutInTrash(trashCards);
            DecisionSelectFunction = 1;
            GoToUsePowerPhase(baccarat);

            //By discarding 3 cards then drawing 2 from the two Hold Em's net -1
            UsePower(baccarat.CharacterCard);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestBaccaratInnatePowerOption2Play0()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card hold1 = GetCard("UnderworldHoldEm", 1);
            Card hold2 = GetCard("UnderworldHoldEm", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { saint, hold1, hold2 };

            //...or put up to 2 trick cards with the same name from your trash into play.

            //In case any of these cards start in hand we want to count hand with them
            PutInHand(trashCards);
            QuickHandStorage(baccarat);
            //prep trash
            PutInTrash(trashCards);
            DecisionSelectFunction = 1;
            DecisionDoNotSelectCard = SelectionType.MoveCard;
            GoToUsePowerPhase(baccarat);

            //By discarding 3 cards then drawing 0 from the two Hold Em's net -3
            UsePower(baccarat.CharacterCard);
            QuickHandCheck(-3);
        }

        [Test()]
        public void TestBaccaratIncap1Hero()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Legacy", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);
            DiscardTopCards(legacy, 4);

            //Put 2 cards from a trash on the bottom of their deck.
            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 0);
            //Assert.AreEqual(2, GetNumberOfCardsInTrash(legacy));
            Assert.AreEqual(34, GetNumberOfCardsInDeck(legacy));
        }

        [Test()]
        public void TestBaccaratIncap1Villain()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Legacy", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);
            DiscardTopCards(baron, 4);

            //Put 2 cards from a trash on the bottom of their deck.
            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 0);
            //Assert.AreEqual(2, GetNumberOfCardsInTrash(baron));
            Assert.AreEqual(22, GetNumberOfCardsInDeck(baron));
        }

        [Test()]
        public void TestBaccaratIncap1Environment()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Legacy", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);
            DiscardTopCards(env, 4);

            //Put 2 cards from a trash on the bottom of their deck.
            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 0);
            Assert.AreEqual(2, GetNumberOfCardsInTrash(env));
            Assert.AreEqual(13, GetNumberOfCardsInDeck(env));
        }

        [Test()]
        public void TestBaccaratIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Legacy", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //Increase the next damage dealt by a hero target by 2.
            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 1);

            QuickHPStorage(mdp);
            DealDamage(legacy, mdp, 2, DamageType.Melee);
            QuickHPCheck(-4);

            QuickHPStorage(mdp);
            DealDamage(legacy, mdp, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBaccaratIncap3Yes()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);

            //Each hero character may deal themselves 3 toxic damage to use a power now.
            QuickHandStorage(bunker);
            QuickHPStorage(bunker);
            int scholarHP = GetHitPoints(scholar);

            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 2);

            Assert.AreEqual(scholarHP - 2, GetHitPoints(scholar));
            QuickHandCheck(1);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBaccaratIncap3No()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(baron);
            AssertIncapacitated(baccarat);

            DecisionDoNotSelectFunction = true;

            //Each hero character may deal themselves 3 toxic damage to use a power now.
            QuickHandStorage(bunker);
            QuickHPStorage(bunker);
            int scholarHP = GetHitPoints(scholar);

            GoToUseIncapacitatedAbilityPhase(baccarat);
            UseIncapacitatedAbility(baccarat, 2);

            Assert.AreEqual(scholarHP, GetHitPoints(scholar));
            QuickHandCheck(0);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestAbyssalSolitaireBeforeNextStart()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card abyssal = GetCard("AbyssalSolitaire");

            //Until the start of your next turn, reduce damage dealt to {Baccarat} by 1.
            QuickHPStorage(baccarat);
            GoToPlayCardPhase(baccarat);
            PlayCard(abyssal);
            GoToEndOfTurn(baccarat);
            DealDamage(baron, baccarat, 2, DamageType.Melee);
            QuickHPCheck(-1);
            PrintJournal();
        }

        [Test()]
        public void TestAbyssalSolitaireAfterNextStart()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card abyssal = GetCard("AbyssalSolitaire");

            QuickHPStorage(baccarat);

            //Until the start of your next turn, reduce damage dealt to {Baccarat} by 1.
            PlayCard(abyssal);
            GoToStartOfTurn(baccarat);
            DealDamage(baron, baccarat, 2, DamageType.Melee);

            QuickHPCheck(-2);

            PrintJournal();
        }

        [Test()]
        public void TestAceInTheHolePlayCard()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card ace = GetCard("AceInTheHole");
            Card saint = GetCard("AceOfSaints");
            PutInHand(baccarat, saint);
            DecisionSelectCard = saint;

            //You may play a card.
            QuickHandStorage(baccarat);
            PlayCard(ace);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestAceInTheHoleDontPlayCard()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card ace = GetCard("AceInTheHole");

            DecisionDoNotSelectCard = SelectionType.PlayCard;

            //You may play a card.
            QuickHandStorage(baccarat);
            PlayCard(ace);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestAceInTheHoleTwoPowerPhase()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card ace = GetCard("AceInTheHole");
            Card saint = GetCard("AceOfSaints");
            PutInHand(baccarat, saint);
            DecisionSelectCard = saint;

            //You may use {Baccarat}'s innate power twice during your phase this turn.
            GoToPlayCardPhase(baccarat);
            AssertNumberOfUsablePowers(baccarat, 1);
            PlayCard(ace);
            GoToUsePowerPhase(baccarat);
            UsePower(baccarat);
            AssertNumberOfUsablePowers(baccarat, 1);
            UsePower(baccarat);
            AssertNumberOfUsablePowers(baccarat, 0);
        }

        [Test()]
        public void TestAceOfSaintsReduceDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat","Bunker", "Megalopolis");
            StartGame();
            Card saint = GetCard("AceOfSaints");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //Reduce damage dealt to hero targets by 1.
            QuickHPStorage(baccarat);
            PlayCard(saint);
            DealDamage(baron, baccarat, 2, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(bunker);
            DealDamage(baron, bunker, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //not villain targetrs
            QuickHPStorage(mdp);
            DealDamage(baccarat, mdp, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAceOfSaintsDestroySelf()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card saint = GetCard("AceOfSaints");

            //...or this card is destroyed.
            GoToPlayCardPhase(baccarat);
            PlayCard(saint);
            AssertNumberOfCardsInPlay(baccarat, 2);
            GoToStartOfTurn(baccarat);
            AssertNumberOfCardsInPlay(baccarat, 1);
        }

        [Test()]
        public void TestAceOfSaintsShuffleSame2Cards()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            DiscardTopCards(baccarat, 35);
            Card saint = GetCard("AceOfSaints");
            PutInHand(saint);

            //At the start of your turn, shuffle 2 cards with the same name from your trash into your deck...
            GoToPlayCardPhase(baccarat);
            PlayCard(saint);
            AssertNumberOfCardsInPlay(baccarat, 2);
            int trash = baccarat.TurnTaker.Trash.NumberOfCards;
            GoToStartOfTurn(baccarat);
            AssertNumberOfCardsInPlay(baccarat, 2);
            Assert.AreEqual(trash - 2, baccarat.TurnTaker.Trash.NumberOfCards);
        }

        [Test()]
        public void TestAceOfSinnersIncreaseDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat","Bunker", "Megalopolis");
            StartGame();
            Card sinner = GetCard("AceOfSinners");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //Increase damage dealt by hero targets by 1.
            PlayCard(sinner);

            QuickHPStorage(mdp);
            DealDamage(baccarat, mdp, 2, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(mdp);
            DealDamage(bunker, mdp, 2, DamageType.Melee);
            QuickHPCheck(-3);

            //Not villain damage
            QuickHPStorage(baccarat);
            DealDamage(mdp, baccarat, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAceOfSinnersDestroySelf()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card sinner = GetCard("AceOfSinners");

            //...or this card is destroyed.
            GoToPlayCardPhase(baccarat);
            PlayCard(sinner);
            AssertNumberOfCardsInPlay(baccarat, 2);
            GoToStartOfTurn(baccarat);
            AssertNumberOfCardsInPlay(baccarat, 1);
        }

        [Test()]
        public void TestAceOfSinnersShuffleSame2Cards()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            DiscardTopCards(baccarat, 35);
            Card sinner = GetCard("AceOfSinners");
            PutInHand(sinner);

            //At the start of your turn, shuffle 2 cards with the same name from your trash into your deck...
            GoToPlayCardPhase(baccarat);
            PlayCard(sinner);
            AssertNumberOfCardsInPlay(baccarat, 2);
            int trash = baccarat.TurnTaker.Trash.NumberOfCards;
            GoToStartOfTurn(baccarat);
            AssertNumberOfCardsInPlay(baccarat, 2);
            Assert.AreEqual(trash - 2, baccarat.TurnTaker.Trash.NumberOfCards);
        }

        [Test()]
        public void TestAfterlifeEuchreIncreaseDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card euchre = GetCard("AfterlifeEuchre");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            //Increase the next damage dealt by {Baccarat} by 1,
            PlayCard(euchre);
            QuickHPStorage(mdp);
            DealDamage(baccarat, mdp, 2, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(mdp);
            DealDamage(baccarat, mdp, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAfterlifeEuchreDealDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card euchre = GetCard("AfterlifeEuchre");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectFunction = 1;
            DecisionSelectTarget = mdp;

            //{Baccarat} deals 1 target 2 toxic damage
            QuickHPStorage(mdp);
            PlayCard(euchre);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAllInDiscard()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Megalopolis");
            StartGame();
            Card allin = GetCard("AllIn");

            QuickHandStorage(baccarat);
            PlayCard(allin);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestAllInDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card allin = GetCard("AllIn");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card battalion = GetCard("BladeBattalion");

            PlayCard(battalion);
            //...{Baccarat} deals each non-hero target 1 infernal damage and 1 radiant damage.
            int bunkerHP = GetHitPoints(bunker);
            int mdpHP = GetHitPoints(mdp);
            QuickHPStorage(battalion);
            PlayCard(allin);
            QuickHPCheck(-2);
            Assert.AreEqual(mdpHP - 2, GetHitPoints(mdp));
            Assert.AreEqual(bunkerHP, GetHitPoints(bunker));
        }

        [Test()]
        public void TestBringDownTheHouseShufflePair()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint };
            PutInTrash(trashCards);

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertNumberOfCardsInTrash(baccarat, 2);
            Assert.IsTrue(baccarat.TurnTaker.Trash.HasCard(saint));
            Assert.IsTrue(baccarat.TurnTaker.Trash.HasCard(house));
        }

        [Test()]
        public void TestBringDownTheHouseDontShuffle()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            DecisionsYesNo = new bool[] { false };

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertNumberOfCardsInTrash(baccarat, 8);
        }

        [Test()]
        public void TestBringDownTheHouseShuffle3Pairs()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            DecisionsYesNo = new bool[]{ true, true, true };

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertNumberOfCardsInTrash(baccarat, 2);
            Assert.IsTrue(baccarat.TurnTaker.Trash.HasCard(saint));
            Assert.IsTrue(baccarat.TurnTaker.Trash.HasCard(house));
        }

        [Test()]
        public void TestBringDownTheHouseDestroy0Cards()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            //Setup Trash
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            //Setup In play
            Card field = GetCard("LivingForceField");
            PlayCard(field);

            DecisionsYesNo = new bool[] { true, true, true };
            DecisionDoNotSelectCard = SelectionType.DestroyCard;

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertIsInPlay(field);
        }

        [Test()]
        public void TestBringDownTheHouseDestroy1Ongoing()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            //Setup Trash
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            //Setup In play
            Card field = GetCard("LivingForceField");
            PlayCard(field);

            DecisionsYesNo = new bool[] { true, true, true };

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertInTrash(field);
        }

        [Test()]
        public void TestBringDownTheHouseDestroy1Environment()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            //Setup Trash
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            //Setup In play
            Card monorail = GetCard("PlummetingMonorail");
            PlayCard(monorail);

            DecisionsYesNo = new bool[] { true, true, true };

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertInTrash(monorail);
        }

        [Test()]
        public void TestBringDownTheHouseDestroy3Cards()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            //Setup Trash
            Card trick1 = GetCard("CheapTrick", 1);
            Card trick2 = GetCard("CheapTrick", 2);
            Card toss1 = GetCard("CardToss", 1);
            Card toss2 = GetCard("CardToss", 2);
            Card bridge1 = GetCard("GraveyardBridge", 1);
            Card bridge2 = GetCard("GraveyardBridge", 2);
            Card saint = GetCard("AceOfSaints");
            IEnumerable<Card> trashCards = new Card[] { trick1, trick2, saint, toss1, toss2, bridge1, bridge2 };
            PutInTrash(trashCards);

            //Setup In play
            Card field = GetCard("LivingForceField");
            Card monorail = GetCard("PlummetingMonorail");
            Card backlash = GetCard("BacklashField");
            Card police = GetCard("PoliceBackup");
            IEnumerable<Card> playCardsToDestroy = new Card[] { field, monorail, backlash };
            PlayCards(playCardsToDestroy);
            PlayCard(police);

            DecisionsYesNo = new bool[] { true, true, true };

            Card house = GetCard("BringDownTheHouse");
            //Shuffle any number of pairs of cards with the same name from your trash into your deck.
            PlayCard(house);
            AssertInTrash(field);
            AssertInTrash(monorail);
            AssertInTrash(backlash);
            AssertIsInPlay(police);
        }

        [Test()]
        public void TestCardTossDealDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card toss = GetCard("CardToss");
            Card saint = GetCard("AceOfSaints");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            PutInHand(saint);
            DecisionSelectCard = saint;
            DecisionSelectTarget = mdp;

            //{Baccarat} deals 1 target 1 projectile damage.
            QuickHPStorage(mdp);
            PlayCard(toss);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestCardTossPlayCard()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card toss = GetCard("CardToss");
            Card saint = GetCard("AceOfSaints");
            PutInHand(baccarat, saint);
            DecisionSelectCard = saint;

            //You may play a card.
            QuickHandStorage(baccarat);
            PlayCard(toss);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestCardTossDontPlayCard()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card toss = GetCard("CardToss");

            DecisionDoNotSelectCard = SelectionType.PlayCard;

            //You may play a card.
            QuickHandStorage(baccarat);
            PlayCard(toss);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestCheapTrick()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card cheap = GetCard("CheapTrick");
            Card abyssal = GetCard("AbyssalSolitaire");
            Card saint = GetCard("AceOfSaints");
            Card euchre = GetCard("AfterlifeEuchre");
            List<Card> list = new List<Card>() { abyssal, saint, euchre };
            int tricks = 0;
            int cheaps = 0;

            PutOnDeck(baccarat, list);
            PlayCard(cheap);
            //Discard the top card of your deck.
            //Reveal cards from the top of your deck until you reveal a trick. Shuffle the other cards back into your deck and put the trick into play.
            AssertNumberOfCardsInTrash(baccarat, 3);
            foreach(Card c in baccarat.TurnTaker.Trash.Cards)
            {
                if (c.DoKeywordsContain("trick"))
                {
                    tricks++;
                }
                else if (c.Identifier == "CheapTrick")
                {
                    cheaps++;
                }
            }
            Assert.IsTrue(cheaps == 1 && tricks == 2);
        }

        [Test()]
        public void TestGraveyardBridge()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            DiscardTopCards(baccarat.TurnTaker.Deck, 36);
            DiscardAllCards(baccarat);
            Card abyssal = GetCard("AbyssalSolitaire");
            Card bridge = GetCard("GraveyardBridge");
            PutInHand(bridge);

            GoToPlayCardPhase(baccarat);
            PlayCard(bridge);
            AssertNumberOfCardsInTrash(baccarat, 37);
            GoToStartOfTurn(baron);

            QuickHPStorage(baccarat);
            DealDamage(baron, baccarat, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestIFold()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card fold = GetCard("IFold");
            PutInHand(fold);
            PlayCard(fold);

            //Discard your hand and draw 3 cards.
            AssertNumberOfCardsInHand(baccarat, 3);
            AssertNumberOfCardsInTrash(baccarat, 5);
        }

        [Test()]
        public void TestUnderworldHoldEmSelfDraw()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card hold = GetCard("UnderworldHoldEm");
            PutInHand(hold);

            //One player may draw a card.
            QuickHandStorage(baccarat);
            PlayCard(hold);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestUnderworldHoldEmOtherDraw()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card hold = GetCard("UnderworldHoldEm");
            PutInHand(hold);
            DecisionSelectTurnTaker = bunker.TurnTaker;

            //One player may draw a card.
            QuickHandStorage(bunker);
            PlayCard(hold);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestUnderworldHoldEmDontDraw()
        {
            SetupGameController("BaronBlade", "Cauldron.Baccarat", "Bunker", "Megalopolis");
            StartGame();
            Card hold = GetCard("UnderworldHoldEm");
            PutInHand(hold);
            DecisionDoNotSelectTurnTaker = true;

            //One player may draw a card.
            int baccaratCards = baccarat.NumberOfCardsInHand;
            QuickHandStorage(bunker);
            PlayCard(hold);
            QuickHandCheck(0);
            Assert.AreEqual(baccaratCards, baccarat.NumberOfCardsInHand + 1);
        }
    }
}