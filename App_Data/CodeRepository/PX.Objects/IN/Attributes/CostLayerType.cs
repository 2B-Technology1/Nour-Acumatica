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
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class CostLayerType
	{
		public const string Normal = "N";
		public const string Project = "P";
		public const string Special = "S";
		public const string Production = "R";

		public class normal : Data.BQL.BqlString.Constant<normal>
		{
			public normal() : base(Normal) {; }
		}
		public class project : Data.BQL.BqlString.Constant<project>
		{
			public project() : base(Project) {; }
		}
		public class special : Data.BQL.BqlString.Constant<special>
		{
			public special() : base(Special) {; }
		}

		public class production : Data.BQL.BqlString.Constant<production>
		{
			public production() : base(Production) {; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public bool AllowSpecialOrders { get; set; } = true;
			public bool AllowProjects { get; set; } = true;
			public bool AllowProductionOrders { get; set; } = true;

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				SetValues(sender, null);
			}

			public virtual int SetValues(PXCache cache, object row)
			{
				var values = new List<(string Value, string Label)>() { (Normal, Messages.Normal) };

				if (PXAccess.FeatureInstalled<FeaturesSet.specialOrders>() && AllowSpecialOrders)
					values.Add((Special, Messages.Special));

				if (PXAccess.FeatureInstalled<FeaturesSet.materialManagement>() && AllowProjects)
					values.Add((Project, Messages.Project));

				if (false/* PXAccess.FeatureInstalled<FeaturesSet.productionOrdersInventorySpecific>() */ && AllowProductionOrders)
					values.Add((Production, Messages.ProductionOrder));

				SetList(cache, row, FieldName, values.ToArray());

				return values.Count;
			}
		}
	}
}
