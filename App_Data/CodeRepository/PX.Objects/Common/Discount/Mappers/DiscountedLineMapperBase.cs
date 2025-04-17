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
using PX.Data;

namespace PX.Objects.Common.Discount.Mappers
{
	public abstract class DiscountedLineMapperBase
	{
		public PXCache Cache { get; }
		public object MappedLine { get; }

		protected DiscountedLineMapperBase(PXCache cache, object row)
		{
			this.Cache = cache;
			this.MappedLine = row;
		}

		public abstract Type GetField<T>() where T : IBqlField;

		public virtual void RaiseFieldUpdating<T>(ref object newValue)
			where T : IBqlField
		{
			Cache.RaiseFieldUpdating(GetField<T>().Name, MappedLine, ref newValue);
		}

		public virtual void RaiseFieldUpdated<T>(object oldValue)
			where T : IBqlField
		{
			Cache.RaiseFieldUpdated(GetField<T>().Name, MappedLine, oldValue);
		}

		public virtual void RaiseFieldVerifying<T>(ref object newValue)
			where T : IBqlField
		{
			Cache.RaiseFieldVerifying(GetField<T>().Name, MappedLine, ref newValue);
		}
	}
}