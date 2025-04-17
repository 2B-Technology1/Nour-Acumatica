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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	public class TaxCalculationMode
	{
		public const string Net = "N";
		public const string Gross = "G";
		public const string TaxSetting = "T";

		public class ListAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
		{
			public ListAttribute() : base(new[] { TaxSetting, Gross, Net }, new[] { AP.Messages.TaxSetting, AP.Messages.TaxGross, AP.Messages.TaxNet }) { }

			public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				if (e.Row == null) return;

				string newTaxCalculationMode = e.NewValue as string;

				if (!PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() && newTaxCalculationMode != TaxCalculationMode.TaxSetting)
				{
					e.NewValue = TaxCalculationMode.TaxSetting;
				}
			}

			public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (e.Row == null) return;

				string newTaxCalculationMode = e.NewValue as string;
				bool isNetGrossEntryModeOn = PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

				if (!isNetGrossEntryModeOn && newTaxCalculationMode != TaxCalculationMode.TaxSetting
					&& ((!sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI) || sender.Graph.UnattendedMode))
				{
					e.NewValue = TaxCalculationMode.TaxSetting;
					return;
				}

				if (!isNetGrossEntryModeOn && newTaxCalculationMode != TaxCalculationMode.TaxSetting)
				{
					throw new PXSetPropertyException(Messages.TaxSetttigModeOnlyWhenNetGrossIsOff);
				}
				else if (isNetGrossEntryModeOn &&
						 newTaxCalculationMode != TaxCalculationMode.TaxSetting &&
						 newTaxCalculationMode != TaxCalculationMode.Net &&
						 newTaxCalculationMode != TaxCalculationMode.Gross)
				{
					throw new PXSetPropertyException(Messages.InvalidTaxCalculationMode);
				}
			}
		}

		public class gross : PX.Data.BQL.BqlString.Constant<gross>
		{
			public gross() : base(Gross) { }
		}

		public class net : PX.Data.BQL.BqlString.Constant<net>
		{
			public net() : base(Net) { }
		}

		public class taxSetting : PX.Data.BQL.BqlString.Constant<taxSetting>
		{
			public taxSetting() : base(TaxSetting) { }
		}
	}
}
