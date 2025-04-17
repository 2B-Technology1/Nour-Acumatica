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
using System.Text;
using PX.AddressValidator;
using PX.Data;
using IAddressBase = PX.CS.Contracts.Interfaces.IAddressBase;

namespace PX.Objects.CS
{
	public static class PXAddressValidator
	{
		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous)
			where T : IAddressBase, IValidatedAddress
		{
			return Validate(aGraph, aAddress, aSynchronous, false);
		}

		public static void Validate<T>(PXGraph aGraph, List<T> aAddresses, bool aSynchronous, bool updateToValidAddress)
			where T : IAddressBase, IValidatedAddress
		{
			foreach (T it in aAddresses)
			{
				Validate<T>(aGraph, it, aSynchronous, updateToValidAddress);
			}
		}

		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous, bool updateToValidAddress)
			where T : IAddressBase, IValidatedAddress
		{
			return Validate<T>(aGraph, aAddress, aSynchronous, updateToValidAddress, false);
		}

		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous, bool updateToValidAddress, bool forceOverride)
			where T : IAddressBase, IValidatedAddress
		{
			return Validate<T>(aGraph, aAddress, aSynchronous, updateToValidAddress, forceOverride, out _);
		}

		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous, bool updateToValidAddress, bool forceOverride,
			out IList<(string fieldName, string fieldValue, string warningMessage)> warnings) where T : IAddressBase, IValidatedAddress
		{
			warnings = new List<(string fieldName, string fieldValue, string warningMessage)>();
			if (!AddressValidatorPluginMaint.IsActive(aGraph, aAddress.CountryID))
			{
				if (aSynchronous)
				{
					PXCache cache = aGraph.Caches[aAddress.GetType()];
					string countryFieldName = nameof(IAddressBase.CountryID);
					string warning = PXMessages.LocalizeFormatNoPrefix(Messages.AddressVerificationServiceIsNotSetup, aAddress.CountryID);

					cache.RaiseExceptionHandling(countryFieldName, aAddress, aAddress.CountryID,
						new PXSetPropertyException(warning, PXErrorLevel.Warning));
					warnings.Add((countryFieldName, aAddress.CountryID, warning));

					return false;
				}
				else
				{
					throw new PXException(Messages.AddressVerificationServiceIsNotSetup, aAddress.CountryID);
				}
			}

			bool isValid = true;

			Country country = Country.PK.Find(aGraph, aAddress.CountryID);
			updateToValidAddress = updateToValidAddress && (country.AutoOverrideAddress == true || forceOverride);

			IAddressValidator service = AddressValidatorPluginMaint.CreateAddressValidator(aGraph, country.AddressValidatorPluginID);
			if (service != null)
			{
				PXCache cache = aGraph.Caches[aAddress.GetType()];
				
				Dictionary<string, string> messages = new Dictionary<string, string>();

				T validAddress = (T)cache.CreateCopy(aAddress);

				try
				{
					isValid = service.ValidateAddress(validAddress, messages);
				}
				catch
				{
					throw new PXException(Messages.UnknownErrorOnAddressValidation);
				}

				if (isValid)
				{
					string[] fields =
					{
						nameof(IAddressBase.AddressLine1),
						nameof(IAddressBase.AddressLine2),
						//nameof(IAddressBase.AddressLine3),
						nameof(IAddressBase.City),
						nameof(IAddressBase.State),
						nameof(IAddressBase.PostalCode)
					};

					var modifiedFields = fields.Select(field => new
					{
						Field = field,
						OriginalValue = ((string)cache.GetValue(aAddress, field)) ?? string.Empty,
						ValidValue = ((string)cache.GetValue(validAddress, field)) ?? string.Empty
					}).Where(x => string.Compare(x.OriginalValue.Trim(), x.ValidValue.Trim(), StringComparison.OrdinalIgnoreCase) != 0).ToArray();

					if (aSynchronous && !updateToValidAddress && modifiedFields.Any())
					{
						var fieldsShouldBeEqual = new HashSet<string>()
						{
							nameof(IAddressBase.PostalCode)
						};

						foreach(var m in modifiedFields)
						{
							string warning = PXMessages.LocalizeFormatNoPrefix(Messages.AddressVerificationServiceReturnsField, m.ValidValue);
							cache.RaiseExceptionHandling(m.Field, aAddress, m.OriginalValue,
									new PXSetPropertyException(warning, PXErrorLevel.Warning));
							warnings.Add((m.Field, m.OriginalValue, warning));

							if (fieldsShouldBeEqual.Contains(m.Field))
								isValid = false;
						}
					}

					if (isValid)
					{
						T copyToUpdate = (T)cache.CreateCopy(aAddress);
						Action<T> raiseWarnings = null;
						if (aSynchronous && updateToValidAddress)
						{
							foreach (var m in modifiedFields)
							{
								var validValue = m.ValidValue == string.Empty ? null : m.ValidValue;
								cache.SetValue(copyToUpdate, m.Field, validValue);
								string warning = PXMessages.LocalizeFormatNoPrefix(Messages.AddressVerificationServiceReplaceValue, m.OriginalValue);

								raiseWarnings += (a) => cache.RaiseExceptionHandling(m.Field, a, validValue,
									new PXSetPropertyException(warning, PXErrorLevel.Warning));
								warnings.Add((m.Field, validValue, warning));
							}
						}
						copyToUpdate.IsValidated = true;
						aAddress = (T)cache.Update(copyToUpdate);
						raiseWarnings?.Invoke(aAddress);
					}
				}
				else
				{
					string message = string.Empty;
					StringBuilder messageBuilder = new StringBuilder();
					int count = 0;
					foreach (var iMsg in messages)
					{
						string error = iMsg.Value;
						if (!aSynchronous)
						{
							if (count > 0) messageBuilder.Append(",");
							messageBuilder.AppendFormat("{0}:{1}", iMsg.Key, error);
							count++;
						}
						else
						{
							object value = cache.GetValue(aAddress, iMsg.Key);
							cache.RaiseExceptionHandling(iMsg.Key, aAddress, value, new PXSetPropertyException(error));
							warnings.Add((iMsg.Key, (string)value, error));
						}
					}
					if (!aSynchronous)
					{
						throw new PXException(messageBuilder.ToString());
					}
				}
			}
			return isValid;
		}

		public static bool IsValidateRequired<T>(PXGraph aGraph, T aAddress)
			where T : IAddressBase
		{
			if (!string.IsNullOrEmpty(aAddress?.CountryID))
			{
				CS.Country country = CS.Country.PK.Find(aGraph, aAddress.CountryID);
				return country?.AddressValidatorPluginID != null;
			}

			return false;
		}

		public static string FormatWarningMessage(List<string> warnings)
		{
			if (warnings == null)
				return string.Empty;

			StringBuilder stringBuilder = new StringBuilder();

			for (int i = 0; i < warnings.Count; i++)
			{
				stringBuilder.AppendLine(warnings[i]);

				if (i < warnings.Count - 1)
					stringBuilder.AppendLine();
			}

			if (warnings.Count == 0)
				stringBuilder.Append(PXMessages.LocalizeNoPrefix(CR.Messages.AddressInvalidWarning));

			return stringBuilder.ToString();
		}

		public static void OnFieldException(PXCache sender, PXExceptionHandlingEventArgs e, Type field, ref List<string> warnings)
		{
			if (sender == null || e == null || field == null || warnings == null)
				return;

			PXSetPropertyException setPropertyException = e.Exception as PXSetPropertyException;

			if (e.Row == null || setPropertyException == null || string.IsNullOrWhiteSpace(setPropertyException.MessageNoPrefix))
				return;

			warnings.Add(PXMessages.LocalizeFormatNoPrefix(
				CR.Messages.AddressInvalidFieldWarning,
				PXUIFieldAttribute.GetDisplayName(sender, field.Name).ToLower(),
				sender.GetValue(e.Row, field.Name),
				Environment.NewLine,
				setPropertyException.MessageNoPrefix));
		}
	}
}
