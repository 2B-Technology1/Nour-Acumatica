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
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.IN.InventoryRelease.Accumulators.Documents
{
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator(BqlTable = typeof(INTranCost))]
	public class INTranCostUpdate : IBqlTable
	{
		#region DocType
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual string DocType
		{
			get;
			set;
		}
		public abstract class docType : BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string RefNbr
		{
			get;
			set;
		}
		public abstract class refNbr : BqlString.Field<refNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion
		#region CostDocType
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string CostDocType
		{
			get;
			set;
		}
		public abstract class costDocType : BqlString.Field<costDocType> { }
		#endregion
		#region CostRefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string CostRefNbr
		{
			get;
			set;
		}
		public abstract class costRefNbr : BqlString.Field<costRefNbr> { }
		#endregion
		#region CostID
		[PXDBLong(IsKey = true)]
		[PXDefault]
		public virtual long? CostID
		{
			get;
			set;
		}
		public abstract class costID : BqlLong.Field<costID> { }
		#endregion
		#region ResetOversoldFlag
		[PXBool]
		[PXUnboundDefault(false)]
		public virtual bool? ResetOversoldFlag
		{
			get;
			set;
		}
		public abstract class resetOversoldFlag : BqlBool.Field<resetOversoldFlag> { }
		#endregion
		#region IsOversold
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsOversold
		{
			get;
			set;
		}
		public abstract class isOversold : BqlBool.Field<isOversold> { }
		#endregion
		#region ValMethod
		[PXString(1, IsFixed = true)]
		public virtual string ValMethod
		{
			get;
			set;
		}
		public abstract class valMethod : BqlString.Field<valMethod> { }
		#endregion
		#region OversoldQty
		[PXDBDecimal(6)]
		[PXDefault(typeof(decimal0), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? OversoldQty
		{
			get;
			set;
		}
		public abstract class oversoldQty : BqlDecimal.Field<oversoldQty> { }
		#endregion
		#region OversoldTranCost
		[PXDBBaseCury]
		[PXDefault(typeof(decimal0), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? OversoldTranCost
		{
			get;
			set;
		}
		public abstract class oversoldTranCost : BqlDecimal.Field<oversoldTranCost> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				var itcrow = (INTranCostUpdate)row;

				columns.UpdateOnly = true;

				if (itcrow.ResetOversoldFlag ?? false)
					columns.Update<isOversold>(false, Replace);

				columns.Update<oversoldQty>(Summarize);
				columns.Update<oversoldTranCost>(Summarize);

				if (itcrow.ResetOversoldFlag ?? false)
				{
					columns.AppendException(string.Empty, new PXAccumulatorRestriction<oversoldQty>(PXComp.EQ, 0));
					if (itcrow.ValMethod != INValMethod.Standard)
						columns.AppendException(string.Empty, new PXAccumulatorRestriction<oversoldTranCost>(PXComp.EQ, 0));
				}
				else
				{
					columns.AppendException(string.Empty, new PXAccumulatorRestriction<oversoldQty>(PXComp.GE, 0));
					if (itcrow.ValMethod != INValMethod.Standard)
					{
						columns.AppendException(string.Empty, new PXAccumulatorRestriction<oversoldTranCost>(PXComp.GE, 0));
						columns.AppendException(string.Empty,
							new PXAccumulatorRestriction<oversoldQty>(PXComp.GT, 0),
							new PXAccumulatorRestriction<oversoldTranCost>(PXComp.EQ, 0));
					}
				}
				return true;
			}

			public override bool PersistInserted(PXCache cache, object row)
			{
				try
				{
					return base.PersistInserted(cache, row);
				}
				catch (PXRestrictionViolationException)
				{
					var diff = (INTranCostUpdate)row;
					var it = INTran.PK.Find(cache.Graph, diff.DocType, diff.RefNbr, diff.LineNbr);

					throw new PXRestartOperationException(new PXException(Messages.INTranCostOverReceipted,
						cache.Graph.Caches[typeof(INTran)].GetValueExt<INTran.inventoryID>(it),
						cache.Graph.Caches[typeof(INTran)].GetValueExt<INTran.subItemID>(it)));
				}
			}
		}
	}
}
