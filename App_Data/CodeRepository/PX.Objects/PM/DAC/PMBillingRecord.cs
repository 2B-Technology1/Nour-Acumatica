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

namespace PX.Objects.PM
{
	/// <summary>
	/// A project billing record.
	/// </summary>
	[PXCacheName(Messages.ProjectBillingRecord)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMBillingRecord : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the record.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID" /> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMProject.contractID))]
		[PXUIField(DisplayName = "Project ID")]
		public virtual Int32? ProjectID
		{
			get; set;
		}
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

		/// <summary>
		/// The sequence of line numbers of the records that belong to one project can include gaps.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the records belonging to a single project may include gaps.
		/// </value>
		[PXDefault(typeof(PMProject.billingLineCntr))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Billing Number")]
		public virtual Int32? RecordID
		{
			get;set;
		}
		#endregion
		
		#region BillingTag
		public abstract class billingTag : PX.Data.BQL.BqlString.Field<billingTag> { }

		/// <summary>
		/// An internal field that is used during the billing to group and segregate transactions between billing rules, tasks, and invoices.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// T: [PMTask.TaskID], if [PMTask.BillSeparately] is <see langword="true" /> for the billed task.
		/// L: [PMTask.LocationID], if [PMTask.LocationID] is not equal to the location ID from the parent project.
		/// P: Otherwise.
		/// </value>
		[PXDefault("P")]
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		public virtual String BillingTag
		{
			get; set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;

		/// <summary>
		/// The date when the billing is applied.
		/// </summary>
		/// <value>
		/// Defaults to the current <see cref="AccessInfo.BusinessDate">business date</see>.
		/// </value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Pro Forma Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region ProformaRefNbr
		public abstract class proformaRefNbr : PX.Data.BQL.BqlString.Field<proformaRefNbr> { }

		/// <summary>
		/// The reference number of the parent <see cref="PMProforma">pro forma invoice</see>.
		/// </summary>
		[PXSelector(typeof(Search<PMProforma.refNbr, Where<PMProforma.projectID, Equal<Current<PMBillingRecord.projectID>>>>))]
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Pro Forma Reference Nbr.")]
		public virtual String ProformaRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }

		/// <summary>
		/// The type of the AR document that is created during the billing.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.ARInvoice.DocType"/> field.
		/// </value>
		[PXUIField(DisplayName = "AR Doc. Type")]
		[AR.ARInvoiceType.List()]
		[PXDBString(3, IsFixed = true)]
		public virtual String ARDocType
		{
			get;
			set;
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }

		/// <summary>
		/// The reference number of the AR document that is created during the billing.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.ARInvoice.RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<PX.Objects.AR.ARInvoice.refNbr>))]
		public virtual String ARRefNbr
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		/// <summary>
		/// An internal field that is used to order records on the Invoices tab of the Projects (PM301000) form.
		/// </summary>
		[PXInt]
		public virtual Int32? SortOrder
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion

		#region RecordNumber
		public abstract class recordNumber : PX.Data.BQL.BqlInt.Field<recordNumber> { }

		/// <summary>
		/// The sequence number of the invoice that is being assigned to the invoices of the project in order of the creation of the invoices.
		/// </summary>
		/// <value>
		/// Retrives the value from the <see cref="RecordID"/> field.
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = "Billing Number", Visible = false)]
		public virtual Int32? RecordNumber
		{
			get { return RecordID < 0 ? null : RecordID; }
		}
		#endregion
	}

	[PXBreakInheritance]
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBillingRecordEx : PMBillingRecord
	{
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

		public new abstract class billingTag : PX.Data.BQL.BqlString.Field<billingTag> { }

		public new abstract class proformaRefNbr : PX.Data.BQL.BqlString.Field<proformaRefNbr> { }

		public new abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		public new abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }



	}
}
