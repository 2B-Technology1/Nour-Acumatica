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
using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.Localizations.CA.AP
{
	/// <summary>
	/// An extension for <see cref="APPayment"/> that is used to translate <see cref="APPayment.AmountToWords"/> to French.
	/// </summary>
	/// <see cref="APPayment" />
	public sealed class APPaymentExt : PXCacheExtension<APPayment>
    {
        #region IsActive
        
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

		#endregion

		/// <summary>
		/// The amount that is spelled out.
		/// </summary>
		[PXRemoveBaseAttribute(typeof(PX.Objects.AP.ToWordsAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [FrenchToWords(typeof(APPayment.curyOrigDocAmt))]		
        public string AmountToWords
        {
            get;
            set;
        }
    }
}
