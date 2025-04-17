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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Exceptions
{
	public class FeatureIsDisabledException<TFeature> : PXException
		where TFeature : IBqlField
	{
		public FeatureIsDisabledException()
			: base(Messages.TheFeatureIsDisabled, GetFeatureName(typeof(TFeature)))
		{
		}

		public FeatureIsDisabledException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		protected static string GetFeatureName(Type featureField)
		{
			var featureProperty = BqlCommand.GetItemType(featureField)?.GetProperties()
				.Where(p => p.Name.Equals(featureField.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

			var featureAttribute = featureProperty?.GetCustomAttributes(typeof(FeatureAttribute), true)
				.FirstOrDefault() as FeatureAttribute;

			return featureAttribute?.DisplayName ?? featureProperty?.Name ?? featureField.Name;
		}
	}
}
