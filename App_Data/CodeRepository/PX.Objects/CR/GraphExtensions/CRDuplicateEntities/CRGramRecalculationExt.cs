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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	public class CRGramRecalculationExt<TGraph> : PXGraphExtension<TGraph> where TGraph : PXGraph
	{
		protected static bool IsFeatureActive() => PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>();

		public SelectFrom<CRSetup>.View Setup;

		[PXOverride]
		public virtual void Persist(Action del)
		{
			// already asked
			if (Setup.View.Answer.IsPositive())
			{
				PXRedirectHelper.TryRedirect(PXGraph.CreateInstance<CRGrammProcess>(), PXRedirectHelper.WindowMode.Same);
			}

			var requiresGrammCalculation = RequiresGramRecalculation();

			del();

			if (!Base.IsImport && !Base.IsExport && requiresGrammCalculation)
			{
				Setup.View.Ask(
					row: null,
					header: "Warning",
					message: PXMessages.Localize(MessagesNoPrefix.WouldYouLikeToRecalculateRecords),
					buttons: MessageButtons.YesNo,
					icon: MessageIcon.Warning);
			}
		}

		protected virtual bool RequiresGramRecalculation()
		{
			return Base
				.Caches<CRValidation>()
				.Updated
				.OfType<CRValidation>()
				.Any(v => v.GramValidationDateTime is null);
		}
	}
}
