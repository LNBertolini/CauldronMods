﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Anathema
{
	public class ThresherClawCardController : CardController
    {
		public ThresherClawCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			//At the end of the Villain Turn, Anathema deals the {H-2} heroes with the highest HP 3 toxic damage each.
			base.AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => c.IsHeroCharacterCard, TargetType.HighestHP, 3, DamageType.Toxic, false, false, 1, (base.H - 2), null, null);
		}



		public override IEnumerator Play()
		{
			if (GetNumberOfArmsInPlay() > 2)
			{
				//Determine the arm with the highest HP
				List<Card> highestArm = new List<Card>();
				IEnumerator coroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => this.IsArm(c) && c.IsInPlay && c != base.Card, highestArm, null, null, false, false, null, false, base.GetCardSource(null));

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(coroutine);
				}
				else
				{
					base.GameController.ExhaustCoroutine(coroutine);
				}
				//Destroy all other arm cards except for the one with the highest HP.

				IEnumerator coroutine2 = base.GameController.DestroyCards(this.DecisionMaker, new LinqCardCriteria((Card c) => this.IsArm(c) && !highestArm.Contains(c) && c != base.Card, "arm", true, false, null, null, false), false, null, null, null, SelectionType.DestroyCard, base.GetCardSource(null));
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(coroutine2);
				}
				else
				{
					base.GameController.ExhaustCoroutine(coroutine2);
				}
			}

			yield break;
		}

		private bool IsArm(Card card)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "arm", false, false);
		}

		private int GetNumberOfArmsInPlay()
		{
			return base.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && this.IsArm(c), false, null, false).Count<Card>();
		}

	}
}
