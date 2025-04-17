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
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;

namespace PX.Objects.IN.GraphExtensions.KitAssemblyEntryExt
{
	public class ShiftedPeriodsExt : ShiftedPeriodsExt<KitAssemblyEntry, INKitRegister, INKitRegister.tranDate, INKitRegister.tranPeriodID, INComponentTran>
	{
		public PXSelectExtension<DocumentLine> Overheads;

		public override void Initialize()
		{
			base.Initialize();
			Documents = new PXSelectExtension<Document>(Base.Document);
			Lines = new PXSelectExtension<DocumentLine>(Base.Components);
			Overheads = new PXSelectExtension<DocumentLine>(Base.Overhead);
		}

		protected virtual DocumentLineMapping GetDocumentOverheadMapping()
		{
			return new DocumentLineMapping(typeof(INOverheadTran));
		}

		protected override void _(Events.RowUpdated<Document> e)
		{
			base._(e);

			if (ShouldUpdateOverheadsOnDocumentUpdated(e))
			{
				foreach (DocumentLine overhead in Overheads.Select())
				{
					Overheads.Cache.SetDefaultExt<DocumentLine.finPeriodID>(overhead);

					Overheads.Cache.MarkUpdated(overhead, assertError: true);
				}
			}
		}

		protected virtual bool ShouldUpdateOverheadsOnDocumentUpdated(Events.RowUpdated<Document> e)
		{
			return ShouldUpdateDetailsOnDocumentUpdated(e);
		}
	}
}
