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

using PX.Commerce.Core;
using PX.Common;
using PX.Data.BQL;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{

	#region FBQL Sequences

	/// <summary>
	/// Sequence used to filter inventory items by type
	/// </summary>
	public class InventoryItemsEntityTypeSequence : IBqlConstants,
							IBqlConstantsOf<IImplement<IBqlEquitable>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return BCEntitiesAttribute.StockItem;
			yield return BCEntitiesAttribute.NonStockItem;
			yield return BCEntitiesAttribute.ProductWithVariant;
		}
	}
	public class ExcludedInventoryItemStatusSequence : IBqlConstants,
								IBqlConstantsOf<IImplement<IBqlEquitable>>,
								IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
								IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return INItemStatus.Inactive;
			yield return InventoryItemStatus.Unknown;
			yield return INItemStatus.ToDelete;
		}
	}

	public class InvalidSyncStatusSquence : IBqlConstants,
							IBqlConstantsOf<IImplement<IBqlEquitable>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return BCSyncStatusAttribute.Filtered;
			yield return BCSyncStatusAttribute.Invalid;
			yield return BCSyncStatusAttribute.Skipped;
			yield return BCSyncStatusAttribute.Deleted;
		}
	}

	#endregion

	#region entities definition for FBQL queries (for multiple joins on the same tables)

	/// <summary>
	/// When writing FBQL queries and we need to join the same table multiple times, we need to define a different entity for each join.
	/// Hence the following entities are defined.
	/// </summary>
	[PXInternalUseOnly]
	[PXHidden]
	public class BCSyncStatusForInventoryItem : BCSyncStatus
	{
		public new abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		public new abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		public new abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		public new abstract class connectorType : BqlType<IBqlString, string>.Field<connectorType> { }
		public new abstract class syncID : PX.Data.BQL.BqlString.Field<syncID> { }
		public new abstract class lastOperation : PX.Data.BQL.BqlString.Field<lastOperation> { }
		public new abstract class lastOperationTS : PX.Data.BQL.BqlDateTime.Field<lastOperationTS> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
	}
	[PXInternalUseOnly]
	[PXHidden]
	public class BCSyncStatusForVariantInventoryItem : BCSyncStatus
	{
		public new abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		public new abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		public new abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		public new abstract class connectorType : BqlType<IBqlString, string>.Field<connectorType> { }
		public new abstract class syncID : PX.Data.BQL.BqlString.Field<syncID> { }
		public new abstract class lastOperation : PX.Data.BQL.BqlString.Field<lastOperation> { }
		public new abstract class lastOperationTS : PX.Data.BQL.BqlDateTime.Field<lastOperationTS> { }

		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
	}
	[PXInternalUseOnly]
	[PXHidden]
	public class BCSyncStatusForPriceClass : BCSyncStatus
	{
		public new abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		public new abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		public new abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		public new abstract class connectorType : BqlType<IBqlString, string>.Field<connectorType> { }
		public new abstract class syncID : PX.Data.BQL.BqlString.Field<syncID> { }
		public new abstract class lastOperation : PX.Data.BQL.BqlString.Field<lastOperation> { }
		public new abstract class lastOperationTS : PX.Data.BQL.BqlDateTime.Field<lastOperationTS> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
	}
	[PXInternalUseOnly]
	[PXHidden]
	public class BCSyncStatusForCustomerPriceClass : BCSyncStatus
	{
		public new abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		public new abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		public new abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		public new abstract class connectorType : BqlType<IBqlString, string>.Field<connectorType> { }
		public new abstract class syncID : PX.Data.BQL.BqlString.Field<syncID> { }
		public new abstract class lastOperation : PX.Data.BQL.BqlString.Field<lastOperation> { }
		public new abstract class lastOperationTS : PX.Data.BQL.BqlDateTime.Field<lastOperationTS> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
	}
	[PXInternalUseOnly]
	[PXHidden]
	public class BCSyncDetailForVariantInventoryItem : BCSyncDetail
	{
		public new abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		public new abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		public new abstract class syncID : PX.Data.BQL.BqlString.Field<syncID> { }

		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
	}
	#endregion
}

