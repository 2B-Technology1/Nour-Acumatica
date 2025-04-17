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

namespace PX.Objects.PM.BudgetControl
{
	public class Document : PXMappedCacheExtension
	{
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region WarningAmount
		public abstract class warningAmount : PX.Data.BQL.BqlDecimal.Field<warningAmount> { }
		public virtual decimal? WarningAmount
		{
			get;
			set;
		}
		#endregion
		#region BudgetControlLinesInitialized
		public abstract class budgetControlLinesInitialized : PX.Data.BQL.BqlBool.Field<budgetControlLinesInitialized> { }
		public virtual bool? BudgetControlLinesInitialized
		{
			get;
			set;
		}
		#endregion
	}
}