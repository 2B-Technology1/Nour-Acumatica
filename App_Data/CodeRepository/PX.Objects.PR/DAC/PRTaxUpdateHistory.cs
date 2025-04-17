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

using PX.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Contains the logs of tax updates.
	/// </summary>
	[Serializable]
	[PXHidden]
	public partial class PRTaxUpdateHistory : IBqlTable
	{
		#region LastUpdateTime
		public abstract class lastUpdateTime : PX.Data.BQL.BqlDateTime.Field<lastUpdateTime> { }
		[PXDBDateAndTime]
		[PXDefault]
		public virtual DateTime? LastUpdateTime { get; set; }
		#endregion
		#region LastCheckTime
		public abstract class lastCheckTime : PX.Data.BQL.BqlDateTime.Field<lastCheckTime> { }
		[PXDBDateAndTime]
		public virtual DateTime? LastCheckTime { get; set; }
		#endregion
		#region ServerTaxDefinitionTimestamp
		public abstract class serverTaxDefinitionTimestamp : PX.Data.BQL.BqlDateTime.Field<serverTaxDefinitionTimestamp> { }
		[PXDBDateAndTime]
		public virtual DateTime? ServerTaxDefinitionTimestamp { get; set; }
		#endregion
		#region ServerCanadaTaxDefinitionTimestamp
		public abstract class serverCanadaTaxDefinitionTimestamp : PX.Data.BQL.BqlDateTime.Field<serverCanadaTaxDefinitionTimestamp> { }
		[PXDBDateAndTime]
		public virtual DateTime? ServerCanadaTaxDefinitionTimestamp { get; set; }
		#endregion
	}
}
