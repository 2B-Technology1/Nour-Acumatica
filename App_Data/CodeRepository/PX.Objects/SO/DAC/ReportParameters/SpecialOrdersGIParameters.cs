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

using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;
using System;

namespace PX.Objects.SO.DAC.ReportParameters
{
	/// <exclude />
	[PXVirtual]
	[PXCacheName(Messages.SpecialOrderGIParameters)]
	public class SpecialOrderGIParameters : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
			[PXInternalUseOnly]
			public class BranchAttribute : PXEventSubscriberAttribute
			{
				public override void CacheAttached(PXCache sender)
				{
					base.CacheAttached(sender);
					HideTheFieldIfFeatureDisabled(sender.Graph);
				}

				protected virtual void HideTheFieldIfFeatureDisabled(PXGraph graph)
				{
					const string FieldName = "Branch";

					if (typeof(PXGenericInqGrph).IsAssignableFrom(graph.GetType()) &&
						graph.Views.ContainsKey(PXGenericInqGrph.FilterViewName))
					{
						graph.FieldSelecting.AddHandler(PXGenericInqGrph.FilterViewName, FieldName, (cache, args) =>
						{
							var state = args.ReturnState as PXFieldState;
							if (state?.Visibility == PXUIVisibility.HiddenByAccessRights)
								state.Visible = false;
						});
					}
				}
			}
		}
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, IsDBField = false)]
		[branchID.Branch]
		public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXString(2, IsKey = true, IsFixed = true)]
		[PXSelector(typeof(Search<SOOrderType.orderType,
			Where<SOOrderType.behavior, In3<SOBehavior.sO, SOBehavior.rM, SOBehavior.qT>>,
			 OrderBy<Desc<SOOrderType.orderType>>>), Filterable = true)]
		public virtual String OrderType
		{
			get;
			set;
		}
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXString(15, IsKey = true, IsUnicode = true)]
		[PXSelector(typeof(Search2<SOOrder.orderNbr,
			LeftJoinSingleTable<Customer, On<SOOrder.FK.Customer.
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<SOOrder.orderType, Equal<Current<orderType>>,
				And<Exists<Select<SOLine, Where<SOLine.isSpecialOrder, Equal<True>, And<SOLine.FK.Order>>>>>>,
			OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
		public virtual String OrderNbr
		{
			get;
			set;
		}
		#endregion
	}
}
