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
using PX.Objects.CM;

namespace PX.Objects.PM.BudgetControl
{
	[PXHidden]
	public class PMBudgetControlLine : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXInt(IsKey = true)]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PXInt(IsKey = true)]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXInt(IsKey = true)]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXInt(IsKey = true)]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXInt(IsKey = true)]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region BudgetedAmount
		public abstract class budgetedAmount : PX.Data.BQL.BqlDecimal.Field<budgetedAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Budgeted")]
		public virtual decimal? BudgetedAmount
		{
			get;
			set;
		}
		#endregion
		#region ConsumedAmount
		public abstract class consumedAmount : PX.Data.BQL.BqlDecimal.Field<consumedAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Consumed")]
		public virtual decimal? ConsumedAmount
		{
			get;
			set;
		}
		#endregion
		#region AvailableAmount
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Available")]
		public virtual decimal? AvailableAmount
		{
			get { return BudgetedAmount - ConsumedAmount; }
		}
		#endregion
		#region DocumentAmount
		public abstract class documentAmount : PX.Data.BQL.BqlDecimal.Field<documentAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Document")]
		public virtual decimal? DocumentAmount
		{
			get;
			set;
		}
		#endregion
		#region RemainingAmount
		public abstract class remainingAmount : PX.Data.BQL.BqlDecimal.Field<remainingAmount> { }
		[PXBaseCury]
		[PXUIField(DisplayName = "Remaining")]
		public virtual decimal? RemainingAmount
		{
			get { return AvailableAmount - DocumentAmount; }
		}
		#endregion

		#region BudgetExist
		public abstract class budgetExist : PX.Data.BQL.BqlBool.Field<budgetExist> { }
		[PXBool]
		public virtual bool? BudgetExist
		{
			get;
			set;
		}
		#endregion
		#region LineNumbers
		public abstract class lineNumbers : PX.Data.BQL.BqlString.Field<lineNumbers> { }
		[PXString]
		public virtual string LineNumbers
		{
			get;
			set;
		}
		#endregion

		#region TranDescription
		public abstract class tranDescription : PX.Data.BQL.BqlString.Field<tranDescription> { }
		[PXString]
		public virtual string TranDescription
		{
			get;
			set;
		}
		#endregion
	}
}
