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
using PX.Objects.AR;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class UpdateIfFieldsChangedScope : IDisposable
	{
		public class Changes
		{
			public HashSet<Type> SourceOfChange { get; set; }
			public int ReferenceCounter { get; set; }
		}

		protected bool _disposed;

		public UpdateIfFieldsChangedScope()
		{
			Changes currentContext = PXContext.GetSlot<Changes>();
			if (currentContext == null)
			{
				currentContext = new Changes();
				PXContext.SetSlot<Changes>(currentContext);
			}
			currentContext.ReferenceCounter++;
		}

		public virtual UpdateIfFieldsChangedScope AppendContext(params Type[] newChanges)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data.SourceOfChange == null) data.SourceOfChange = new HashSet<Type>();

			foreach (var change in newChanges)
				if (!data.SourceOfChange.Contains(change)) data.SourceOfChange.Add(change);

			return this;
		}

		public virtual UpdateIfFieldsChangedScope AppendContext<Field>() where Field : IBqlField
			=> AppendContext(typeof(Field));

		public virtual UpdateIfFieldsChangedScope AppendContext<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> AppendContext(typeof(Field1), typeof(Field2));

		public void Dispose()
		{
			if (_disposed) throw new PXObjectDisposedException();
			_disposed = true;

			Changes currentContext = PXContext.GetSlot<Changes>();
			currentContext.ReferenceCounter--;
			if (currentContext.ReferenceCounter == 0) PXContext.SetSlot<Changes>(null);
		}

		public virtual bool IsUpdateNeeded(params Type[] changes)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data?.SourceOfChange == null) return true;

			foreach (var change in changes)
				if (data.SourceOfChange.Contains(change)) return true;

			return false;
		}

		public virtual bool IsUpdateNeeded<Field>() where Field : IBqlField
			=> IsUpdateNeeded(typeof(Field));

		public virtual bool IsUpdateNeeded<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> IsUpdateNeeded(typeof(Field1), typeof(Field2));

		public virtual bool IsUpdatedOnly(params Type[] fields)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data?.SourceOfChange == null) return true;

			foreach (var sourceOfChange in data.SourceOfChange)
				if (!fields.Contains(sourceOfChange)) return false;

			return true;
		}

		public virtual bool IsUpdatedOnly<Field>() where Field : IBqlField
			=> IsUpdatedOnly(typeof(Field));

		public virtual bool IsUpdatedOnly<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> IsUpdatedOnly(typeof(Field1), typeof(Field2));

		public static bool Contains<TField>() where TField: IBqlField
		{
			var data = PXContext.GetSlot<Changes>();
			return data?.SourceOfChange?.Contains(typeof(TField)) == true;
		}

		public static bool Any(Func<Type, bool> predicate)
		{
			var data = PXContext.GetSlot<Changes>();
			return data?.SourceOfChange?.Any(predicate) == true;
		}

		public static UpdateIfFieldsChangedScope Create(PXCache cache, object oldRow, object newRow, params Type[] fields)
		{
			var changedFields = new List<Type>();
			foreach(var type in fields)
			{
				if (!Equals(cache.GetValue(oldRow, type.Name), cache.GetValue(newRow, type.Name)))
					changedFields.Add(type);
			}
			if (!changedFields.Any())
				return null;

			return new UpdateIfFieldsChangedScope().AppendContext(changedFields.ToArray());
		}
	}
}
