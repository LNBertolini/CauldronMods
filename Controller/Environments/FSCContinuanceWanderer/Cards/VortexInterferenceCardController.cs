﻿using System;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron
{
    public class VortexInterferenceCardController : CardController
    {
        #region Constructors

        public VortexInterferenceCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        #endregion Constructors

        #region Methods

        public override void AddTriggers()
        {
            //Whenever a hero uses a power, destroy 1 hero ongoing or equipment card.
            base.AddTrigger<UsePowerAction>((UsePowerAction p) => true, this.DestroyHeroOngoingOrEquipmentResponse, new TriggerType[] { TriggerType.DestroyCard }, TriggerTiming.After);
            //When another environment card enters play, destroy this card.
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction p) => p.CardEnteringPlay.IsEnvironment, (base.DestroyThisCardResponse), TriggerType.DestroySelf, TriggerTiming.After);
        }

        public IEnumerator DestroyHeroOngoingOrEquipmentResponse(UsePowerAction action)
        {
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || base.IsEquipment(c))), false, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
        #endregion Methods
    }
}