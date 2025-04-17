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

namespace PX.Objects.Localizations.CA.GL
{
	public class JournalWithSubEntryExt : PXGraphExtension<JournalWithSubEntry>
	{
		#region IsActive

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
		}

		#endregion

		protected virtual void _(Events.RowUpdated<GLTranDoc> e, PXRowUpdated baseHandler)
		{
			PXCache sender = e.Cache;
			GLTranDoc row = e.Row;
			GLTranDoc oldRow = e.OldRow;
			if (LocalizationServiceExtensions.LocalizationEnabled<FeaturesSet.canadianLocalization>(sender.Graph) &&
			    row.CuryTranAmt.Value != oldRow.CuryTranAmt.Value)
			{
				//To run TermsAttribute logic with correct tax values set
				sender.RaiseFieldUpdated<GLTranDoc.curyTranTotal>(row, row.CuryTranTotal.Value);
			}

			baseHandler(e.Cache, e.Args);
		}
	}
}
