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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using System.Linq;

namespace PX.Objects.AM.GraphExtensions
{
	public class EPShiftCodeSetupExtension : PXGraphExtension<EPShiftCodeSetup>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
		}

		public virtual void _(Events.FieldUpdating<EPShiftCode.shiftCD> e)
		{
			if (SelectFrom<EPShiftCode>
				.Where<EPShiftCode.isManufacturingShift.IsEqual<True>
					.And<EPShiftCode.shiftCD.IsEqual<P.AsString>>>.View.Select(Base, e.NewValue).Any())
			{
				throw new PXSetPropertyException<EPShiftCode.shiftCD>(Messages.ShiftCodeExistsInManufacturing);
			}
		}
	}
}
