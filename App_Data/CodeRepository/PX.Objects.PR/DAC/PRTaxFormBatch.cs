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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;
using PX.Objects.TX.DAC.ReportParameters;
using PX.Payroll.GovernmentSlips;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information of multiple employees' annual tax form related to a specific year.
	/// </summary>
	[PXCacheName(Messages.PRTaxFormBatch)]
	[Serializable]
	[PXPrimaryGraph(typeof(PRTaxFormBatchMaint))]
	public class PRTaxFormBatch : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRTaxFormBatch>.By<batchID>
		{
			public static PRTaxFormBatch Find(PXGraph graph, string batchID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, batchID, options);
		}
		#endregion

		#region Events
		public class Events : PXEntityEvent<PRTaxFormBatch>.Container<Events>
		{
			public PXEntityEvent<PRTaxFormBatch> PublishTaxForm;
			public PXEntityEvent<PRTaxFormBatch> UnPublishTaxForm;
		}
		#endregion

		#region BatchID
		public abstract class batchID : PX.Data.BQL.BqlString.Field<batchID> { }
		[PXUIField(DisplayName = "Batch ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAA")]
		[PXSelector(typeof(SearchFor<PRTaxFormBatch.batchID>), SelectorMode = PXSelectorMode.DisplayMode)]
		[AutoNumber(typeof(PRSetup.batchForSubmissionNumberingCD), typeof(AccessInfo.businessDate))]
		public string BatchID { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the Tax Form.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="TaxFormBatchStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TaxFormBatchStatus.NonePublished)]
		[TaxFormBatchStatus.List]
		public virtual string Status { get; set; }
		#endregion
		#region FormType
		public abstract class formType : PX.Data.BQL.BqlString.Field<formType> { }
		[PXDBString(30)]
		[PXDefault(GovernmentSlipTypes.T4)]
		[PXUIField(DisplayName = "Form Name", Enabled = false)]
		[PXStringList(new[] { GovernmentSlipTypes.T4, GovernmentSlipTypes.RL1 }, new[] { Messages.T4, Messages.Releve1 })]
		public string FormType { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxFormBatchType.Original)]
		[PXUIField(DisplayName = "Document Type", Enabled = false)]
		[TaxFormBatchType.List]
		public string DocType { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		[PXDBString(4)]
		[PXUIField(DisplayName = "Year", Enabled = false)]
		public string Year { get; set; }
		#endregion
		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
		[OrganizationTree(
			treeDataMember: typeof(TaxTreeSelect),
			onlyActive: true,
			SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches, Enabled = false)]
		public virtual int? OrgBAccountID { get; set; }
		#endregion
		#region DownloadedAt
		public abstract class downloadedAt : PX.Data.BQL.BqlDateTime.Field<downloadedAt> { }
		[PXDBDate]
		[PXUIField(DisplayName = "Downloaded On", Enabled = false)]
		public DateTime? DownloadedAt { get; set; }
		#endregion
		#region EverPublished
		public abstract class everPublished : PX.Data.BQL.BqlBool.Field<everPublished> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Enabled = false)]
		public virtual bool? EverPublished { get; set; }
		#endregion
		#region NumberOfEmployees
		public abstract class numberOfEmployees : PX.Data.BQL.BqlInt.Field<numberOfEmployees> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Number of Employees", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? NumberOfEmployees { set; get; }
		#endregion
		#region NumberOfPublishedEmployees
		public abstract class numberOfPublishedEmployees : PX.Data.BQL.BqlInt.Field<numberOfPublishedEmployees> { }
		/// <summary>
		/// The number of published employees.
		/// </summary>
		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? NumberOfPublishedEmployees { set; get; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
