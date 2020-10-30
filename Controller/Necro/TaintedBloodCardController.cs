﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace SotMWorkshop.Controller.Necro
{
	public class TaintedBloodCardController : CardController
	{
		public TaintedBloodCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}


		public override void AddTriggers()
		{
			//At the end of your draw phase, Necro deals the undead target with the lowest HP 2 irreducible toxic damage.
			base.AddTrigger<PhaseChangeAction>((PhaseChangeAction pca) => pca.FromPhase.Phase == Phase.DrawCard && pca.GameController.ActiveTurnTaker == base.HeroTurnTaker, new Func<PhaseChangeAction, IEnumerator>(this.DealDamageResponse), TriggerType.DealDamage, TriggerTiming.After, ActionDescription.Unspecified, false, true, null, false, null, null, false, false);
		}

		private IEnumerator DealDamageResponse(PhaseChangeAction pca)
		{
			//Necro deals the undead target with the lowest HP 2 irreducible toxic damage.
			IEnumerator coroutine = base.DealDamageToLowestHP(base.CharacterCard, 1, (Card c) => this.IsUndead(c), (Card c) => new int?(2), DamageType.Toxic, true, false, null, 1, null, null, false);
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

		private bool IsUndead(Card card)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "undead", false, false);
		}
	}
}
