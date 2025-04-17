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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	[Obsolete("This DAC is obsolete, please use INCostCenter instead.")]
	[PXCacheName(Messages.CostCenter)]
	public class PMCostCenter : IBqlTable
	{
		public const string ValuatedCostCenterCD = "Valuated";

		#region Keys

		public class PK : PrimaryKeyOf<PMCostCenter>.By<costCenterID>.Dirty
		{
			public static PMCostCenter Find(PXGraph graph, int? costSiteID) => FindBy(graph, costSiteID, (costSiteID ?? 0) <= 0);
		}

		public class UK : PrimaryKeyOf<PMCostCenter>.By<siteID, costCenterCD>
		{
			public static PMCostCenter Find(PXGraph graph, int? siteID, string costSiteCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, costSiteCD, options);
		}

		public class UKProject : PrimaryKeyOf<PMCostCenter>.By<siteID, locationID, projectID, taskID>
		{
			public static PMCostCenter Find(PXGraph graph, int? siteID, int? locationID, int? projectID, int? taskID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, locationID, projectID, taskID, options);
		}

		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<PMCostCenter>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<PMCostCenter>.By<locationID> { }
			public class Project : PMProject.PK.ForeignKeyOf<PMCostCenter>.By<projectID> { }
			public class Task : PMTask.PK.ForeignKeyOf<PMCostCenter>.By<projectID, taskID> { }
			
		}

		#endregion

		#region CostCenterID

		[PXDBForeignIdentity(typeof(INCostSite), IsKey = true)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? CostCenterID { get; set; }
		public abstract class costCenterID : PX.Data.BQL.BqlInt.Field<costCenterID> { }
		#endregion

		#region CostCenterCD

		[PXDefault]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Cost Center ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string CostCenterCD { get; set; }
		public abstract class costCenterCD : PX.Data.BQL.BqlInt.Field<costCenterCD> { }

		#endregion

		#region SiteID

		[PXDefault]
		[Site]
		[PXForeignReference(typeof(FK.Site))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[Location(typeof(siteID))]
		public virtual Int32? LocationID
		{
			get;
			set;
		}
		#endregion

		#region ProjectID

		[PXDefault]
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>>),
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(FK.Project))]
		public virtual int? ProjectID { get; set; }
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		#endregion

		#region TaskID

		[ProjectTask(typeof(projectID), AllowNull = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(FK.Task))]
		public virtual int? TaskID { get; set; }
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		#endregion

		
		#region System

		#region CreatedDateTime

		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        #endregion

        #region LastModifiedDateTime

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        #endregion

        #region tstamp

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        #endregion 
        #endregion
    }
}
