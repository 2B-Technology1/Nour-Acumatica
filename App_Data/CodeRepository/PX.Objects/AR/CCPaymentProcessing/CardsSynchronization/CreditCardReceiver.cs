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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class CreditCardReceiver
	{ 
		public string CustomerProfileId { get; private set; }

		public List<CreditCardData> Result { get; private set; }

		public int AttempsCnt { get;set; } = 1;

		ICCProfileProcessor profileProcessor;

		public Func<CreditCardData, bool> ProcessFilter { get; set; }

		public CreditCardReceiver(ICCProfileProcessor profileProcessor,string customerProfileId)
		{
			this.profileProcessor = profileProcessor;
			this.CustomerProfileId = customerProfileId;
		}

		public void DoAction()
		{
			try
			{  
				AttempsCnt --;
				IEnumerable<CreditCardData> paymentsProfiles = profileProcessor.GetAllPaymentProfiles(CustomerProfileId);
				Result = new List<CreditCardData>();

				foreach (CreditCardData item in paymentsProfiles)
				{
					if (ProcessFilter != null)
					{
						if (ProcessFilter(item))
						{
							Result.Add(item);
						}
					}
					else
					{
						Result.Add(item);
					}
				}
			}
			catch(CCProcessingException)
			{

				if (AttempsCnt > 0)
				{
					Thread.Sleep(100);
					DoAction();
				}
				else
				{
					throw;
				}
			}
		}
	}
}
