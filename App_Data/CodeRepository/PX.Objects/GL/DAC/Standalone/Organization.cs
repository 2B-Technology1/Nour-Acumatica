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
using System.Diagnostics;
using PX.Data;
using PX.Objects.CR;
using PX.SM;
using PX.Objects.CS;
using PX.Objects.CS.DAC;
using PX.Objects.AP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;

namespace PX.Objects.GL.DAC.Standalone
{
	[PXCacheName(CS.Messages.Company)]
	[Serializable]
	public partial class OrganizationAlias : GL.DAC.Organization
	{
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		public new abstract class organizationCD : PX.Data.BQL.BqlString.Field<organizationCD> { }
		public new abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
	}
}