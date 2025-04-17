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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	public static class ConversionResultExtensions
	{
		public static TResult ThrowIfHasException<TResult>(this TResult conversionResult)
			where TResult : ConversionResult
		{
			if (conversionResult.Exception != null)
				ExceptionDispatchInfo.Capture(conversionResult.Exception).Throw();
			return conversionResult;
		}
	}

	public static class ConversionOptionsExtensions
	{
		public static IDisposable PreserveCachedRecords<TTargetGraph, TTarget>(
			this ConversionOptions<TTargetGraph, TTarget> options)
			where TTargetGraph : PXGraph, new ()
			where TTarget : class, IBqlTable, INotable, new ()
		{
			if (options == null
				|| options.PreserveCachedRecordsFilters == null
				|| options.PreserveCachedRecordsFilters.Count == 0)
				return Disposable.Empty;

			return new CompositeDisposable(
				options.PreserveCachedRecordsFilters.Select(f => f.PreserveCachedRecords()));
		}
	}

	/// <exclude/>
	public class ConversionResult
	{
		// false if it was Existing, or conversion skipped for any reason
		// default true
		public bool Converted { get; set; } = true;
		public Exception Exception { get; set; }
		public PXGraph Graph { get; set; }
	}

	/// <exclude/>
	public class ConversionResult<TTarget> : ConversionResult
		where TTarget : class, IBqlTable, INotable, new()
	{
		public TTarget Entity { get; set; }
	}

	/// <exclude/>
	public abstract class ConversionOptions<TTargetGraph, TTarget>
		where TTargetGraph : PXGraph, new()
		where TTarget : class, IBqlTable, INotable, new()
	{
		public IList<ICRPreserveCachedRecordsFilter> PreserveCachedRecordsFilters { get; set; } =
			new List<ICRPreserveCachedRecordsFilter>();

		public bool DoNotPersistAfterConvert { get; set; }

		public bool DoNotCancelAfterConvert { get; set; }
	}

	/// <exclude/>
	public class ContactConversionOptions : ConversionOptions<ContactMaint, Contact>
	{
		public PXGraph GraphWithRelation { get; set; }
	}

	/// <exclude/>
	public class AccountConversionOptions : ConversionOptions<BusinessAccountMaint, BAccount>
	{
		public Contact ExistingContact { get; set; }
	}

	/// <exclude/>
	public class OpportunityConversionOptions : ConversionOptions<OpportunityMaint, CROpportunity>
	{
		// hack to skip automation changing OverrideRefContact field, or changed by Contact/Account creation
		public bool? ForceOverrideContact { get; set; }
	}
}
