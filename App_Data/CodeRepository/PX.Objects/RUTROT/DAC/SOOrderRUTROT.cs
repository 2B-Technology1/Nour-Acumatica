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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;

namespace PX.Objects.RUTROT
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class SOOrderRUTROT : PXCacheExtension<SOOrder>, IRUTROTable
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region IsRUTROTDeductible
		public abstract class isRUTROTDeductible : PX.Data.BQL.BqlBool.Field<isRUTROTDeductible> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is subjected to ROT and RUT deductions.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">ROT and RUT Deduction</see> feature is enabled,
		/// the value of the <see cref="BranchRUTROT.AllowsRUTROT"/> field is <c>true</c> 
		/// for the <see cref="SOOrder.BranchID">branch of the document</see>,
		/// and the document has a compatible type (see<see cref="SOOrder.OrderType"/>, <see cref = "RUTROTHelper.IsRUTROTcompatibleType" />).
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
			return Base.Completed;
		}

		public string GetDocumentNbr()
		{
			return Base.OrderNbr;
		}

		public string GetDocumentType()
		{
			return Base.OrderType;
		}

		/// <summary>
		/// Gets <see cref="SOOrder.BranchID"/>.
		/// </summary>
		public int? GetDocumentBranchID()
		{
			return Base.BranchID;
		}

		/// <summary>
		/// Gets <see cref="SOOrder.curyID"/>.
		/// </summary>
		public string GetDocumentCuryID()
		{
			return Base.CuryID;
		}

		/// <summary>
		/// Specifies (if returns <c>true</c>) that the document is on hold and can be saved in the unbalanced state.
		/// </summary>
		public bool? GetDocumentHold()
		{
			return Base.Hold;
		}
		
		/// <summary>
		/// Returns <see cref="SOOrder">base document</see>.
		/// </summary>
		public IBqlTable GetBaseDocument()
		{
			return Base;
		}
	}
}
