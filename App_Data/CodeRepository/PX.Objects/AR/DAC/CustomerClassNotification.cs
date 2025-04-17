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
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.SM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.RQ;

namespace PX.Objects.AR
{	
	public class CustomerContactType : NotificationContactType
	{
        /// <summary>
        /// Defines a list of the possible ContactType for the AR Customer <br/>
        /// Namely: Primary, Billing, Shipping, Employee <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>		
		public class ClassListAttribute : PXStringListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { Primary, Billing, Shipping, Employee },
							 new string[] { CR.Messages.AccountEmail, Messages.Billing, CR.Messages.AccountLocationEmail, EP.Messages.Employee })
			{
			}
		}
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Primary, Billing, Shipping, Employee, Contact },
							 new string[] { CR.Messages.AccountEmail, Messages.Billing, CR.Messages.AccountLocationEmail, EP.Messages.Employee, CR.Messages.Contact })
			{
			}
		}
	}
}
