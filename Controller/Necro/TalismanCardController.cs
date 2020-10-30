﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SotMWorkshop.Controller.Necro
{
	public class TalismanCardController : CardController
	{
		public TalismanCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
		{
			//Place this card next to a target
			IEnumerator coroutine = base.SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget, "targets", false, false, null, null, false), storedResults, isPutIntoPlay, decisionSources);
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
		public override void AddTriggers()
		{
			//That target is immune to damage from undead targets.
			base.AddImmuneToDamageTrigger((DealDamageAction dd) => this.IsUndead(dd.DamageSource.Card) && base.IsThisCardNextToCard(dd.Target), false);
			base.AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger(null);
		}


		private bool IsUndead(Card card)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "undead", false, false);
		}
	}
}
