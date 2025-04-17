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
using PX.SM;

namespace PX.Objects.SM
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ScaleWeightConversionExt : PXGraphExtension<ScaleMaint>
	{
		protected virtual void _(Events.RowSelected<SMScale> e)
		{
			if (e.Row == null)
				return;

			if (!Base.IsImport)
				RaiseConversionError(e.Row);
		}

		protected virtual void RaiseConversionError(SMScale scale)
		{
			var scaleExt = scale.GetExtension<SMScaleWeightConversion>();

			PXSetPropertyException uomError = null;
			PXSetPropertyException weightError = null;
			if (scaleExt?.CompanyUOM == null)
				uomError = new PXSetPropertyException(IN.Messages.BaseCompanyUomIsNotDefined);
			else if (scale.LastWeight != null && scaleExt.CompanyLastWeight == null)
				weightError = new PXSetPropertyException(IN.Messages.MissingGlobalUnitConversion, scale.UOM, scaleExt.CompanyUOM);

			Base.Scale.Cache.RaiseExceptionHandling<SMScaleWeightConversion.companyUOM>(scale, scaleExt.CompanyUOM, uomError);
			Base.Scale.Cache.RaiseExceptionHandling<SMScaleWeightConversion.companyLastWeight>(scale, scaleExt.CompanyLastWeight, weightError);
		}
	}
}
