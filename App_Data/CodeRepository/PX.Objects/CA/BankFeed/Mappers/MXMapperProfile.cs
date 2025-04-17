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
using PX.BankFeed.MX;

namespace PX.Objects.CA.BankFeed
{
	internal class MXMapperProfile : Profile
	{
		class PendingConverter : IValueConverter<string, bool?>
		{
			public bool? Convert(string source, ResolutionContext context)
				=> source == "PENDING";
		}

		class AmountResolver : IValueResolver<Transaction, BankFeedTransaction, decimal?>
		{
			public decimal? Resolve(Transaction source, BankFeedTransaction destination, decimal? destMember, ResolutionContext context)
				=> source.Type == "CREDIT" ? source.Amount * -1 : source.Amount;
		}

		class CategoryResolver : IValueResolver<Transaction, BankFeedTransaction, string>
		{
			public string Resolve(Transaction source, BankFeedTransaction destination, string destMember, ResolutionContext context)
				=> !string.IsNullOrEmpty(source.TopLevelCategory) && !string.IsNullOrEmpty(source.Category)
					&& source.TopLevelCategory != source.Category ? source.TopLevelCategory + BankFeedManager.CategoriesSeparator + source.Category : source.Category;
		}

		public MXMapperProfile()
		{
			CreateMap<ConnectResponse, BankFeedFormResponse>()
				.ForMember(d => d.ItemID, m => m.MapFrom(i => i.MemberGuid));
			CreateMap<MemberResponse, BankFeedFormResponse>()
				.ForMember(d => d.InstitutionID, m => m.MapFrom(i => i.Member.InstitutionCode))
				.ForMember(d => d.InstitutionName, m => m.MapFrom(i => i.Member.Name));
			CreateMap<Account, BankFeedAccount>()
				.ForMember(d => d.AccountID, m => m.MapFrom(i => i.Guid))
				.ForMember(d => d.Currency, m => m.MapFrom(i => i.CurrencyCode))
				.ForMember(d => d.Mask, m => m.MapFrom(i => i.AccountNumber))
				.ForMember(d => d.Name, m => m.MapFrom(i => i.Name))
				.ForMember(d => d.Type, m => m.MapFrom(i => i.Type))
				.ForMember(d => d.Subtype, m => m.MapFrom(i => i.Subtype));
			CreateMap<Transaction, BankFeedTransaction>()
				.ForMember(d => d.AccountID, m => m.MapFrom(i => i.AccountGuid))
				.ForMember(d => d.PartnerAccountId, m => m.MapFrom(i => i.AccountId))
				.ForMember(d => d.Amount, m => m.MapFrom(new AmountResolver()))
				.ForMember(d => d.Date, m => m.MapFrom(i => i.Date))
				.ForMember(d => d.IsoCurrencyCode, m => m.MapFrom(i => i.CurrencyCode))
				.ForMember(d => d.Name, m => m.MapFrom(i => i.Description))
				.ForMember(d => d.TransactionID, m => m.MapFrom(i => i.Guid))
				.ForMember(d => d.Type, m => m.MapFrom(i => i.Type))
				.ForMember(d => d.CheckNumber, m => m.MapFrom(i => i.CheckNumberString))
				.ForMember(d => d.Pending, m => m.ConvertUsing(new PendingConverter(), src => src.Status))
				.ForMember(d => d.Category, m => m.MapFrom(new CategoryResolver()))
				.ForMember(d => d.CreatedAt, m => m.MapFrom(i => i.CreatedAt))
				.ForMember(d => d.PostedAt, m => m.MapFrom(i => i.PostedAt))
				.ForMember(d => d.TransactedAt, m => m.MapFrom(i => i.TransactedAt))
				.ForMember(d => d.UpdatedAt, m => m.MapFrom(i => i.UpdatedAt))
				.ForMember(d => d.AccountStringId, m => m.MapFrom(i => i.AccountId))
				.ForMember(d => d.CategoryGuid, m => m.MapFrom(i => i.CategoryGuid))
				.ForMember(d => d.ExtendedTransactionType, m => m.MapFrom(i => i.ExtendedTransactionType))
				.ForMember(d => d.Id, m => m.MapFrom(i => i.Id))
				.ForMember(d => d.IsBillPay, m => m.MapFrom(i => i.IsBillPay))
				.ForMember(d => d.IsDirectDeposit, m => m.MapFrom(i => i.IsDirectDeposit))
				.ForMember(d => d.IsExpense, m => m.MapFrom(i => i.IsExpense))
				.ForMember(d => d.IsFee, m => m.MapFrom(i => i.IsFee))
				.ForMember(d => d.IsIncome, m => m.MapFrom(i => i.IsIncome))
				.ForMember(d => d.IsInternational, m => m.MapFrom(i => i.IsInternational))
				.ForMember(d => d.IsOverdraftFee, m => m.MapFrom(i => i.IsOverdraftFee))
				.ForMember(d => d.IsPayrollAdvance, m => m.MapFrom(i => i.IsPayrollAdvance))
				.ForMember(d => d.IsRecurring, m => m.MapFrom(i => i.IsRecurring))
				.ForMember(d => d.IsSubscription, m => m.MapFrom(i => i.IsSubscription))
				.ForMember(d => d.Latitude, m => m.MapFrom(i => i.Latitude))
				.ForMember(d => d.LocalizedDescription, m => m.MapFrom(i => i.LocalizedDescription))
				.ForMember(d => d.LocalizedMemo, m => m.MapFrom(i => i.LocalizedMemo))
				.ForMember(d => d.Longitude, m => m.MapFrom(i => i.Longitude))
				.ForMember(d => d.MemberIsManagedByUser, m => m.MapFrom(i => i.MemberIsManagedByUser))
				.ForMember(d => d.MerchantCategoryCode, m => m.MapFrom(i => i.MerchantCategoryCode))
				.ForMember(d => d.MerchantGuid, m => m.MapFrom(i => i.MerchantGuid))
				.ForMember(d => d.MerchantLocationGuid, m => m.MapFrom(i => i.MerchantLocationGuid))
				.ForMember(d => d.Metadata, m => m.MapFrom(i => i.Metadata))
				.ForMember(d => d.OriginalDescription, m => m.MapFrom(i => i.OriginalDescription))
				.ForMember(d => d.UserId, m => m.MapFrom(i => i.UserId));
		}
	}
}
