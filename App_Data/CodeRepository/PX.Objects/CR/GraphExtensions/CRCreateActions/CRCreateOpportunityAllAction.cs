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
using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public class CRCreateOpportunityAllAction<TGraph, TMaster, TOpportunityExt, TAccountExt, TContactExt>
		: PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TOpportunityExt : CRCreateOpportunityAction<TGraph, TMaster>
		where TAccountExt : CRCreateAccountAction<TGraph, TMaster>
		where TContactExt : CRCreateContactAction<TGraph, TMaster>
		where TMaster : class, IBqlTable, new()
	{
		public TOpportunityExt OpportunityExt { get; private set; }
		public TAccountExt AccountExt { get; private set; }
		public TContactExt ContactExt { get; private set; }


		public override void Initialize()
		{
			base.Initialize();

			OpportunityExt = Base.GetExtension<TOpportunityExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TOpportunityExt).Name);
			AccountExt = Base.GetExtension<TAccountExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TAccountExt).Name);
			ContactExt = Base.GetExtension<TContactExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TContactExt).Name);
		}

		public PXAction<TMaster> ConvertToOpportunityAll;
		[PXUIField(DisplayName = Messages.CreateOpportunity, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable convertToOpportunityAll(PXAdapter adapter)
		{
			if (AccountExt.NeedToUse)
			{
				var existingContact = ContactExt.ExistingContact.SelectSingle();
				var existingAccount = AccountExt.ExistingAccount.SelectSingle();

				if (existingContact?.BAccountID != null && existingAccount == null)
				{
					AccountExt.Documents.Cache.SetValue<Document.bAccountID>(AccountExt.Documents.Current, existingContact.BAccountID);

					Base.Caches<TMaster>().Update(AccountExt.GetMainCurrent());
				}
			}

			var validators = new List<CRPopupValidator>();

			if (ContactExt.NeedToUse)
			{
				validators.Add(ContactExt.PopupValidator);
			}
			if (AccountExt.NeedToUse)
			{
				validators.Add(AccountExt.PopupValidator);
			}

			if (OpportunityExt.AskExtConvert(out bool redirect, validators.ToArray()))
			{
				var processingGraph = Base.CloneGraphState();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateOpportunityAllAction<TGraph, TMaster, TOpportunityExt, TAccountExt, TContactExt>>();

					extension.ContactExt.NeedToUse = this.ContactExt.NeedToUse;
					extension.AccountExt.NeedToUse = this.AccountExt.NeedToUse;

					extension.DoConvert(redirect);
				});
			}

			return adapter.Get();
		}

		public virtual void DoConvert(bool redirect)
		{
			ConversionResult<CROpportunity> opptyResult;
			ConversionResult<BAccount> baccountResult = null;
			using (var ts = new PXTransactionScope())
			{
				// to save original value because it would be changed during contact/account creation
				var overrideContact =
					OpportunityExt.Documents.Cache.GetValueOriginal<Document.overrideRefContact>(OpportunityExt.Documents.Current) as bool?;

				if (AccountExt.NeedToUse)
					baccountResult = AccountExt.Convert(new AccountConversionOptions
					{
						PreserveCachedRecordsFilters =
						{
							ContactExt.PopupValidator,
							OpportunityExt.PopupValidator
						}
					});

				if (ContactExt.NeedToUse)
					ContactExt.Convert(new ContactConversionOptions
					{
						GraphWithRelation = baccountResult?.Graph,
						PreserveCachedRecordsFilters =
						{
							OpportunityExt.PopupValidator
						}
					});

				opptyResult = OpportunityExt.Convert(new OpportunityConversionOptions
				{
					ForceOverrideContact = overrideContact,
				});

				if (OpportunityExt.Documents.Current.RefContactID == null)
					throw new PXException(Messages.CannotCreateOpportunity);

				ts.Complete();
			}

			if (redirect)
				OpportunityExt.Redirect(opptyResult);
		}

	}
}
