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
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions.Abstract;

namespace PX.Objects.IN.GraphExtensions
{
	/// <exclude/>
	[PXCacheName(Messages.LocationLinkFilter)]
	public class LocationLinkFilter : IBqlTable
	{
		#region LocationID
		[Location(IsKey = true)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion

		// Acuminator disable once PX1076 CallToInternalApi [Justification]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public abstract class AttachedLocationDescription<TSelf, TGraph> : PXFieldAttachedTo<LocationLinkFilter>.By<TGraph>.AsString.Named<TSelf>
			where TGraph : PXGraph
			where TSelf : PXFieldAttachedTo<LocationLinkFilter>.By<TGraph>.AsString
		{
			public override string GetValue(LocationLinkFilter Row) => INLocation.PK.Find(Base, Row?.LocationID)?.Descr;
		}
	}

	public abstract class LocationLinkFilterExtensionBase<TGraph, TGraphFilter, TGraphFilterLocationID> : EntityLinkFilterExtensionBase<TGraph, TGraphFilter, TGraphFilterLocationID, LocationLinkFilter, LocationLinkFilter.locationID, int?>
		where TGraph : PXGraph, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
		where TGraphFilter : class, IBqlTable, new()
		where TGraphFilterLocationID : class, IBqlField, IImplement<IBqlInt>
	{
		protected override string EntityViewName => nameof(SelectedLocations);

		[PXVirtualDAC]
		[PXImport]
		[PXReadOnlyView]
		public PXSelect<LocationLinkFilter> SelectedLocations;
		public IEnumerable selectedLocations() => GetEntities();

		[PXButton, PXUIField(DisplayName = "List")]
		public virtual void selectLocations() => SelectedLocations.AskExt();
		public PXAction<TGraphFilter> SelectLocations;

		public abstract class AttachedLocationDescription<TSelf> : LocationLinkFilter.AttachedLocationDescription<TSelf, TGraph>
			where TSelf : PXFieldAttachedTo<LocationLinkFilter>.By<TGraph>.AsString
		{
		}
	}
}
