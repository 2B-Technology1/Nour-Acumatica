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

using PX.Api;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Commerce.Objects
{
	public class CommerceHelper : BaseHelper
	{
		#region Selects
		public PXSelectJoin<PX.Objects.AR.Customer, LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>,
						Where<PX.Objects.CR.Contact.eMail, Equal<Required<PX.Objects.CR.Contact.eMail>>>> CustomerByEmail;
		public PXSelectJoin<PX.Objects.AR.Customer, LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>,
						Where<PX.Objects.CR.Contact.phone1, Equal<Required<PX.Objects.CR.Contact.phone1>>, Or<PX.Objects.CR.Contact.phone2, Equal<Required<PX.Objects.CR.Contact.phone2>>>>> CustomerByPhone;
		public PXSelect<PX.Objects.AR.ARRegister, Where<PX.Objects.AR.ARRegister.externalRef, Equal<Required<PX.Objects.AR.ARRegister.externalRef>>>> PaymentByExternalRef;
		public PXSelect<PX.Objects.SO.SOOrder, Where<PX.Objects.SO.SOOrder.orderType, IsNotNull, And<PX.Objects.SO.SOOrder.orderType, In<Required<PX.Objects.SO.SOOrder.orderType>>, And<PX.Objects.SO.SOOrder.customerRefNbr, Equal<Required<PX.Objects.SO.SOOrder.customerRefNbr>>>>>> OrderByTypesAndCustomerRefNbr;
		#endregion

		#region SYSubstitution
		public PXSelect<SYSubstitutionValues,
			Where<SYSubstitutionValues.substitutionID, Like<Required<SYSubstitutionValues.substitutionID>>,
				And<SYSubstitutionValues.substitutedValue, Equal<Required<SYSubstitutionValues.substitutedValue>>>>> SubstituteLocal;
		public PXSelect<SYSubstitutionValues,
			Where<SYSubstitutionValues.substitutionID, Like<Required<SYSubstitutionValues.substitutionID>>,
				And<SYSubstitutionValues.originalValue, Equal<Required<SYSubstitutionValues.originalValue>>>>> SubstituteExtern;

		protected Dictionary<String, Dictionary<String, String>> substitutionLocalByExternal = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		protected Dictionary<String, Dictionary<String, String>> substitutionExternalByLocal = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Searches for <paramref name="externValue"/> in <paramref name="substitute"/>.<br/>
		/// If found, it adds the value to <see cref="CommerceHelper.substitutionLocalByExternal"/>.
		/// </summary>
		/// <param name="substitute">Name of the substitute list to search.</param>
		/// <param name="externValue">Extern value to be searched.</param>
		/// <param name="def">Default value.</param>
		/// <returns>The value found or the <paramref name="def"/> value.</returns>
		public virtual string GetSubstituteLocalByExtern(string substitute, string externValue, string def)
		{
			if (String.IsNullOrEmpty(substitute) || string.IsNullOrEmpty(externValue))
				return def;
			Dictionary<string, string> values;
			string valueSubstitution;
			if (!substitutionLocalByExternal.TryGetValue(substitute, out values))
				values = substitutionLocalByExternal[substitute] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			else if (values.TryGetValue(externValue, out valueSubstitution))
				return valueSubstitution;
			SYSubstitutionValues substituteValue = SubstituteExtern.Select(substitute, externValue);
			if (substituteValue == null)
			{
				_processor?.LogInfo(_processor.Operation.LogScope(), BCMessages.ThereIsNotSubstitution, externValue, substitute);
			}
			String value = substituteValue?.SubstitutedValue ?? def;
			if(!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(externValue)) //only put the value to Dictionary if it is not null, otherwise we will always get the null value from Dictionary
				substitutionLocalByExternal[substitute][externValue] = value;
			return value;
		}

		public virtual string GetSubstituteExternByLocal(string substitute, string localValue, string def)
		{
			if (String.IsNullOrEmpty(substitute) || string.IsNullOrEmpty(localValue))
				return def;
			Dictionary<string, string> values;
			string valueSubstitution;
			if (!substitutionExternalByLocal.TryGetValue(substitute, out values))
				values = substitutionExternalByLocal[substitute] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			else if (values.TryGetValue(localValue, out valueSubstitution))
				return valueSubstitution;
			SYSubstitutionValues substituteValue = SubstituteLocal.Select(substitute, localValue);
			String value = substituteValue?.OriginalValue ?? def;
			if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(localValue)) //only put the value to Dictionary if it is not null, otherwise we will always get the null value from Dictionary
				substitutionExternalByLocal[substitute][localValue] = value;
			return value;
		}
		#endregion

		#region Initialization

		protected HashSet<string> _taxCodes;
		public HashSet<string> TaxCodes
		{
			get
			{
				if (_taxCodes is null)
				{
					_taxCodes = new HashSet<string>();
					foreach (PX.Objects.TX.Tax tax in SelectFrom<PX.Objects.TX.Tax>.View.Select(this))
					{
						_taxCodes.Add(tax.TaxID);
					}
				}
				return _taxCodes;
			}
		}
		#endregion

		#region Cached Collections
		public virtual List<BCPaymentMethods> PaymentMethods() => BCPaymentMethodsMappingSlot.Get(Processor.Operation.Binding);
		public virtual List<BCShippingMappings> ShippingMethods() => BCShippingMethodsMappingSlot.Get(Processor.Operation.Binding);

		protected Dictionary<String, Dictionary<String, String>> _countryStates;
		public Dictionary<String, Dictionary<String, String>> CountryStates
		{
			get
			{
				if (_countryStates == null)
				{
					_countryStates = new Dictionary<string, Dictionary<string, string>>(System.StringComparer.OrdinalIgnoreCase);
					foreach (State row in PXSelect<State>.Select(this))
					{
						Dictionary<string, string> states;
						if (!_countryStates.TryGetValue(row.CountryID, out states))
							_countryStates[row.CountryID] = states = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

						if (!states.TryGetValue(row.StateID, out string state))
							states[row.StateID] = state = row.Name;
					}
				}
				return _countryStates;
			}
		}
		public string SearchStateID(String countryID, String stateName, String stateID)
		{
			if (countryID == null || stateID == null) return stateID;

			if (CountryStates.TryGetValue(countryID, out Dictionary<string, string> states))
			{
				if (states.ContainsKey(stateID)) return stateID;

				foreach(KeyValuePair<String, String> pair in states)
				{
					if (string.Equals(pair.Value, stateName, StringComparison.OrdinalIgnoreCase)) return pair.Key;
				}
			}
			return GetSubstituteLocalByExtern(BCSubstitute.GetValue(Processor.Operation.ConnectorType, BCSubstitute.State), stateName, stateID);
		}
		public string SearchStateName(String countryID, String stateID, String stateName)
		{
			if (countryID == null || stateID == null) return stateID;

			if (CountryStates.TryGetValue(countryID, out Dictionary<string, string> states))
			{
				if (states.TryGetValue(stateID, out String state))
				{
					return state;
				}
			}
			return GetSubstituteExternByLocal(BCSubstitute.GetValue(Processor.Operation.ConnectorType, BCSubstitute.State), stateID, stateName);
		}
		#endregion

		#region Location helper methods
		public virtual void DeactivateLocation(CustomerMaint graph, int? previousDefault, CustomerLocationMaint locGraph, BCSyncStatus value)
		{
			//Need to disable BAccountUpdate here to prevent multiple processes modifying BAccount at once.
			locGraph.GetExtension<BCLocationMaintExt>().SkipBAccountUpdate = true;
			locGraph.LocationCurrent.Current = PXSelect<PX.Objects.CR.Location, Where<PX.Objects.CR.Location.bAccountID, Equal<Required<PX.Objects.CR.Location.bAccountID>>, And<PX.Objects.CR.Location.noteID, Equal<Required<PX.Objects.CR.Location.noteID>>>>>.Select(locGraph, graph.BAccount.Current.BAccountID, value.LocalID);
			if (locGraph.LocationCurrent.Current != null)
			{
				// only if default location is set to inactive
				bool isDefaultLocationCurrentLocation = previousDefault == locGraph.LocationCurrent.Current.LocationID;
				locGraph.GetExtension<BCLocationMaintExt>().ClearCache = isDefaultLocationCurrentLocation;
				locGraph.ApplyWorkflowState(locGraph.LocationCurrent.Current);
				locGraph.LocationCurrent.Current.Status = LocationStatus.Inactive;
				locGraph.LocationCurrent.Update(locGraph.LocationCurrent.Current);
				locGraph.Actions.PressSave();
				//Need to enable BAccountUpdate here to maintain functionality in other parts of the code which require Location to update BAccount automatically.
				locGraph.GetExtension<BCLocationMaintExt>().SkipBAccountUpdate = false;
			}
		}

		/// <summary>
		/// Sets the default location according to <paramref name="defaultLocalId"/>.
		/// </summary>
		/// <param name="defaultLocalId"></param>
		/// <param name="locations">List to retrieve a <see cref="Location"/> according to <paramref name="defaultLocalId"/></param>
		/// <param name="graph">A <see cref="CustomerMaint"/> graph.</param>
		/// <param name="updated">Flag to indicate that the default location was updated.</param>
		/// <returns>The previous default <see cref="Location.LocationID"/>.</returns>
		public virtual int? SetDefaultLocation(Guid? defaultLocalId, List<Location> locations, CustomerMaint graph, ref bool updated)
		{
			var nextDefault = locations.FirstOrDefault(x => x.NoteID == defaultLocalId);
			var defLocationExt = graph.GetExtension<CustomerMaint.DefLocationExt>();
			var locationDetails = graph.GetExtension<CustomerMaint.LocationDetailsExt>();

			if (nextDefault == null && defaultLocalId != null)
			{
				locationDetails.Locations.Cache.Clear();
				locationDetails.Locations.Cache.ClearQueryCache();
				nextDefault = locationDetails.Locations.Select().RowCast<Location>()?.ToList()?.FirstOrDefault(x => x.NoteID == defaultLocalId);
			}
			var previousDefault = graph.BAccount.Current.DefLocationID;
			if (nextDefault == null	|| previousDefault == nextDefault?.LocationID) //if mapped default and defLocation are not in sync
				return previousDefault;

			defLocationExt.DefLocation.Current = nextDefault;
			if (defLocationExt.DefLocation.Current?.IsActive == false && _processor.GetEntity(_processor.Operation.EntityType).PrimarySystem == BCSyncSystemAttribute.External)
					{
						var locGraph = PXGraph.CreateInstance<PX.Objects.AR.CustomerLocationMaint>();

						locGraph.LocationCurrent.Current = locGraph.Location.Select(graph.BAccount.Current.BAccountID).FirstOrDefault(x => x.GetItem<PX.Objects.CR.Location>().NoteID == defLocationExt.DefLocation.Current.NoteID);
						locGraph.ApplyWorkflowState(locGraph.LocationCurrent.Current);
						locGraph.LocationCurrent.Current.Status = LocationStatus.Active;
						locGraph.LocationCurrent.Update(locGraph.LocationCurrent.Current);
						locGraph.Actions.PressSave();

						defLocationExt.DefLocation.Cache.Clear();
						defLocationExt.DefLocation.Cache.ClearQueryCache();
						defLocationExt.DefLocation.Current  = locationDetails.Locations.Select().RowCast<Location>()?.ToList()?.FirstOrDefault(x => x.NoteID == defaultLocalId);
					}
				updated = true;
				defLocationExt.SetDefaultLocation.Press();
				graph.Actions.PressSave();
			
			return previousDefault;
		}

		public virtual bool CompareStrings(string value1, string value2)
		{
			return string.Equals(value1?.Trim() ?? string.Empty, value2?.Trim() ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
		}

		public virtual DateTime? GetUpdatedDate(string customerID, DateTime? date)
		{
			List<PXDataField> fields = new List<PXDataField>();
			fields.Add(new PXDataField(nameof(BAccount.lastModifiedDateTime)));
			fields.Add(new PXDataFieldValue(nameof(BAccount.acctCD), customerID));
			using (PXDataRecord rec = PXDatabase.SelectSingle(typeof(BAccount), fields.ToArray()))
			{
				if (rec != null)
				{
					date = rec.GetDateTime(0);
					if (date != null)
					{
						date = PXTimeZoneInfo.ConvertTimeFromUtc(date.Value, LocaleInfo.GetTimeZone());

					}
				}
			}

			return date;
		}
		#endregion

		#region Taxes
		public virtual String DetermineTaxType(List<string> taxList)
        {
			return taxList.All(i => i.Contains(BCConstants.Dash)) ? BCObjectsConstants.BCAvalaraTaxName : null;
		}
		public virtual string ProcessTaxName(BCBindingExt currentBindingExt, string taxName, string taxType)
		{
			int taxLength = PX.Objects.TX.Tax.taxID.Length;

			taxName = taxName?.ToUpper();
			//Third parameter set to tax name in order to simplify process (if tax names are equal and user don't want to fill lists)
			string nameSubstitution = GetSubstituteLocalByExtern(currentBindingExt.TaxSubstitutionListID, taxName, taxName);
			if (string.IsNullOrEmpty(taxName)) throw new PXException(PX.Commerce.Objects.BCObjectsMessages.TaxNameDoesntExist);
			// If tax too long due to bad mapping or no mapping, but it's avalara
			if (nameSubstitution.Length > taxLength && taxType == BCObjectsConstants.BCAvalaraTaxName)
			{
				nameSubstitution = taxName.FieldsSplit(0, nameSubstitution, BCConstants.Dash);
				nameSubstitution = nameSubstitution.Contains(BCObjectsConstants.SpecialTax, StringComparison.InvariantCultureIgnoreCase) ? taxName.FieldsSplit(1, nameSubstitution, BCConstants.Dash) : nameSubstitution;
			}
			//If still name is too long, just cut off enough words
			if (nameSubstitution.Length > taxLength)
			{
				if(nameSubstitution.ElementAt(taxLength) == ' ')
					nameSubstitution = nameSubstitution.Substring(0, taxLength).TrimEnd(' ');
                else
                {
					string[] tokens = nameSubstitution.Split(' ');
					nameSubstitution = tokens[0].Length > taxLength ? tokens[0].Substring(0, taxLength) : tokens[0];
					for (int i = 1; i < tokens.Length; i++)
						if ((nameSubstitution + ' ' + tokens[i]).Length < taxLength) nameSubstitution += ' ' + tokens[i];
						else break;
				}

			}
			return nameSubstitution;
		}
		public virtual void ValidateTaxes(int? syncID, SalesOrder impl, SalesOrder local)
		{
			if (impl != null && (_processor.GetBindingExt<BCBindingExt>().TaxSynchronization == true) && (local.IsTaxValid?.Value == true))
			{
				String receivedTaxes = String.Join("; ", impl.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				_processor.LogInfo(_processor.Operation.LogScope(syncID), BCMessages.LogTaxesOnOrderReceived,
					impl.OrderNbr?.Value,
					impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(receivedTaxes) ? BCConstants.None : receivedTaxes);

				List<TaxDetail> sentTaxesToValidate = local?.TaxDetails?.ToList() ?? new List<TaxDetail>();
				List<TaxDetail> receivedTaxesToValidate = impl.TaxDetails?.ToList() ?? new List<TaxDetail>();
				//Validate Tax Zone
				if (sentTaxesToValidate.Count > 0 && impl.FinancialSettings.CustomerTaxZone.Value == null)
				{
					throw new PXException(BCObjectsMessages.CannotFindTaxZone,
						String.Join(", ", sentTaxesToValidate.Select(x => x.TaxID?.Value).Where(x => x != null).ToArray() ?? new String[] { BCConstants.None }));
				}
				//Validate tax codes and amounts
				List<TaxDetail> invalidSentTaxes = new List<TaxDetail>();
				foreach (TaxDetail sent in sentTaxesToValidate)
				{
					TaxDetail received = receivedTaxesToValidate.FirstOrDefault(x => String.Equals(x.TaxID?.Value, sent.TaxID?.Value, StringComparison.InvariantCultureIgnoreCase));
					if (received == null)
						_processor.LogInfo(_processor.Operation.LogScope(syncID), BCMessages.LogTaxesNotApplied,
						impl.OrderNbr?.Value,
						sent.TaxID?.Value);
					// This is the line to filter out the incoming taxes that has 0 value, thus if settings in AC are correct they wont be created as lines on SO
					if ((received == null && sent.TaxAmount.Value != 0)
						|| (received != null && !EqualWithRounding(sent.TaxAmount?.Value, received.TaxAmount?.Value)))
					{
						invalidSentTaxes.Add(sent);
					}

					if (received != null) receivedTaxesToValidate.Remove(received);
				}
				if (invalidSentTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxExt,
						String.Join(",", invalidSentTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
				List<TaxDetail> invalidReceivedTaxes = receivedTaxesToValidate.Where(x => (x.TaxAmount?.Value ?? 0m) == 0m && (x.TaxableAmount?.Value ?? 0m) == 0m).ToList();
				if (invalidReceivedTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxAcu,
						String.Join(",", invalidReceivedTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
			}
		}

		public virtual void LogTaxDetails(int? syncID, SalesOrder order)
		{
			//Logging for taxes
			if ((_processor.GetBindingExt<BCBindingExt>().TaxSynchronization == true) && (order.IsTaxValid?.Value == true))
			{
				String sentTaxes = String.Join("; ", order.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				_processor.LogInfo(_processor.Operation.LogScope(syncID), BCMessages.LogTaxesOnOrderSent,
					order.OrderNbr?.Value ?? BCConstants.None,
					order.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(sentTaxes) ? BCConstants.None : sentTaxes);
			}
		}
		public virtual string TrimAutomaticTaxNameForAvalara(string mappedTaxName)
		{
			return mappedTaxName.Split(new string[] { " - " }, StringSplitOptions.None).FirstOrDefault() ?? mappedTaxName;
		}

		#endregion

		#region Utilities
		public virtual bool EqualWithRounding(decimal? sent, decimal? received)
		{
			if (sent.HasValue && received.HasValue)
			{
				int countSent = BitConverter.GetBytes(decimal.GetBits(sent.Value)[3])[2];
				int countReceived = BitConverter.GetBytes(decimal.GetBits(received.Value)[3])[2];
				int maxPrecision = countSent > countReceived ? countReceived : countSent;
				return RoundToStoreSetting(sent.Value, maxPrecision) == RoundToStoreSetting(received.Value, maxPrecision);
			}
			return false;
		}

		[Obsolete]
		public virtual decimal? RoundToStoreSetting(decimal? price)
		{
			return RoundToStoreSetting(price, null);
		}

		/// <summary>
		/// Rounds the number according to the currency passed as a parameter.
		/// This method retrieve the currency information from the ERP and use its 
		/// </summary>
		/// <param name="price"></param>
		/// <param name="currencyCode"></param>
		/// <returns></returns>
		public virtual decimal? RoundToCurrencyPrecision(decimal? price, string currencyCode)
		{
			var currency = GetCurrencyInfo(currencyCode);
			int? maxPrecision = currency?.DecimalPlaces;
			return RoundToStoreSetting(price, maxPrecision);
		}

		public virtual decimal? RoundToStoreSetting(decimal? price, int? maxPrecision)
		{
			//For decimal value comparsion, if their precision are different, it will have a different result.
			//So we need to check their precision and use the max precision to make sure they are in the same precision level.
			//Example: precision is 4 for 0.4980 and precision is 2 for 0.50, we should use 2 as their both precisions to round the number.
			price = price != null ? Decimal.Round(price.Value,
				maxPrecision == null || maxPrecision > CommonSetupDecPl.PrcCst ? CommonSetupDecPl.PrcCst : maxPrecision.Value, MidpointRounding.AwayFromZero) : 0;
			return price;
		}

		/// <summary>
		/// The string is null or empty, it will be replaced by the following sort order string
		/// </summary>
		/// <param name="replaceIfNullOrEmpty">If true: the string will be replaced if it's null or empty; otherwise it will be replaced if it's null only</param>
		/// <param name="replaceStrByOrder">The string list by sort order</param>
		/// <returns>The frist string is not null or empty. If both of them are null or empty, it will return null</returns>
		public virtual string ReplaceStrByOrder(bool replaceIfNullOrEmpty, params string [] replaceStrByOrder)
		{
			if (replaceStrByOrder == null || replaceStrByOrder.Length == 0)
				return null;
			foreach(var str in replaceStrByOrder)
			{
				if (!string.IsNullOrEmpty(str) || (str != null && replaceIfNullOrEmpty == false))
					return str;
			}
			return null;
		}

		public virtual void GetExistingRefundPayment(Payment refundPayment, string docType, string reference)
		{
			ARPayment existinCRPayment = null;
			switch (docType)
			{
				case ARPaymentType.VoidPayment:
					{
						existinCRPayment = PXSelect<PX.Objects.AR.ARPayment,
												   Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
												   And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, ARPaymentType.VoidPayment, reference);
						if (existinCRPayment != null)
						{
							refundPayment.NoteID = existinCRPayment.NoteID.ValueField();
						}
						break;
					}
				case ARPaymentType.Refund:
					{
						existinCRPayment = PXSelect<PX.Objects.AR.ARPayment,
						   Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
						   And<ARRegister.externalRef, Equal<Required<ARRegister.externalRef>>>>>.Select(this, docType, reference);
						if (existinCRPayment != null)
						{
							refundPayment.NoteID = existinCRPayment.NoteID.ValueField();
						}
						else// if cannot find wit external ref search with transaction number
						{
							foreach (var crPayment in PXSelectJoin<CCProcTran, InnerJoin<ARPayment, On<CCProcTran.docType, Equal<ARPayment.docType>, And<CCProcTran.refNbr, Equal<ARPayment.refNbr>>>>,
									Where<CCProcTran.docType, Equal<Required<CCProcTran.docType>>,
									And<CCProcTran.tranType, Equal<CCTranTypeCode.credit>,
									And<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>>>>>.Select(this, docType, reference))
							{
								refundPayment.NoteID = crPayment?.GetItem<ARPayment>()?.NoteID?.ValueField();
								break;
							}
						}
						break;
					}
			}
		}

		/// <summary>
		/// Validates settings for credit card payment method and processing center configuration for all payment methods.
		/// </summary>
		/// <param name="bcPaymentMethod"></param>
		/// <exception cref="PXException"></exception>
		public virtual void IsCreditCardPaymentMethodValid(BCPaymentMethods bcPaymentMethod)
		{
			var resultSet = PXSelectJoin<
				PaymentMethod,
				LeftJoin<CCProcessingCenterPmntMethod,
					On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>,
					Where<PaymentMethod.paymentMethodID, Equal<Required<BCPaymentMethods.paymentMethodID>>>>
				.Select(this, bcPaymentMethod.PaymentMethodID)
				.ToArray();

			PaymentMethod paymentMethod = resultSet?.FirstOrDefault();

			if (paymentMethod?.PaymentType == PaymentMethodType.CreditCard)
			{
				//One possible error is dependent of this list's quantity.
				var listOfCCProcessingCenters = resultSet
													.RowCast<CCProcessingCenterPmntMethod>()
													.Where(processingCenter => processingCenter.IsActive == true);

				bool hasMatchingSelectedProcessingCenter = listOfCCProcessingCenters
															.Where(processingCenter => processingCenter.ProcessingCenterID == bcPaymentMethod.ProcessingCenterID)
															.Count() > 0;
				ARSetup arSetup = PXSelect<ARSetup>.Select(this);

				if (arSetup?.IntegratedCCProcessing != true)
				{
					throw new PXException(BCObjectsMessages.IntegratedCCProcessingSync, bcPaymentMethod.ProcessingCenterID, bcPaymentMethod.PaymentMethodID);
				}
				else if (paymentMethod?.ARIsProcessingRequired != true)
				{
					throw new PXException(BCObjectsMessages.MissingIntegratedCCProcessing, bcPaymentMethod.PaymentMethodID);
				}
				else if (!hasMatchingSelectedProcessingCenter)
				{
					string message = (listOfCCProcessingCenters.Count() > 0) ?
						BCObjectsMessages.MissingProcessingCenterCreditCardExists : BCObjectsMessages.MissingProcessingCenterCreditCard;

					throw new PXException(message, bcPaymentMethod.PaymentMethodID);
				}
			}
			else if (bcPaymentMethod.ProcessingCenterID != null)
			{
				throw new PXException(BCObjectsMessages.RemoveProcessingCenterNotCreditCard, bcPaymentMethod.PaymentMethodID);
			}
		}

		public virtual string FirstCharToUpper(string message)
		{
			if (string.IsNullOrEmpty(message)) return "";
			return message[0].ToString()?.ToUpper() + message.Substring(1);
		}
		#endregion

		#region UserMappings
		public string SOTypeImportMapping;
		public virtual void TryGetCustomOrderTypeMappings(ref List<string> orderTypes)
		{
			if (SOTypeImportMapping == null)
			{
				var allMappings = PXSelect<BCEntityImportMapping,
				Where<BCEntityImportMapping.connectorType, Equal<Required<BCEntity.connectorType>>,
				And<BCEntityImportMapping.bindingID, Equal<Required<BCEntity.bindingID>>,
				And<BCEntityImportMapping.entityType, Equal<BCEntitiesAttribute.order>>>>>.Select(this, _processor.Operation.ConnectorType, _processor.Operation.Binding).FirstTableItems;
				SOTypeImportMapping = allMappings?.FirstOrDefault(
					i => i.TargetObject == nameof(SalesOrder) && i.TargetField == nameof(SalesOrder.OrderType))?.SourceField ?? String.Empty;
			}
			Tuple<string, string>[] soTypes = MultipleOrderTypeAttribute.BCOrderTypeSlot.GetCachedOrderTypes().OrderTypes;

			if(!String.IsNullOrEmpty(SOTypeImportMapping))
				try
				{
					if (!SOTypeImportMapping.StartsWith("="))
						return;

					var constants = ECExpressionHelper.FormulaConstantValuesRetrieval(SOTypeImportMapping)?.Distinct();
					foreach (string constant in constants ?? new List<string>())
						if (constant.Length == 2 && !orderTypes.Contains(constant, StringComparer.InvariantCultureIgnoreCase) && soTypes.Any(i => String.Equals(i.Item1, constant, StringComparison.InvariantCultureIgnoreCase)))
							orderTypes.Add(constant.ToUpper());
				}
				catch (Exception ex) {
					_processor.LogError(_processor.Operation.LogScope(), BCMessages.OrderTypeMappingParseFailed, SOTypeImportMapping, ex);
				}
		}
		#endregion

		#region Specialized
		/// <summary>Sets external object's custom field value.</summary>
		///	<param name="entity">Represents a bucket of the object's synchronization process.</param>
		///	<param name="customFieldInfo">An external custom field.</param>
		///	<param name="targetData">An identifier of target field.</param>
		///	<param name="targetObject">A target object that contains <paramref name="targetField"/>.</param>
		///	<param name="targetField">A target field.</param>
		///	<param name="value">A value to apply.</param>
		public virtual string ClearHTMLContent(string html)
		{
			String result = html;

			if (result != null)
			{
				HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
				doc.LoadHtml(result);

				result = doc.DocumentNode?.SelectSingleNode("//body")?.InnerHtml ?? result;
			}

			return result;
		}
		#endregion

		#region "ERP Currencies"

		/// <summary>
		/// Get currency information from ERP.
		/// </summary>
		/// <param name="currencyCode"></param>
		/// <returns></returns>
		public virtual CurrencyList GetCurrencyInfo(string currencyCode)
		{
			if (currencyCode == null) return null;
			var currency = PX.Objects.CM.CurrencyList.PK.Find(this, currencyCode.ToUpper());
			return currency;
		}

		#endregion

	}
}
