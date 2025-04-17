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

using AutoMapper;
using PX.BankFeed.Plaid;
using System.Collections.Generic;

namespace PX.Objects.CA.BankFeed
{
	internal class PlaidMapperProfile : Profile
	{
		class CategoryConverter : IValueConverter<List<string>, string>
		{
			public string Convert(List<string> source, ResolutionContext context)
				=> source != null ? string.Join(BankFeedManager.CategoriesSeparator, source) : null;
		}

		class PersonFinanceCategoryConverter : IValueConverter<PersonalFinanceCategory, string>
		{
			public string Convert(PersonalFinanceCategory personalFinanceCategory, ResolutionContext context)
				=> personalFinanceCategory != null ? $"{personalFinanceCategory.Primary},{personalFinanceCategory.Detailed}" : null;
		}

		public PlaidMapperProfile()
		{
			CreateMap<Transaction, BankFeedTransaction>()
				.ForMember(d => d.Category, m => m.ConvertUsing(new CategoryConverter(), src => src.Categories))
				.ForMember(d => d.AuthorizedDate, m => m.MapFrom(i => i.AuthorizedDate))
				.ForMember(d => d.AuthorizedDatetime, m => m.MapFrom(i => i.AuthorizedDatetime))
				.ForMember(d => d.DatetimeValue, m => m.MapFrom(i => i.DatetimeValue))
				.ForMember(d => d.Address, m => m.MapFrom(i => i.Location.Address))
				.ForMember(d => d.City, m => m.MapFrom(i => i.Location.City))
				.ForMember(d => d.Country, m => m.MapFrom(i => i.Location.Country))
				.ForMember(d => d.Latitude, m => m.MapFrom(i => i.Location.Lat))
				.ForMember(d => d.Longitude, m => m.MapFrom(i => i.Location.Lon))
				.ForMember(d => d.PostalCode, m => m.MapFrom(i => i.Location.PostalCode))
				.ForMember(d => d.Region, m => m.MapFrom(i => i.Location.Region))
				.ForMember(d => d.StoreNumber, m => m.MapFrom(i => i.Location.StoreNumber))
				.ForMember(d => d.MerchantName, m => m.MapFrom(i => i.MerchantName))
				.ForMember(d => d.PaymentChannel, m => m.MapFrom(i => i.PaymentChannel))
				.ForMember(d => d.ByOrderOf, m => m.MapFrom(i => i.PaymentMetadata.ByOrderOf))
				.ForMember(d => d.Payee, m => m.MapFrom(i => i.PaymentMetadata.Payee))
				.ForMember(d => d.Payer, m => m.MapFrom(i => i.PaymentMetadata.Payer))
				.ForMember(d => d.PaymentMethod, m => m.MapFrom(i => i.PaymentMetadata.PaymentMethod))
				.ForMember(d => d.PaymentProcessor, m => m.MapFrom(i => i.PaymentMetadata.PaymentProcessor))
				.ForMember(d => d.PpdId, m => m.MapFrom(i => i.PaymentMetadata.PpdId))
				.ForMember(d => d.Reason, m => m.MapFrom(i => i.PaymentMetadata.Reason))
				.ForMember(d => d.ReferenceNumber, m => m.MapFrom(i => i.PaymentMetadata.ReferenceNumber))
				.ForMember(d => d.PersonalFinanceCategory, m => m.ConvertUsing(new PersonFinanceCategoryConverter(), src => src.PersonalFinanceCategory))
				.ForMember(d => d.TransactionCode, m => m.MapFrom(i => i.TransactionCode))
				.ForMember(d => d.UnofficialCurrencyCode, m => m.MapFrom(i => i.UnofficialCurrencyCode));
			CreateMap<Account, BankFeedAccount>()
				.ForMember(d => d.Currency, m => m.MapFrom(i => i.Balances.IsoCurrencyCode));
			CreateMap<ExchangeTokenResponse, BankFeedFormResponse>();
			CreateMap<ConnectResponse, BankFeedFormResponse>()
				.ForMember(d => d.InstitutionID, m => m.MapFrom(i => i.Metadata.Institution.InstitutionId))
				.ForMember(d => d.InstitutionName, m => m.MapFrom(i => i.Metadata.Institution.Name));
		}
	}
}
