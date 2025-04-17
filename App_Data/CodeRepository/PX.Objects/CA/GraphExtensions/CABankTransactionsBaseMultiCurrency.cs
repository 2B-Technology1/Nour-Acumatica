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
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.CA.MultiCurrency
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class CABankTransactionsBaseMultiCurrency<TGraph> : CAMultiCurrencyGraph<TGraph, CABankTran>
		where TGraph : PXGraph
	{
		#region SettingUp
		protected override string Module => GL.BatchModule.CA;
		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(CashAccount))
			{
				CuryID = typeof(CashAccount.curyID),
				CuryRateTypeID = typeof(CashAccount.curyRateTypeID)
			};
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(CABankTran))
			{
				CuryID = typeof(CABankTran.curyID),
				CuryInfoID = typeof(CABankTran.curyInfoID),
				BAccountID = typeof(CABankTran.cashAccountID),
				DocumentDate = typeof(CABankTran.matchingPaymentDate),
			};
		}
		#endregion

		#region Document
		protected override void DocumentRowInserting<CuryInfoID, CuryID>(PXCache sender, object row)
		{
			CABankTran tran = GetCABankTranFromDocument(row);
			if (tran.CanCreateCurrencyInfo())
			{
				base.DocumentRowInserting<CuryInfoID, CuryID>(sender, row);
			}
		}

		protected override void DocumentRowUpdating<CuryInfoID, CuryID>(PXCache sender, object row)
		{
			CABankTran tran = GetCABankTranFromDocument(row);
			if (tran.CanCreateCurrencyInfo())
			{
				long? key = (long?)sender.GetValue<CuryInfoID>(row);
				if (key != null && key.Value < 0L)
				{
					bool found = false;
					foreach (CurrencyInfo cached in currencyinfo.Cache.Inserted)
					{
						if (object.Equals(cached.CuryInfoID, key))
						{
							found = true;
							break;
						}
					}

					//when populatesavedvalues is called in ExecuteSelect we can sometimes endup here
					if (!found)
					{
						sender.SetValue<CuryInfoID>(row, null);
						key = null;
					}
				}

				if (key == null)
				{
					CurrencyInfo info = new CurrencyInfo();
					info = currencyinfo.Insert(info);
					currencyinfo.Cache.IsDirty = false;
					if (info != null)
					{
						sender.SetValueExt<CuryInfoID>(row, info.CuryInfoID);
						sender.SetValue<CuryID>(row, info.CuryID);
						defaultCurrencyRate(currencyinfo.Cache, info, true, true);
					}
				}
			}
		}
		#endregion

		#region CABankTran
		protected void _(Events.RowPersisting<CABankTran> e)
		{
			bool needPersistingCheck = e.Row?.CreateDocument == true || e.Row?.MultipleMatching == true;
			if (needPersistingCheck != true)
			{
				PXDBDefaultAttribute.SetPersistingCheck<CABankTran.curyInfoID>(e.Cache, e.Row, PXPersistingCheck.Nothing);
			}
		}
		#endregion

		#region Details
		protected void _(Events.RowInserting<CABankTranMatch> e)
		{
			InsertNewCurrencyInfoWithoutCuryID<CABankTranMatch.curyInfoID>(e.Cache, e.Row);
		}
		#endregion

		#region CurrencyInfo
		protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.baseCuryID> e)
		{
			e.NewValue = Base.Accessinfo.BaseCuryID;
			e.Cancel = true;
		}
		#endregion

		protected virtual void InsertNewCurrencyInfoWithoutCuryID<CuryInfoID>(PXCache sender, object row)
			where CuryInfoID : class, IBqlField
		{
			long? id = (long?)sender.GetValue<CuryInfoID>(row);
			CurrencyInfo info = new CurrencyInfo();
			info = currencyinfo.Insert(info);
			currencyinfo.Cache.IsDirty = false;
			if (info != null)
			{
				sender.SetValue<CuryInfoID>(row, info.CuryInfoID);
				CurrencyInfo orig = null;
				if (id != null)
				{
					orig = currencyinfobykey.Select(id);
					if (orig != null)
					{
						orig = (CurrencyInfo)currencyinfo.Cache.GetOriginal(orig);
					}
				}
				if (orig == null)
				{
					defaultCurrencyRate(currencyinfo.Cache, info, true, true);
				}
				else
				{
					id = info.CuryInfoID;
					currencyinfo.Cache.RestoreCopy(info, orig);
					info.CuryInfoID = id;
					currencyinfo.Cache.Remove(orig);
				}
			}
		}

		private static CABankTran GetCABankTranFromDocument(object row)
		{
			Document document = row as Document;
			CABankTran tran = document?.Base as CABankTran;
			return tran;
		}
	}
}
