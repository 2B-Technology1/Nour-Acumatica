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
using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Repositories;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class V2ProcessingInputGenerator
	{
		private ICardProcessingReadersProvider _provider;

		public bool FillCardData { get; set; } = true;
		public bool FillCustomerData { get; set; } = true;
		public bool FillAdressData { get; set; } = false;

		public V2ProcessingInputGenerator(ICardProcessingReadersProvider provider)
		{
			_provider = provider;
		}

		public V2.ProcessingInput GetProcessingInput(V2.CCTranType tranType, ICCPayment pDoc)
		{
			V2.PaymentFormPrepareOptions options = GetPaymentFormPrepareOptions(tranType, pDoc);
			return Convert(options);
		}

		public V2.PaymentFormPrepareOptions GetPaymentFormPrepareOptions(V2.CCTranType tranType, ICCPayment pDoc)
		{
			if (pDoc == null) throw new ArgumentNullException(nameof(pDoc));
			var result = new V2.PaymentFormPrepareOptions()
			{
				TranType = tranType,
				Amount = pDoc.CuryDocBal.Value,
				CuryID = pDoc.CuryID,
				OrigTranID = tranType == V2.CCTranType.CaptureOnly ? null : pDoc.OrigRefNbr,
				AuthCode = tranType == V2.CCTranType.CaptureOnly ? pDoc.OrigRefNbr : null
			};
			if (FillCardData)
			{
				result.CardData = GetCardData(_provider.GetCardDataReader());
				result.CardData.AddressData = GetAddressData(_provider.GetCustomerDataReader());
			}

			if (FillAdressData && result.CardData == null)
			{
				result.CardData = new V2.CreditCardData();
				result.CardData.AddressData = GetAddressData(_provider.GetCustomerDataReader());
			}

			if (FillCustomerData)
			{
				result.CustomerData = GetCustomerData(_provider.GetCustomerDataReader());
			}
			result.DocumentData = new V2.DocumentData();
			result.DocumentData.DocType = pDoc.DocType;
			result.DocumentData.DocRefNbr = pDoc.RefNbr;
			FillDocumentData(result);
			return result;
		}

		public V2.ProcessingInput GetProcessingInput(CCTranType tranType, TranProcessingInput inputData)
		{
			V2.PaymentFormPrepareOptions options = GetPaymentFormPrepareOptions(tranType, inputData);
			return Convert(options);
		}

		private V2.PaymentFormPrepareOptions GetPaymentFormPrepareOptions(CCTranType commonTranType, TranProcessingInput inputData)
		{
			if (inputData == null) throw new ArgumentNullException(nameof(inputData));
			V2.CCTranType tranType = V2Converter.ConvertTranTypeToV2(commonTranType);
			var result = new V2.PaymentFormPrepareOptions()
			{
				TranType = tranType,
				Amount = inputData.Amount,
				CuryID = inputData.CuryID,
				OrigTranID = commonTranType == CCTranType.CaptureOnly ? null : inputData.OrigRefNbr,
				AuthCode = commonTranType == CCTranType.CaptureOnly ? inputData.OrigRefNbr : null,
				MeansOfPayment = inputData.MeansOfPayment == CA.PaymentMethodType.EFT ? V2.MeansOfPayment.EFT : V2.MeansOfPayment.CreditCard,
				SubtotalAmount = inputData.SubtotalAmount,
				Tax = inputData.Tax,
			};

			if (FillCardData)
			{
				result.CardData = GetCardData(_provider.GetCardDataReader());
				result.CardData.AddressData = GetAddressData(_provider.GetCustomerDataReader());
			}

			if (FillCustomerData)
			{
				result.CustomerData = GetCustomerData(_provider.GetCustomerDataReader());
			}

			result.DocumentData = new V2.DocumentData();
			result.DocumentData.DocType = inputData.DocType;
			result.DocumentData.DocRefNbr = inputData.DocRefNbr;
			FillDocumentData(result);
			result.TranUID = inputData.TranUID;
			return result;
		}

		private static V2.ProcessingInput Convert(V2.PaymentFormPrepareOptions options)
		{
			return new V2.ProcessingInput
			{
				TranType = options.TranType,
				Amount = options.Amount,
				CuryID = options.CuryID,
				OrigTranID = options.OrigTranID,
				AuthCode = options.AuthCode,
				DocumentData = options.DocumentData,
				CustomerData = options.CustomerData,
				CardData = options.CardData,
				TranUID = options.TranUID,
				SaveCard = options.SaveCard,
				MeansOfPayment = options.MeansOfPayment,
				SubtotalAmount = options.SubtotalAmount,
				Tax = options.Tax,
			};
		}

		public static V2.CreditCardData GetCardData(V1.ICreditCardDataReader cardReader, String2DateConverterFunc expirationDateConverter = null)
		{
			Dictionary<string, string> cardData = new Dictionary<string, string>();
			cardReader.ReadData(cardData);
			var v2CardData = new V2.CreditCardData();
			string value;
			if (cardData.TryGetValue(cardReader.Key_PMCCProcessingID, out value))
			{
				v2CardData.PaymentProfileID = value;
			}
			if (cardData.TryGetValue(cardReader.Key_CardNumber, out value))
			{
				v2CardData.CardNumber = value;
			}
			if (cardData.TryGetValue(cardReader.Key_CardCVV, out value))
			{
				v2CardData.CVV = value;
			}
			if (expirationDateConverter != null)
			{
				if (cardData.TryGetValue(cardReader.Key_CardExpiryDate, out value))
				{
					v2CardData.CardExpirationDate = expirationDateConverter(value);
				}
			}
			else
			{
				v2CardData.CardExpirationDate = null;
			}

			return v2CardData;
		}

		public static V2.CustomerData GetCustomerData(V1.ICustomerDataReader customerReader)
		{
			var v2CustomerData = new V2.CustomerData();
			Dictionary<string, string> customerData = new Dictionary<string, string>();
			customerReader.ReadData(customerData);
			string value;
			if (customerData.TryGetValue(customerReader.Key_Customer_CCProcessingID, out value))
			{
				v2CustomerData.CustomerProfileID = value;
			}
			if (customerData.TryGetValue(customerReader.Key_CustomerCD, out value))
			{
				v2CustomerData.CustomerCD = value;
			}
			if (customerData.TryGetValue(customerReader.Key_CustomerName, out value))
			{
				v2CustomerData.CustomerName = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillContact_Email, out value))
			{
				v2CustomerData.Email = value;
			}

			return v2CustomerData;
		}

		public static V2.AddressData GetAddressData(V1.ICustomerDataReader customerReader)
		{
			var addressData = new V2.AddressData();
			Dictionary<string, string> customerData = new Dictionary<string, string>();
			customerReader.ReadData(customerData);
			string value;

			if (customerData.TryGetValue(customerReader.Key_Customer_FirstName, out value))
			{
				addressData.FirstName = value;
			}
			if (customerData.TryGetValue(customerReader.Key_Customer_LastName, out value))
			{
				addressData.LastName = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_Address, out value))
			{
				addressData.Address = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_AddressLine1, out value))
			{
				addressData.AddressLine1 = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_AddressLine2, out value))
			{
				addressData.AddressLine2 = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_AddressLine3, out value))
			{
				addressData.AddressLine3 = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_City, out value))
			{
				addressData.City = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_Country, out value))
			{
				addressData.Country = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_PostalCode, out value))
			{
				addressData.PostalCode = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillAddr_State, out value))
			{
				addressData.State = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillContact_Email, out value))
			{
				addressData.Email = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillContact_Fax, out value))
			{
				addressData.Fax = value;
			}
			if (customerData.TryGetValue(customerReader.Key_BillContact_Phone, out value))
			{
				addressData.Phone = value;
			}

			return addressData;
		}

		private void FillDocumentData(V2.PaymentFormPrepareOptions processingInput)
		{
			processingInput.DocumentData.DocumentDetails = new List<V2.DocumentDetailData>();
			V1.IDocDetailsDataReader documentReader = _provider.GetDocDetailsDataReader();
			List<V1.DocDetailInfo> detailsData = new List<V1.DocDetailInfo>();
			documentReader.ReadData(detailsData);
			foreach (var item in detailsData)
			{
				V2.DocumentDetailData v2Item = ToV2(item);
				processingInput.DocumentData.DocumentDetails.Add(v2Item);
			}
		}

		public static V2.DocumentDetailData ToV2(V1.DocDetailInfo docDetail)
		{
			V2.DocumentDetailData result = new V2.DocumentDetailData();
			result.ItemID = docDetail.ItemID;
			result.ItemDescription = docDetail.ItemDescription;
			result.ItemName = docDetail.ItemName;
			result.Price = docDetail.Price;
			result.Quantity = docDetail.Quantity;
			result.IsTaxable = docDetail.IsTaxable;
			return result;
		}
	}
}
