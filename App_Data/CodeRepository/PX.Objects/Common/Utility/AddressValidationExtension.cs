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
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public interface IAddressValidationHelper
	{
		bool CurrentAddressRequiresValidation { get; }

		public void ValidateAddress();
	}

	public static class AddressValidationHelperExtension
	{
		public static bool RequiresValidation(this IEnumerable<IAddressValidationHelper> addressLookupExtensions) => addressLookupExtensions.Any(_ => _.CurrentAddressRequiresValidation);

		public static void ValidateAddresses(this IEnumerable<IAddressValidationHelper> addressExtensions)
		{
			foreach (IAddressValidationHelper addressExtension in addressExtensions)
			{
				addressExtension.ValidateAddress();
			}
		}
	}

	public abstract class AddressValidationExtension<TGraph, TAddress> : PXGraphExtension<TGraph>, IAddressValidationHelper
		where TGraph : PXGraph
		where TAddress : class, IBqlTable, IAddress, new()
	{
		protected abstract IEnumerable<PXSelectBase<TAddress>> AddressSelects();

		public virtual void _(Events.RowInserted<TAddress> e) => StoreCached(e.Row);

		protected virtual void StoreCached(TAddress address)
		{
			foreach (PXSelectBase<TAddress> view in AddressSelects())
			{
				view.StoreCached(new PXCommandKey(new object[] { address.AddressID }, true), new List<object> { address });
			}
		}

		public bool CurrentAddressRequiresValidation => AddressSelects().Select(SelectAddress).Any(RequiresValidation);

		protected virtual TAddress SelectAddress(PXSelectBase<TAddress> selectBase) => selectBase.Select();

		protected virtual bool RequiresValidation(TAddress address) => address?.IsDefaultAddress == false && address?.IsValidated == false;

		public void ValidateAddress()
		{
			foreach (TAddress address in AddressSelects().Select(SelectAddress).Where(RequiresValidation))
			{
				PXAddressValidator.Validate(Base, address, true, true);
			}
		}
	}
}
