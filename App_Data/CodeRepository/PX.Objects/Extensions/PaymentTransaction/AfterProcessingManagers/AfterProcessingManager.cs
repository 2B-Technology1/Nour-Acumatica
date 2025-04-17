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
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public abstract class AfterProcessingManager
	{
		public bool IsMassProcess { get; set; }

		public AfterProcessingManager()
		{

		}

		public virtual void RunAuthorizeActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCaptureActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunPriorAuthorizedCaptureActions(IBqlTable table, bool success)
		{ 
		}

		public virtual void RunVoidActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCreditActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCaptureOnlyActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunUnknownActions(IBqlTable table, bool success)
		{
		}

		public virtual bool CheckDocStateConsistency(IBqlTable table)
		{
			return true;
		}

		public abstract PXGraph GetGraph();

		public abstract void PersistData();
	}
}
