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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;

namespace PX.Objects.IN.Turnover
{
	[PXCacheName(Messages.INTurnoverCalc,
		PXDacType.Document)]
	public class INTurnoverCalc : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INTurnoverCalc>.By<branchID, fromPeriodID, toPeriodID>
		{
			public static INTurnoverCalc Find(PXGraph graph, int? branchID, string fromPeriodID, string toPeriodID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, branchID, fromPeriodID, toPeriodID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<INTurnoverCalc>.By<branchID> { }
			public class Site : INSite.PK.ForeignKeyOf<INTurnoverCalc>.By<siteID> { }
			public class ItemClass : INItemClass.PK.ForeignKeyOf<INTurnoverCalc>.By<itemClassID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTurnoverCalc>.By<inventoryID> { }
		}
		#endregion

		#region BranchID
		[Branch(onlyActive: false, useDefaulting: false, IsKey = true)]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion

		#region FromPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[PXUIField(DisplayName = "From Period")]
		[FinPeriodID(IsKey = true)]
		[PXDefault]
		public virtual string FromPeriodID { get; set; }
		public abstract class fromPeriodID : BqlString.Field<fromPeriodID> { }
		#endregion

		#region ToPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[PXUIField(DisplayName = "To Period")]
		[FinPeriodID(IsKey = true)]
		[PXDefault]
		public virtual string ToPeriodID { get; set; }
		public abstract class toPeriodID : BqlString.Field<toPeriodID> { }
		#endregion

		#region IsFullCalc
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsFullCalc { get; set; }
		public abstract class isFullCalc : BqlBool.Field<isFullCalc> { }
		#endregion

		#region IsInventoryListCalc
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsInventoryListCalc { get; set; }
		public abstract class isInventoryListCalc : BqlBool.Field<isInventoryListCalc> { }
		#endregion

		#region SiteID
		[Site]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region ItemClassID
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem.IsEqual<True>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion

		#region InventoryID
		[AnyInventory(typeof(SelectFrom<InventoryItem>
			.Where<Match<AccessInfo.userName.FromCurrent>
				.And<InventoryItem.stkItem.IsEqual<True>>>
			.SearchFor<InventoryItem.inventoryID>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.markedForDeletion>>),
			IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region IncludedProduction
		[PXDBBool]
		[PXDefault(typeof(INSetup.includeProductionInTurnover))]
		public virtual bool? IncludedProduction { get; set; }
		public abstract class includedProduction : BqlBool.Field<includedProduction> { }
		#endregion
		#region IncludedAssembly
		[PXDBBool]
		[PXDefault(typeof(INSetup.includeAssemblyInTurnover))]
		public virtual bool? IncludedAssembly { get; set; }
		public abstract class includedAssembly : BqlBool.Field<includedAssembly> { }
		#endregion
		#region IncludedIssue
		[PXDBBool]
		[PXDefault(typeof(INSetup.includeIssueInTurnover))]
		public virtual bool? IncludedIssue { get; set; }
		public abstract class includedIssue : BqlBool.Field<includedIssue> { }
		#endregion
		#region IncludedTransfer
		[PXDBBool]
		[PXDefault(typeof(INSetup.includeTransferInTurnover))]
		public virtual bool? IncludedTransfer { get; set; }
		public abstract class includedTransfer : BqlBool.Field<includedTransfer> { }
		#endregion

		#region NoteID
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : BqlGuid.Field<noteID> { }
		#endregion

		#region System fields

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Calculation Date and Time", Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region tstamp
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion

		#endregion

		#region Selected
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		public abstract class selected : BqlBool.Field<selected> { }
		#endregion
	}
}
