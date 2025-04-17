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
using System.Linq;

namespace PX.Objects.Extensions
{
	public class PXSetupBase<TSelf, TGraph, THeader, TSetup, Where> : PXGraphExtension<TGraph>
		where TSelf : PXSetupBase<TSelf, TGraph, THeader, TSetup, Where>
		where TGraph : PXGraph
		where THeader : class, IBqlTable, new()
		where TSetup : class, IBqlTable, new()
		where Where : IBqlWhere, new()
	{
		public TSetup UserSetup => EnsureUserSetup();

		public PXSelect<TSetup, Where> UserSetupView;

		public PXAction<THeader> UserSetupDialog;
		[PXButton(IsLockedOnToolbar = true), PXUIField(DisplayName = "User Settings")]
		protected virtual void userSetupDialog()
		{
			EnsureUserSetup();
			PXCache setupCache = Base.Caches[typeof(TSetup)];
			if (UserSetupView.AskExt() == WebDialogResult.OK)
			{
				using (var tr = new PXTransactionScope())
				{
					setupCache.IsDirty = true;
					setupCache.Persist(PXDBOperation.Insert);
					setupCache.Persist(PXDBOperation.Update);
					tr.Complete();
				}
				Base.Clear();
			}
			else
			{
				setupCache.Clear();
				setupCache.ClearQueryCacheObsolete();
			}
		}

		protected virtual TSetup EnsureUserSetup()
		{
			if (UserSetupView.Current == null)
				UserSetupView.Current = UserSetupView.Select();

			if (UserSetupView.Current == null)
				UserSetupView.Current = UserSetupView.Cache.Inserted.Cast<TSetup>().FirstOrDefault();

			if (UserSetupView.Current == null)
				UserSetupView.Current = UserSetupView.Insert();

			UserSetupView.Cache.IsDirty = false;

			return UserSetupView.Current;
		}

		/// <summary>
		/// Gets a <typeparamref name="TSetup"/> instance for a <typeparamref name="TGraph"/> instance.
		/// </summary>
		public static TSetup For(TGraph graph) => graph.FindImplementation<TSelf>().UserSetup;
	}
}
