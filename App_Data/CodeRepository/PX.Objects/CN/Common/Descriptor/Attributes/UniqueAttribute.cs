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
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class UniqueAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public string ErrorMessage
		{
			get;
			set;
		} = ErrorMessages.ValueIsNotUnique;

		public Type WhereCondition
		{
			get;
			set;
		}

		public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
		{
			if (args.Operation.IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				ValidateEntity(cache, args.Row);
			}
		}

		private void ValidateEntity(PXCache cache, object entity)
		{
			var value = cache.GetValue(entity, _FieldName);
			if (value != null)
			{
				var view = GetView(cache);
				var records = view.SelectMulti(value.SingleToArray());
				if (records.HasAtLeastTwoItems())
				{
					cache.RaiseException(_FieldName, entity, ErrorMessage, value);
				}
			}
		}

		private PXView GetView(PXCache cache)
		{
			var command = GetBqlCommand(cache);
			if (WhereCondition != null)
			{
				command = command.WhereAnd(WhereCondition);
			}
			return new PXView(cache.Graph, false, command);
		}

		private BqlCommand GetBqlCommand(PXCache cache)
		{
			var fieldType = cache.GetBqlField(_FieldName);
			return BqlCommand.CreateInstance(typeof(Select<,>), BqlTable,
				typeof(Where<,>), fieldType, typeof(Equal<>), typeof(Required<>), fieldType);
		}
	}
}