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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.TX;
using PX.TaxProvider;
using PX.Objects.Common.Extensions;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	public class ARInvoiceEntryExternalTaxImport : PXGraphExtension<ARInvoiceEntryExternalTax, ARInvoiceEntry>
	{
		[PXVirtualDAC]
		public PXFilter<ARTaxTranImported> ImportedTaxes;

		public override void Initialize()
		{
			base.Initialize();
			typeof(PX.Data.MassProcess.FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
		}

		#region APTaxTran Events
		protected virtual void _(Events.RowInserting<ARTaxTran> e)
		{
			if (e.Row == null) return;
			if (e.Cache.Current == null && e.Cache.Graph.IsImport)
			{
				e.Cache.Current = e.Cache.Inserted.FirstOrDefault_();
			}

			ARTaxTran taxTran = e.Row as ARTaxTran;
			var invoice = Base.Document.Current;

			if (e.ExternalCall == true && invoice != null && invoice.ExternalTaxesImportInProgress == true && e.Cache.Graph.IsContractBasedAPI)
			{
				ARTaxTranImported importedTaxTran = (ARTaxTranImported)ImportedTaxes.Cache.CreateInstance();
				Base.Taxes.Cache.RestoreCopy(importedTaxTran, taxTran);

				ImportedTaxes.Insert(importedTaxTran);

				//Delete exisitng taxes when trying to update tax line via API (when not enough data provided to select the correct tax line (partial key and/or no record id)).
				foreach (ARTaxTran tax in Base.Taxes.Select())
				{
					if (Base.Taxes.Cache.GetStatus(tax) == PXEntryStatus.Notchanged &&
						string.Equals(tax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
					{
						Base.Taxes.Delete(tax);
					}
				}

				//Do not insert tax if it has already been inserted automatically
				//TaxAmount and TaxableAmount will be updated later
				foreach (ARTaxTran tax in Base.Taxes.Cache.Inserted)
				{
					if (string.Equals(tax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
					{
						e.Cancel = true;
						break;
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<ARTaxTran, ARTaxTran.taxID> e)
		{
			if (e.Row == null)
				return;

			var invoice = Base.Document.Current;
			var taxZone = Base.taxzone.Current;

			if (invoice != null && taxZone != null && invoice.ExternalTaxesImportInProgress == true && taxZone.IsExternal == true)
			{
				e.Cancel = true;
			}
		}
		#endregion

		[PXOverride]
		public virtual void InsertImportedTaxes()
		{
			ARInvoice invoice = Base.Document.Current;

			if (invoice != null && invoice.ExternalTaxesImportInProgress == true &&
				Base.IsContractBasedAPI && !Base1.skipExternalTaxCalcOnSave && Base.Document.Current != null)
			{
				try
				{
					if (ImportedTaxes.Cache.Inserted.Any_() != true)
					//When no taxes imported and IsTaxValid (ExternalTaxesImportInProgress) = true - delete all internally calculated taxes
					{
						TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

						PXResultset<ARTaxTran> taxTransactions = Base.Taxes.Select();
						foreach (ARTaxTran taxTran in taxTransactions)
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
							Sign sign = Base1.GetDocumentSign(invoice);

							foreach (ARTaxTranImported taxTran in ImportedTaxes.Cache.Inserted)
							{
								decimal taxableAmount = sign * taxTran.CuryTaxableAmt ?? 0m;
								decimal taxAmount = sign * taxTran.CuryTaxAmt ?? 0m;
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

								if (taxTran.LineNbr == 32000)
								{
									PX.TaxProvider.TaxLine taxLine = new TaxProvider.TaxLine
									{
										Index = short.MinValue,
										TaxableAmount = taxableAmount,
										TaxAmount = taxAmount,
										Rate = rate
									};
									taxLines.Add(taxLine);
								}

								totalTaxAmount += taxTran.CuryTaxAmt ?? 0m;

								taxDetails.Add(taxDetail);
							}
							result.TaxSummary = taxDetails.ToArray();
							result.TotalTaxAmount = sign * totalTaxAmount;

							ImportedTaxes.Cache.Clear();

							using (new PXTimeStampScope(null))
							{
								Base1.ApplyTax(invoice, result);
							}
						}
						else
						//IsTaxValid(ExternalTaxesImportInProgress) = true + ExternalTaxZone = false scenario.
						//Taxable and tax amounts on internally calculated taxes will be updated with imported values.
						//Taxes calculated internally, but not present in the list of imported taxes, will be deleted.
						//Exception will be thrown in case any of imported taxes was not inserted properly.
						{
							List<KeyValuePair<string, Dictionary<string, string>>> errors = new List<KeyValuePair<string, Dictionary<string, string>>>();
							foreach (ARTaxTran tax in Base.Taxes.Cache.Cached)
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

							TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

							PXResultset<ARTaxTran> taxTransactions = Base.Taxes.Select();

							//Delete all invalid taxes first and update amounts of valid taxes later
							foreach (ARTaxTran taxTran in taxTransactions)
							{
								ARTaxTranImported matchingTax = GetMatchingTax(taxTran);

								if (matchingTax == null)
								{
									Base.Taxes.Delete(taxTran);
								}
							}

							foreach (ARTaxTran taxTran in taxTransactions)
							{
								ARTaxTranImported matchingTax = GetMatchingTax(taxTran);

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

		public virtual ARTaxTranImported GetMatchingTax(ARTaxTran taxTran)
		{
			ARTaxTranImported matchingTax = null;
			foreach (ARTaxTranImported importedTax in ImportedTaxes.Cache.Inserted)
			{
				if (string.Equals(importedTax.TaxID, taxTran.TaxID, StringComparison.OrdinalIgnoreCase))
				{
					matchingTax = importedTax;
				}
			}

			return matchingTax;
		}
	}

	[System.SerializableAttribute()]
	[PXVirtual]
	[PXBreakInheritance]
	[PXHidden]
	public partial class ARTaxTranImported : ARTaxTran
	{
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<TaxTran.tranType>>, And<ARRegister.refNbr, Equal<Current<TaxTran.refNbr>>>>>))]
		[PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
		public override String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false)]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(ARRegister.docDate))]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
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
		[PXDBCurrency(typeof(ARTaxTran.curyInfoID), typeof(ARTaxTran.taxableAmt))]
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
