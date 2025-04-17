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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.Common;
using System;

namespace PX.Objects.Extensions.MultiCurrency.CR
{
	public abstract class CRMultiCurrencyGraph<TGraph, TPrimary> : MultiCurrencyGraph<TGraph, TPrimary>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		protected override string Module => GL.BatchModule.CR;

		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(Customer));
		}

		public PXSelect<CRSetup> crSetup;

		public override void Initialize()
		{
			base.Initialize();

			if (BAccountField != null && DetailsView != null)
				Base.FieldVerifying.AddHandler(typeof(TPrimary), BAccountField.Name, BAccountID__FieldVerifying);
		}

		protected virtual Type BAccountField => null;

		protected virtual PXView DetailsView => null;

		protected virtual BAccount GetRelatedBAccount() => null;

		protected virtual CurySource GetFallbackMapping()
		{
			var setup    = crSetup.SelectSingle();
			var baccount = GetRelatedBAccount();
			return new CurySource
			{
				CuryID            = baccount?.CuryID,
				AllowOverrideCury = baccount?.AllowOverrideCury,
				// cury rate type not used for prospect
				CuryRateTypeID    = setup?.DefaultRateTypeID,
				AllowOverrideRate = setup?.AllowOverrideRate,
			};
		}

		protected override CurySource CurrentSourceSelect()
		{
			var curySource = base.CurrentSourceSelect();
			var fallback   = GetFallbackMapping();
			return new CurySource
			{
				CuryID            = curySource?.CuryID ?? fallback?.CuryID,
				AllowOverrideCury = curySource?.AllowOverrideCury ?? fallback?.AllowOverrideCury,
				CuryRateTypeID    = curySource?.CuryRateTypeID ?? fallback?.CuryRateTypeID,
				AllowOverrideRate = curySource?.AllowOverrideRate ?? fallback?.AllowOverrideRate,
			};
		}

		protected virtual void BAccountID__FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs args)
		{
			var oldValue = sender.GetValue(args.Row, BAccountField.Name);
			if (object.Equals(args.NewValue, oldValue))
				return;

			if (HasDetailRecords() is false)
				return;

			var newBAccount = BAccount.PK.Find(Base, args.NewValue as int?);
			if (newBAccount == null)
				return;

			var curyID = Documents.Cache.GetValue<Document.curyID>(args.Row) as string;
			if (newBAccount.AllowOverrideCury != true
			    && newBAccount.CuryID != curyID
			    && !string.IsNullOrEmpty(newBAccount.CuryID))
			{
				throw new PXSetPropertyException(PX.Objects.CR.Messages.BAccountChangedWithRestrictedCurrency)
				{
					ErrorValue = newBAccount.AcctCD,
				};
			}
		}

		protected override void _(Events.FieldUpdated<Document, Document.bAccountID> e)
		{
			if (e.ExternalCall || e.Row?.CuryID == null || Base.IsWithinContext)
			{
				SourceFieldUpdated<Document.curyInfoID, Document.curyID, Document.documentDate>(
					e.Cache, e.Row, resetCuryID: HasDetailRecords() is false);
			}
		}

		public virtual bool HasDetailRecords()
		{
			if (DetailsView == null)
				return false;

			var primaryCache = Base.Caches[typeof(TPrimary)];
			if (primaryCache.Current == null)
				return false;

			if (primaryCache.GetStatus(primaryCache.Current) == PXEntryStatus.Inserted)
			{
				return DetailsView.Cache.IsDirty;
			}
			else
			{
				return DetailsView.SelectSingle() != null;
			}
		}

	}
}
