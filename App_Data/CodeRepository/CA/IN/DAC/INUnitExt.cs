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
using PX.Objects.IN;
using PX.Objects.Localizations.CA.CS;

namespace PX.Objects.Localizations.CA.IN
{
	/// <summary>
	/// A cache extention for <see cref="INUnit"/> that is used to synchronize localizable units of measurement between <see cref="INUnit"/> and <see cref="UnitOfMeasure"/>.
	/// </summary>
	// Override the primary graph to set the default page to 10CS2001 
	[PXPrimaryGraph(typeof(UnitOfMeasureMaint))]
    public sealed class INUnitExt : PXCacheExtension<INUnit>
    {
        #region IsActive
        
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

		#endregion

		/// <summary>
		/// The field that adds the <see cref="MultilingualUnitOfMeasure"/> attribute to <see cref="INUnit.FromUnit"/>.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Append)]
        [MultilingualUnitOfMeasure]
        public string FromUnit
        {
            get;
            set;
        }
    }
}
