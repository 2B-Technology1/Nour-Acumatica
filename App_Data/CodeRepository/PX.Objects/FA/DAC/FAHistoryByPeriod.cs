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
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{
	/// <summary>
	/// The DAC used to simplify selection and aggregation of proper <see cref="FABookHistory"/> records
	/// on various inquiry and processing screens of the Fixed Assets module.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select5<
		FABookHistory,
		InnerJoin<FixedAsset, 
			On<FABookHistory.assetID, Equal<FixedAsset.assetID>>,
		InnerJoin<Branch, 
			On<FixedAsset.branchID, Equal<Branch.branchID>>,
		InnerJoin<FABook, 
			On<FABookHistory.bookID, Equal<FABook.bookID>>,
		InnerJoin<FABookPeriod, 
			On<FABookPeriod.bookID, Equal<FABookHistory.bookID>, 
			And<FABookPeriod.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>,
			And<FABookPeriod.finPeriodID, GreaterEqual<FABookHistory.finPeriodID>>>>>>>>,
		Aggregate<
			GroupBy<FABookHistory.assetID,
			GroupBy<FABookHistory.bookID,
			Max<FABookHistory.finPeriodID,
			GroupBy<FABookPeriod.finPeriodID>>>>>>))]
	[PXCacheName(FA.Messages.FAHistoryByPeriod)]
	public partial class FAHistoryByPeriod : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FAHistoryByPeriod>.By<assetID, bookID, finPeriodID>
		{
			public static FAHistoryByPeriod Find(PXGraph graph, Int32? assetID, Int32? bookID, String finPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, assetID, bookID, finPeriodID, options);
		}
		public static class FK
		{
			public class FixedAsset : FA.FixedAsset.PK.ForeignKeyOf<FAHistoryByPeriod>.By<assetID> { }
			public class Book : FA.FABook.PK.ForeignKeyOf<FAHistoryByPeriod>.By<bookID> { }
		}
		#endregion
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true, BqlField = typeof(FABookHistory.assetID))]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt(IsKey = true, BqlField = typeof(FABookHistory.bookID))]
		public virtual Int32? BookID
		{
			get
			{
				return this._BookID;
			}
			set
			{
				this._BookID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FABookPeriodID(
			assetSourceType: typeof(FAHistoryByPeriod.assetID),
			bookSourceType: typeof(FAHistoryByPeriod.bookID),
			IsKey = true, 
			BqlField = typeof(FABookPeriod.finPeriodID))]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[FABookPeriodID(
			assetSourceType: typeof(FAHistoryByPeriod.assetID),
			bookSourceType: typeof(FAHistoryByPeriod.bookID),
			BqlField = typeof(FABookHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
	}
}
