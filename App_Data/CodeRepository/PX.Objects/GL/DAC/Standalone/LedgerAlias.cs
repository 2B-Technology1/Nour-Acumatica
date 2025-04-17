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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;

namespace PX.Objects.GL.Standalone
{
	[Serializable]
	[PXCacheName(Messages.Ledger)]
	public partial class LedgerAlias : PX.Objects.GL.Ledger
	{
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		public new abstract class ledgerCD : PX.Data.BQL.BqlInt.Field<ledgerCD> { }
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		public new abstract class baseCuryID : PX.Data.BQL.BqlInt.Field<baseCuryID> { }
		public new abstract class descr : PX.Data.BQL.BqlInt.Field<descr> { }
		public new abstract class balanceType : PX.Data.BQL.BqlInt.Field<balanceType> { }
	}
}
