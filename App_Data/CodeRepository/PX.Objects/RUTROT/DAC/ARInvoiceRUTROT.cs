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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.RUTROT
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class ARInvoiceRUTROT : PXCacheExtension<ARInvoice>, IRUTROTable
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}
		#region IsRUTROTDeductible
		public abstract class isRUTROTDeductible : PX.Data.BQL.BqlBool.Field<isRUTROTDeductible> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is subjected to ROT and RUT deductions.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">ROT and RUT Deduction</see> feature is enabled,
		/// the value of the <see cref="BranchRUTROT.AllowsRUTROT"/> field is <c>true</c> for the 
		/// <see cref = "ARInvoice.BranchID" > branch of the document</see>,
		/// and the document has a compatible type (see <see cref="ARInvoice.DocType"/>, <see cref="RUTROTHelper.IsRUTROTcompatibleType"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ROT and RUT deductible document", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual bool? IsRUTROTDeductible
		{
			get;
			set;
		}
		#endregion

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is either completed or released and cannot be edited anymore.
		/// </summary>
		public bool? GetRUTROTCompleted()
		{
			return Base.Released;
		}

		/// <summary>
		/// Gets <see cref="ARInvoice.RefNbr"/>.
		/// </summary>
		public string GetDocumentNbr()
		{
			return Base.RefNbr;
		}

		/// <summary>
		/// Gets <see cref="ARInvoice.DocType"/>.
		/// </summary>
		public string GetDocumentType()
		{
			return Base.DocType;
		}

		/// <summary>
		/// Gets <see cref="ARInvoice.BranchID"/>.
		/// </summary>
		public int? GetDocumentBranchID()
		{
			return Base.BranchID;
		}

		/// <summary>
		/// Gets <see cref="ARInvoice.CuryID"/>.
		/// </summary>
		public string GetDocumentCuryID()
		{
			return Base.CuryID;
		}

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is on hold and can be saved in the unbalanced state.
		/// </summary>
		public bool? GetDocumentHold()
		{
			return Base.Hold;
		}

		/// <summary>
		/// Returns the <see cref="ARInvoice">base document</see>.
		/// </summary>
		public IBqlTable GetBaseDocument()
		{
			return Base;
		}
	}
}
