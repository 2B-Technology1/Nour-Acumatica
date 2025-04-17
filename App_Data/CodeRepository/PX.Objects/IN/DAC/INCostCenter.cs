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
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.SO;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INCostCenter)]
	public class INCostCenter : IBqlTable
	{
		#region Keys

		public class PK : PrimaryKeyOf<INCostCenter>.By<costCenterID>.Dirty
		{
			public static INCostCenter Find(PXGraph graph, int? costSiteID) => FindBy(graph, costSiteID, (costSiteID ?? 0) <= 0);
		}

		public class UKProject : PrimaryKeyOf<INCostCenter>.By<siteID, locationID, projectID, taskID>
		{
			public static INCostCenter Find(PXGraph graph, int? siteID, int? locationID, int? projectID, int? taskID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, locationID, projectID, taskID, options);
		}

		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INCostCenter>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INCostCenter>.By<locationID> { }
			public class Project : PMProject.PK.ForeignKeyOf<INCostCenter>.By<projectID> { }
			public class OrderLine : SOLine.PK.ForeignKeyOf<INCostCenter>.By<sOOrderType, sOOrderNbr, sOOrderLineNbr> { }
			public class Task : PMTask.PK.ForeignKeyOf<INCostCenter>.By<projectID, taskID> { }
		}

		#endregion

		#region CostCenterID
		[PXDBForeignIdentity(typeof(INCostSite), IsKey = true)]
		[PXReferentialIntegrityCheck]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		public abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }
		#endregion
		#region CostCenterCD
		[PXDefault]
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Cost Center ID", Visible = false)]
		public virtual string CostCenterCD
		{
			get;
			set;
		}
		public abstract class costCenterCD : Data.BQL.BqlInt.Field<costCenterCD> { }
		#endregion
		#region CostLayerType
		[PXDBString(1)]
		[PXDefault]
		[CostLayerType.List]
		public virtual string CostLayerType
		{
			get;
			set;
		}
		public abstract class costLayerType : Data.BQL.BqlString.Field<costLayerType> { }
		#endregion
		#region SiteID
		[PXDefault]
		[Site]
		[PXForeignReference(typeof(FK.Site))]
		public virtual int? SiteID
		{
			get;
			set;
		}
		public abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		#endregion

		#region LocationID
		public abstract class locationID : Data.BQL.BqlInt.Field<locationID> { }
		[Location(typeof(siteID))]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>>),
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(FK.Project))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		public abstract class projectID : Data.BQL.BqlInt.Field<projectID> { }
		#endregion
		#region TaskID
		[ProjectTask(typeof(projectID), AllowNull = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(FK.Task))]
		public virtual int? TaskID
		{
			get;
			set;
		}
		public abstract class taskID : Data.BQL.BqlInt.Field<taskID> { }
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : Data.BQL.BqlString.Field<sOOrderType> { }
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Sales Order Type", Enabled = false)]
		public virtual string SOOrderType
		{
			get;
			set;
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : Data.BQL.BqlString.Field<sOOrderNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Sales Order Nbr.", Enabled = false)]
		public virtual string SOOrderNbr
		{
			get;
			set;
		}
		#endregion
		#region SOOrderLineNbr
		public abstract class sOOrderLineNbr : Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Sales Order Line Nbr.", Enabled = false)]
		public virtual int? SOOrderLineNbr
		{
			get;
			set;
		}
		#endregion

		#region System
		#region CreatedByID
		public abstract class createdByID : Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		public abstract class createdDateTime : Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#endregion
	}
}
