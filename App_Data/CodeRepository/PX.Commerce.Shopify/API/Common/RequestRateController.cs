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
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using RestSharp;

namespace PX.Commerce.Shopify.API
{
	/// <summary>
	/// The request controller for Shopify API call, it will use the API call limit data from the Shopify store, allow or prohibit the API request to Shopify store.
	/// For restful API request, if the current capacity is available, it will allow the API request to access Shopify store; Otherwise controller will put the request on hold status until capacity is available
	/// For graphQL API request, it will allow the request if query cost is less than current available cost points, otherwise controller will put the request on hold status until cost points are available
	/// The capacity or cost point will be auto refilled, the rate is different based on the Shopify store plan.
	/// </summary>
	public class RequestRateController
	{
		private const int MaxQueryCostPoints = 1000; //The max cost points of each graphQL query
		private const int DefaultCallCapacity = 40;
		private const int Ratio = 25; //The ratio between Rest API call capacity and GraphQL cost.
		private int TotalCallCapacity = DefaultCallCapacity;
		private const int DelayTime = 505; //Default time to release API call limit, unit is ms.
		private const int MaxGapOfCallLimit = 5; //Allow the maximum gap between the rest call limit from Shopify and the rest call limit in the Controller
		private int LeakyRate = 1;
		private static Timer RestoreTimer = new Timer(_ => DripAll(), null, DelayTime, DelayTime); //Auto call back to release API call limit.
		private static ConcurrentBag<RequestRateController> AllLeakyControllers = new ConcurrentBag<RequestRateController>();
		private SemaphoreSlim CallController;

		public RequestRateController()
		{
			CallController = new SemaphoreSlim(DefaultCallCapacity - 1, 1000);
			AllLeakyControllers.Add(this);
		}

		/// <summary>
		/// For Rest API
		/// </summary>
		public void GrantAccess() => CallController.Wait();

		/// <summary>
		/// For GraphQL
		/// </summary>
		/// <param name="queryCost">The estimated cost of query</param>
		public async Task GrantAccessAsync(int queryCost)
		{
			if (queryCost > MaxQueryCostPoints) throw new PXException(GraphQLMessages.GraphQLCostExceed);

			//if the current cost less than queryCost, should wait for system refills and then check again.
			while (CallController.CurrentCount * Ratio < queryCost)
			{
				await Task.Delay(DelayTime);
			}
			await CallController.WaitAsync();
		}

		/// <summary>
		/// Update the API call limit data from Shopify REST API header data
		/// </summary>
		/// <param name="restCallInfo">The API call data from header, it's a string as "10/40" format</param>
		public void UpdateController(object restCallInfo)
		{
			if (restCallInfo != null)
			{
				var numStr = restCallInfo.ToString().Split('/');
				if (numStr != null && numStr.Length == 2)
				{
					if(int.TryParse(numStr[0], out var usedCalls) && int.TryParse(numStr[1], out var totalCalls))
					{
						//If the total call capacity from store is different with initialized data, we should reset it as the store value.
						if (totalCalls > TotalCallCapacity)
						{
							lock (CallController)
							{
								CallController.Release(totalCalls - this.TotalCallCapacity - 1);
								TotalCallCapacity = totalCalls;
								LeakyRate = totalCalls / DefaultCallCapacity;
							}
						}

						if (CallController.CurrentCount < ((this.TotalCallCapacity - usedCalls) - MaxGapOfCallLimit))
						{
							CallController.Release((this.TotalCallCapacity - usedCalls) - MaxGapOfCallLimit - CallController.CurrentCount);
						}
						else while (CallController.CurrentCount > (this.TotalCallCapacity - usedCalls))
						{
							CallController.Wait();
						}
					}
				}
			}
		}

		/// <summary>
		/// Update the API call limit data from Shopify GraphQL API response data
		/// </summary>
		/// <param name="cost">The rest cost info from GraphQL API response data</param>
		public async Task UpdateControllerAsync(CostExtension cost)
		{
			if (cost != null && cost.ThrottleStatus != null)
			{
				ThrottleStatus costStatus = cost.ThrottleStatus;
				int totalCalls = costStatus.MaximumAvailable.HasValue? (int)(costStatus.MaximumAvailable.Value/Ratio) : this.TotalCallCapacity;
				int restCalls = costStatus.CurrentlyAvailable.HasValue ? (int)(costStatus.CurrentlyAvailable.Value / Ratio) : 0;

				if (totalCalls > TotalCallCapacity)
				{
					lock (CallController)
					{
						CallController.Release(totalCalls - this.TotalCallCapacity - 1);
						TotalCallCapacity = totalCalls;
						LeakyRate = totalCalls / DefaultCallCapacity;
					}
				}

				if(CallController.CurrentCount < (restCalls - MaxGapOfCallLimit))
				{
					CallController.Release(restCalls - MaxGapOfCallLimit - CallController.CurrentCount);
				}
				else while (CallController.CurrentCount > restCalls)
				{
					await CallController.WaitAsync();
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
