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

using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.Common
{
	class CurrentConfiguration
	{
		#region APSetup

		/// <summary>
		/// Returns actual values of selected fields from APSetup table.
		/// </summary>
		public static APSetupCache ActualAPSetup
		{
			get
			{
				APSetupCache apSetup =
					PXContext.GetSlot<APSetupCache>() ??
					PXContext.SetSlot(
						PXDatabase.GetSlot<APSetupCache>(
							"ActualAPSetup",
							typeof(APSetup)));
				return apSetup;
			}
		}

		public class APSetupCache : IPrefetchable
		{
			public bool isMigrationModeEnabled { get; private set; }
			public string ApplyQuantityDiscountBy { get; private set; }

			public void Prefetch()
			{
				using (PXDataRecord apSetup = PXDatabase.SelectSingle<APSetup>(
					new PXDataField<APSetup.migrationMode>(),
					new PXDataField<APSetup.applyQuantityDiscountBy>()))
				{
					if (apSetup != null)
					{
						isMigrationModeEnabled = (bool)apSetup.GetBoolean(0);
						ApplyQuantityDiscountBy = apSetup.GetString(1);
					}
				}
			}
		}
		#endregion

		#region ARSetup

		/// <summary>
		/// Returns actual values of selected fields from APSetup table.
		/// </summary>
		public static ARSetupCache ActualARSetup
		{
			get
			{
				ARSetupCache apSetup =
					PXContext.GetSlot<ARSetupCache>() ??
					PXContext.SetSlot(
						PXDatabase.GetSlot<ARSetupCache>(
							"ActualARSetup",
							typeof(ARSetup)));
				return apSetup;
			}
		}

		public class ARSetupCache : IPrefetchable
		{
			public bool isMigrationModeEnabled { get; private set; }
			public string ApplyQuantityDiscountBy { get; private set; }

			public void Prefetch()
			{
				using (PXDataRecord arSetup = PXDatabase.SelectSingle<ARSetup>(
					new PXDataField<ARSetup.migrationMode>(),
					new PXDataField<APSetup.applyQuantityDiscountBy>()))
				{
					if (arSetup != null)
					{
						isMigrationModeEnabled = (bool)arSetup.GetBoolean(0);
						ApplyQuantityDiscountBy = arSetup.GetString(1);
					}
				}
			}
		}
		#endregion
	}
}
