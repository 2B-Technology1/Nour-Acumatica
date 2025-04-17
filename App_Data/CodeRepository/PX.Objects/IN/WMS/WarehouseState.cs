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
using PX.Data.BQL;
using PX.BarcodeProcessing;

namespace PX.Objects.IN
{
	public abstract class WarehouseState<TScanBasis> : EntityState<TScanBasis, INSite>
		where TScanBasis : PXGraphExtension, IBarcodeDrivenStateMachine
	{
		public const string Value = "SITE";
		public class value : BqlString.Constant<value> { public value() : base(WarehouseState<TScanBasis>.Value) { } }

		public override string Code => Value;
		protected override string StatePrompt => Msg.Prompt;

		protected abstract int? SiteID { get; set; }
		protected abstract bool UseDefaultWarehouse { get; }
		protected virtual int? DefaultSiteID => UserPreferenceExt.GetDefaultSite(Basis.Graph);
		protected override bool IsStateSkippable() => SiteID != null;

		protected override void OnTakingOver()
		{
			if (IsActive && !IsSkippable && UseDefaultWarehouse && DefaultSiteID != null && INSite.PK.Find(Basis.Graph, DefaultSiteID) is INSite defaultSite)
				Process(defaultSite.SiteCD);
		}

		protected override INSite GetByBarcode(string barcode) => INSite.UK.Find(Basis.Graph, barcode);
		protected override void ReportMissing(string barcode) => Basis.Reporter.Error(Msg.Missing, barcode);
		protected override void Apply(INSite site) => SiteID = site.SiteID;
		protected override void ClearState() => SiteID = null;
		protected override void ReportSuccess(INSite site) => Basis.Reporter.Info(Msg.Ready, site.SiteCD);

		[PXLocalizable]
		public abstract class Msg
		{
			public const string Prompt = "Scan the barcode of the warehouse.";
			public const string Ready = "The {0} warehouse is selected.";
			public const string Missing = "The {0} warehouse is not found.";
		}
	}
}