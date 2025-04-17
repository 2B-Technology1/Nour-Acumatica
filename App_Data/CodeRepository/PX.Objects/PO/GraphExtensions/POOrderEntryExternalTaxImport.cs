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
using System.Diagnostics;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.TX;
using PX.TaxProvider;
using PX.Objects.Common.Extensions;
using PX.Data.BQL;
using PX.Objects.CM;

namespace PX.Objects.PO
{
	public class POOrderEntryExternalTaxImport : PXGraphExtension<POOrderEntryExternalTax, POOrderEntry>
	{
		[PXVirtualDAC]
		public PXFilter<POTaxTranImported> ImportedTaxes;

		public override void Initialize()
		{
			base.Initialize();
			typeof(PX.Data.MassProcess.FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
		}

		#region SOTaxTran Events
		protected virtual void _(Events.RowInserting<POTaxTran> e)
		{
			if (e.Row == null) return;

			POTaxTran taxTran = e.Row as POTaxTran;
			var soorder = Base.Document.Current;

			if (e.ExternalCall == true && soorder != null && soorder.ExternalTaxesImportInProgress == true && e.Cache.Graph.IsContractBasedAPI)
			{
				POTaxTranImported importedTaxTran = (POTaxTranImported)ImportedTaxes.Cache.CreateInstance();
				Base.Taxes.Cache.RestoreCopy(importedTaxTran, taxTran);

				ImportedTaxes.Insert(importedTaxTran);

				//Delete exisitng taxes when trying to update tax line via API (when there is not enough data provided to select the correct tax line (partial key and/or no record id)).
				foreach (POTaxTran tax in Base.Taxes.Select())
				{
					if (Base.Taxes.Cache.GetStatus(tax) == PXEntryStatus.Notchanged &&
						string.Equals(tax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
					{
						Base.Taxes.Delete(tax);
					}
				}

				//Do not insert tax if it has already been inserted automatically
				//TaxAmount and TaxableAmount will be updated later
				foreach (POTaxTran tax in Base.Taxes.Cache.Inserted)
				{
					if (string.Equals(tax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
					{
						e.Cancel = true;
						break;
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<POTaxTran, POTaxTran.taxID> e)
		{
			if (e.Row == null)
				return;

			var soorder = Base.Document.Current;
			var taxZone = Base.taxzone.Current;

			if (soorder != null && taxZone != null && soorder.ExternalTaxesImportInProgress == true && taxZone.IsExternal == true)
			{
				e.Cancel = true;
			}
		}
		#endregion

		[PXOverride]
		public virtual void InsertImportedTaxes()
		{
			POOrder order = Base.Document.Current;

			if (order != null && order.ExternalTaxesImportInProgress == true &&
				Base.IsContractBasedAPI && !Base1.skipExternalTaxCalcOnSave && Base.Document.Current != null)
			{
				try
				{
					if (ImportedTaxes.Cache.Inserted.Any_() != true)
					//When no taxes imported and IsTaxValid (ExternalTaxesImportInProgress) = true - delete all internally calculated taxes
					{
						TaxBaseAttribute.SetTaxCalc<POLine.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

						PXResultset<POTaxTran> taxTransactions = Base.Taxes.Select();
						foreach (POTaxTran taxTran in taxTransactions)
						{
							Base.Taxes.Delete(taxTran);
						}
					}
					else
					{
						TaxZone taxZone = Base.taxzone.Current;
						bool isExternalTaxZone = Base.taxzone.Current != null && Base.taxzone.Current.IsExternal == true;

						GetTaxResult result = new GetTaxResult();
						List<PX.TaxProvider.TaxLine> taxLines = new List<PX.TaxProvider.TaxLine>();
						List<PX.TaxProvider.TaxDetail> taxDetails = new List<PX.TaxProvider.TaxDetail>();
						decimal totalTaxAmount = 0m;

						if (isExternalTaxZone)
						//IsTaxValid (ExternalTaxesImportInProgress) = true + ExternalTaxZone = true scenario.
						//Imported taxes will be inserted as-is, ignoring all internal tax calculation rules.
						{
							foreach (POTaxTranImported taxTran in ImportedTaxes.Cache.Inserted)
							{
								decimal taxableAmount = taxTran.CuryTaxableAmt ?? 0m;
								decimal taxAmount = taxTran.CuryTaxAmt ?? 0m;
								decimal rate = !taxTran.TaxRate.IsNullOrZero() ? (taxTran.TaxRate ?? 0m) :
										(taxTran.CuryTaxableAmt.IsNullOrZero() ? 0m :
										Decimal.Round((taxTran.CuryTaxAmt ?? 0m) / (taxTran.CuryTaxableAmt ?? 1m), 6));

								PX.TaxProvider.TaxDetail taxDetail = new TaxProvider.TaxDetail
								{
									TaxName = taxTran.TaxID,
									TaxableAmount = taxableAmount,
									TaxAmount = taxAmount,
									Rate = rate
								};

								totalTaxAmount += taxTran.CuryTaxAmt ?? 0m;

								taxDetails.Add(taxDetail);
							}
							result.TaxSummary = taxDetails.ToArray();
							result.TotalTaxAmount = totalTaxAmount;

							ImportedTaxes.Cache.Clear();

							using (new PXTimeStampScope(null))
							{
								Base1.ApplyExternalTaxes(order, result, result);
							}
						}
						else
						//IsTaxValid(ExternalTaxesImportInProgress) = true + ExternalTaxZone = false scenario.
						//Taxable and tax amounts on internally calculated taxes will be updated with imported values.
						//Taxes calculated internally, but not present in the list of imported taxes, will be deleted.
						//Exception will be thrown in case any of imported taxes was not inserted properly.
						{
							List<KeyValuePair<string, Dictionary<string, string>>> errors = new List<KeyValuePair<string, Dictionary<string, string>>>();
							foreach (POTaxTran tax in Base.Taxes.Cache.Cached)
							{
								PXEntryStatus status = Base.Taxes.Cache.GetStatus(tax);
								Dictionary<string, string> lineErrors = PXUIFieldAttribute.GetErrors(Base.Taxes.Cache, tax);
								if (lineErrors.Count != 0)
									errors.Add(new KeyValuePair<string, Dictionary<string, string>>(tax.TaxID, lineErrors));
							}
							if (errors.Any())
							{
								string errorMessage = string.Empty;
								foreach (KeyValuePair<string, Dictionary<string, string>> error in errors)
								{
									errorMessage += string.Format(SO.Messages.TaxWasNotImported, error.Key, error.Value.Select(x => x.Value).Aggregate((e1, e2) => e1 + "; " + e2)) + " ";
								}
								throw new PXException(errorMessage);
							}

							TaxBaseAttribute.SetTaxCalc<POLine.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

							PXResultset<POTaxTran> taxTransactions = Base.Taxes.Select();

							//Delete all invalid taxes first and update amounts of valid taxes later
							foreach (POTaxTran taxTran in taxTransactions)
							{
								POTaxTranImported matchingTax = GetMatchingTax(taxTran);

								if (matchingTax == null)
								{
									Base.Taxes.Delete(taxTran);
								}
							}

							foreach (POTaxTran taxTran in taxTransactions)
							{
								POTaxTranImported matchingTax = GetMatchingTax(taxTran);

								if (matchingTax != null)
								{
									if (matchingTax.CuryTaxableAmt != null)
										taxTran.CuryTaxableAmt = matchingTax.CuryTaxableAmt;
									if (matchingTax.CuryTaxAmt != null)
										taxTran.CuryTaxAmt = matchingTax.CuryTaxAmt;
									Base.Taxes.Update(taxTran);
								}
							}
						}
					}
				}
				finally
				{
					ImportedTaxes.Cache.Clear();
				}
			}
		}

		public virtual POTaxTranImported GetMatchingTax(POTaxTran taxTran)
		{
			POTaxTranImported matchingTax = null;
			foreach (POTaxTranImported importedTax in ImportedTaxes.Cache.Inserted)
			{
				if (string.Equals(importedTax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
				{
					matchingTax = importedTax;
				}
			}

			return matchingTax;
		}
	}

	// Acuminator disable once PX1094 NoPXHiddenOrPXCacheNameOnDac [Works the same as in SO]
	[System.SerializableAttribute()]
	[PXVirtual]
	[PXBreakInheritance]
	public partial class POTaxTranImported : POTaxTran
	{
		#region Keys
		public new static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POTaxTranImported>.By<orderType, orderNbr> { }
		}
		#endregion

		#region OrderType
		public new abstract class orderType : BqlString.Field<orderType> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDBDefault(typeof(POOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Enabled = false, Visible = false)]
		public override string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public new abstract class orderNbr : BqlString.Field<orderNbr> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXDBDefault(typeof(POOrder.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false, Visible = false)]
		public override string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public new abstract class lineNbr : BqlInt.Field<lineNbr> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBInt(IsKey = true)]
		[PXDefault(int.MaxValue)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.Order))]
		public override int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true, ValidateValue = false)]
		public override String TaxID { get; set; }
		#endregion
		#region CuryInfoID
		[PXDBLong]
		public override Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public new Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
	}
}
