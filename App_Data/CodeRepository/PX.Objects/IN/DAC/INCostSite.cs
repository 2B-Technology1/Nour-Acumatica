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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[PXHidden]
	public class INCostSite : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCostSite>.By<costSiteID>
		{
			public static INCostSite Find(PXGraph graph, int? costSiteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, costSiteID, options);
		}
		#endregion
		#region CostSiteID
		[PXDBIdentity]
		public virtual Int32? CostSiteID { get; set; }
		public abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region CostSiteType
		[PXDBForeignIdentityType]
		public virtual string CostSiteType { get; set; }
		public abstract class costSiteType : PX.Data.BQL.BqlString.Field<costSiteType> { }
		#endregion
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
