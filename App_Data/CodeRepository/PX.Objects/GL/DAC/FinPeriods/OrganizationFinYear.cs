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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.FinPeriods
{
    [PXCacheName("Company Financial Period")]
	[Serializable]
	[PXProjection(typeof(Select<FinYear,
			Where<FinYear.organizationID, NotEqual<FinPeriod.organizationID.masterValue>>>),
		Persistent = true)]
	public class OrganizationFinYear : IBqlTable, IFinYear
	{
		#region Keys
		public class PK : PrimaryKeyOf<OrganizationFinYear>.By<organizationID, year>
		{
			public static OrganizationFinYear Find(PXGraph graph, int? organizationID, string year, PKFindOptions options = PKFindOptions.None) => FindBy(graph, organizationID, year, options);
			public static OrganizationFinYear FindByBranch(PXGraph graph, int? branchID, string year)
				=> (OrganizationFinYear)PXSelectJoin<OrganizationFinYear, InnerJoin<Branch, On<Branch.organizationID, Equal<OrganizationFinYear.organizationID>>>,
									Where<year, Equal<Required<year>>, And<Branch.branchID, Equal<Required<Branch.branchID>>>>>
									.SelectWindowed(graph, 0, 1, year, branchID);
		}
		public static class FK
		{
			public class Organization : GL.DAC.Organization.PK.ForeignKeyOf<OrganizationFinYear>.By<organizationID> { }
			public class Year : GL.FinPeriods.MasterFinYear.PK.ForeignKeyOf<OrganizationFinYear>.By<year> { }
		}
		#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(IsKey = true, BqlTable = typeof(TableDefinition.FinYear))]
		[PXParent(typeof(Select<
			Organization, 
			Where<Organization.organizationID, Equal<Current<OrganizationFinYear.organizationID>>>>))]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }

		/// <summary>
		/// Key field.
		/// The financial year.
		/// </summary>
		[PXDBString(4, IsKey = true, IsFixed = true, BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault("")]
		[PXUIField(DisplayName = "Financial Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<
			OrganizationFinYear.year, 
			Where<OrganizationFinYear.organizationID, Equal<Current<OrganizationFinYear.organizationID>>>, 
			OrderBy<
				Desc<OrganizationFinYear.year>>>))]
		[PXFieldDescription]
		[PXParent(typeof(Select<
			MasterFinYear,
			Where<MasterFinYear.year, Equal<Current<OrganizationFinYear.year>>>>))]
		public virtual string Year { get; set; }
		#endregion

		#region StartMasterFinPeriodID
		public abstract class startMasterFinPeriodID : PX.Data.BQL.BqlString.Field<startMasterFinPeriodID> { }

		[PXDBString(6, IsFixed = true, BqlTable = typeof(TableDefinition.FinYear))]
		[FinPeriodIDFormatting]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.Visible, Enabled = false, DisplayName = "Start Master Period ID")]
		public virtual string StartMasterFinPeriodID { get; set; }
		#endregion	

		#region FinPeriods
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }

		/// <summary>
		/// The number of periods in the year.
		/// </summary>
		[PXDBShort(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Periods", Enabled = false)]
		public virtual short? FinPeriods { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		/// <summary>
		/// The start date of the year.
		/// </summary>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		/// <summary>
		/// The end date of the year (inclusive).
		/// </summary>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault()]
		[PXUIField(DisplayName = "EndDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote(DescriptionField = typeof(MasterFinYear.year), BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
