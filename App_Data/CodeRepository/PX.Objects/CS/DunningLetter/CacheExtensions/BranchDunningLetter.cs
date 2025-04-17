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
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.AR
{
	public sealed class BranchDunningLetter : PXCacheExtension<Branch>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>();
		}

		#region DunningCompanyBranchID
		public abstract class dunningCompanyBranchID : PX.Data.BQL.BqlInt.Field<dunningCompanyBranchID> { }
		/// <summary>
		/// Is the Branch is used for Dunning Letter as a source for DL when consolidating by Company
		/// </summary>
		[PXInt]
		public int? DunningCompanyBranchID { get; set; }
		#endregion

		#region IsDunningCompanyBranchID
		public abstract class isDunningCompanyBranchID : PX.Data.BQL.BqlBool.Field<isDunningCompanyBranchID> { }
		/// <summary>
		/// Is the Branch is used for Dunning Letter as a source for DL when consolidating by Company
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Dunning Letter Branch", FieldClass = "DunningLetter")]
		public bool? IsDunningCompanyBranchID { get; set; }
		#endregion

	}
}
