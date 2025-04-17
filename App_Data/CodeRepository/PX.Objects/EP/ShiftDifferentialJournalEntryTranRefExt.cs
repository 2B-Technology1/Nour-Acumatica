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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PM.GraphExtensions;

namespace PX.Objects.EP
{
	public class ShiftDifferentialJournalEntryTranRefExt : PXGraphExtension<JournalEntryTranRef>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.shiftDifferential>();
		}

		public delegate void AssignAdditionalFieldsDelegate(GLTran glTran, PMTran pmTran);
		[PXOverride]
		public virtual void AssignAdditionalFields(GLTran glTran, PMTran pmTran, AssignAdditionalFieldsDelegate baseMethod)
		{
			baseMethod(glTran, pmTran);

			ShiftDifferentialGLTranExt glTranExt = PXCache<GLTran>.GetExtension<ShiftDifferentialGLTranExt>(glTran);
			ShiftDifferentialPMTranExt pmTranExt = PXCache<PMTran>.GetExtension<ShiftDifferentialPMTranExt>(pmTran);
			pmTranExt.ShiftID = glTranExt.ShiftID;
		}
	}
}
