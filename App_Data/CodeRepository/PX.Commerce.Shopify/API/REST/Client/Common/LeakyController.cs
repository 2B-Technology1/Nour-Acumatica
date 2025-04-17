/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PX.Commerce.Shopify.API.REST
{
	public class LeakyController
	{
		private const string HEADER_LIMIT = "X-Shopify-Shop-Api-Call-Limit";
		private const int DefaultCallCapacity = 40;
		private int TotalCallCapacity = DefaultCallCapacity;
		private static readonly int DelayTime = 505; //Default time to release API call limit. 
		private int LeakyRate = 1;
		private readonly Object obj = new Object();
		private static Timer RestoreTimer = new Timer(_ => DripAll(), null, DelayTime, DelayTime); //Auto call back to release API call limit.
		private static ConcurrentBag<LeakyController> AllLeakyControllers = new ConcurrentBag<LeakyController>();
		private SemaphoreSlim CallController;

		public LeakyController()
		{
			CallController = new SemaphoreSlim(DefaultCallCapacity - 1, 1000);
			AllLeakyControllers.Add(this);
		}
		public void GrantAccess() => CallController.Wait();

		public void UpdateController(IList<HeaderParameter> header)
		{
			var paraLimit = header?.FirstOrDefault(x => string.Equals(x.Name, HEADER_LIMIT, StringComparison.OrdinalIgnoreCase));
			if (paraLimit != null && paraLimit.Value != null)
			{
				var numStr = paraLimit.Value.ToString().Split('/');
				if (numStr != null && numStr.Length == 2)
				{
					var usedCalls = int.Parse(numStr[0]);
					var totalCalls = int.Parse(numStr[1]);
					if (totalCalls > TotalCallCapacity)
					{
						lock (CallController)
						{
							CallController.Release(totalCalls - this.TotalCallCapacity - 1);
							TotalCallCapacity = totalCalls;
							LeakyRate = totalCalls / DefaultCallCapacity;
						}
					}

					while (CallController.CurrentCount > (this.TotalCallCapacity - usedCalls))
					{
						CallController.Wait();
					}
				}
			}
		}

		private void RestoreCall()
		{
			if (CallController.CurrentCount < (TotalCallCapacity - 1))
			{
				int waitings = TotalCallCapacity - CallController.CurrentCount - 1;
				CallController.Release(Math.Min(LeakyRate, waitings));
			}
		}

		/// <summary>
		/// Release API call limit based on the leaky rate.
		/// </summary>
		private static void DripAll()
		{
			foreach(var controller in AllLeakyControllers)
			{
				controller.RestoreCall();
			}
		}
	}
}
