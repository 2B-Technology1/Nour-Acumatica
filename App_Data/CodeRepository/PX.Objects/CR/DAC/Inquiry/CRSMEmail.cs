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
using PX.Data;
using PX.TM;

namespace PX.Objects.CR.Inquiry
{
	[PXCacheName(Messages.EmailActivity)]
	[PXProjection(typeof(Select<PX.Objects.CR.CRSMEmail,
		Where<PX.Objects.CR.CRSMEmail.classID, Equal<PX.Objects.CR.CRActivityClass.email>,
			And<Where<PX.Objects.CR.CRSMEmail.createdByID, Equal<CurrentValue<AccessInfo.userID>>,
				Or<PX.Objects.CR.CRSMEmail.ownerID, Equal<CurrentValue<AccessInfo.contactID>>,
				Or<PX.Objects.CR.CRSMEmail.ownerID, IsSubordinateOfContact<CurrentValue<AccessInfo.contactID>>,
				Or<PX.Objects.CR.CRSMEmail.workgroupID, IsWorkgroupOfContact<CurrentValue<AccessInfo.contactID>>>>>>>
	>>))]
	public class CRSMEmail : PX.Objects.CR.CRSMEmail
	{
	}
}
