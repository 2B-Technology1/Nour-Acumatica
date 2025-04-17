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

namespace PX.Objects.Common.Attributes
{
	/// <summary>
	/// Sets the value of the target boolean field to true once the <see cref="ObservedField"/> got modified.
	/// </summary>
	public class HasFieldBeenModifiedAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowUpdatedSubscriber
	{
		/// <summary>
		/// A field the attribute waits an update for.
		/// </summary>
		public Type ObservedField { get; set; }

		/// <summary>
		/// A field of whose value the attribute compares to the value of the observed field with to avoid original row selection from the database.
		/// </summary>
		public Type OriginalValueField { get; set; }

		/// <summary>
		/// Indicates whether <c>false</c> or <c>true</c> shall be assigned to the target field once the <see cref="ObservedField"/> got modified.
		/// </summary>
		public bool InvertResult { get; set; }

		/// <exclude/>
		public HasFieldBeenModifiedAttribute(Type observedField) => ObservedField = observedField;

		void IPXRowPersistingSubscriber.RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Update)
				if (OriginalValueField == null
					? !object.Equals(cache.GetValue(e.Row, ObservedField.Name), cache.GetValueOriginal(e.Row, ObservedField.Name))
					: !object.Equals(cache.GetValue(e.Row, ObservedField.Name), cache.GetValue(e.Row, OriginalValueField.Name)))
				{
					cache.SetValue(e.Row, FieldName, !InvertResult);
				}
		}

		void IPXRowUpdatedSubscriber.RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (cache.GetStatus(e.Row) == PXEntryStatus.Inserted && !object.Equals(cache.GetValue(e.Row, ObservedField.Name), cache.GetValue(e.OldRow, ObservedField.Name)))
				cache.SetValue(e.Row, FieldName, !InvertResult);
		}
	}
}
