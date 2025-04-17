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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{	
	public class BatchNbrSelectorAttribute : PXEntityAttribute
	{
		public BatchNbrSelectorAttribute(Type docTypeField)
			:base()
		{
			if(docTypeField != null)
			{
				Type SearchType = GetBatches(docTypeField);
				var attr = new PXSelectorAttribute(SearchType);
				_Attributes.Add(attr);
			}

			if (PXAccess.FeatureInstalled<CS.FeaturesSet.multipleBaseCurrencies>())
			{
				var restrictor = new PX.Objects.IN.Attributes.RestrictorWithParametersAttribute(typeof(Where<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>),
				Messages.BranchBaseCurrencyDifference,
				typeof(Branch.branchID), typeof(Current<AccessInfo.branchID>));
				_Attributes.Add(restrictor); 
			}
		}

		private static Type GetBatches(Type docTypeField)
		{
			return GetQueryCommand(docTypeField);
		}

		private static Type GetQueryCommand(Type docTypeField)
		{
			Type where = BqlCommand.Compose(typeof(Where<,>), typeof(AMBatch.docType), typeof(Equal<>), typeof(Optional<>), docTypeField);

			Type order = typeof(OrderBy<Desc<AMBatch.batNbr>>);

			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>())
			{
				Type join = typeof(LeftJoin<Branch, On<Branch.branchID, Equal<AMBatch.branchID>>>);
				where = BqlCommand.Compose(typeof(Where2<,>), where, typeof(And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>));
				return BqlCommand.Compose(typeof(Search2<,,,>), typeof(AMBatch.batNbr), join, where, order);
			}
			else
			{
				return BqlCommand.Compose(typeof(Search<,,>), typeof(AMBatch.batNbr), where, order);
			}

			
		}
	}
}
