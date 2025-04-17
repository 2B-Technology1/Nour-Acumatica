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
using PX.Common;
using PX.Data;
using PX.SM;


namespace SP.Objects.SP
{
	public class SPFeedback : PXGraphExtension<KBFeedbackMaint>
	{
		public override void Initialize()
		{
			Base.Actions["Submit"].SetVisible(true);
			Base.Actions["Close"].SetVisible(false);
		}
		
		[PXDBString()]
		[FeedBackIsFind()]
		[PXUIField(DisplayName = "Did you find what you were looking for")]
		protected virtual void KBFeedback_IsFind_CacheAttached(PXCache sender)
		{
		}

		[PXDBString()]
		[FeedBackSatisfaction()]
		[PXUIField(DisplayName = "Please rate your overall satisfaction with Self-Service Portal")]
		protected virtual void KBFeedback_Satisfaction_CacheAttached(PXCache sender)
		{
		}
	}
}
