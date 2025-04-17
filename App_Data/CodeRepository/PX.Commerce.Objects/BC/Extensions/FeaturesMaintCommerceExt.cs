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

using PX.Commerce.Core;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PX.Commerce.Objects
{
	public class FeaturesMaintCommerceExt : PXGraphExtension<FeaturesMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		public SelectFrom<BCEntity>.View Entities;

		protected virtual void _(Events.RowPersisted<FeaturesSet> e)
		{
			if (e.Row == null) return;
			bool anyChanges = false;

			foreach (BCEntity entity in Entities.Select())
			{
				EntityInfo requiredEntity = ConnectorHelper.GetConnectorEntity(entity.ConnectorType, entity.EntityType);

				if (requiredEntity?.AcumaticaFeaturesSet == null || GetPropertyStatusByName(e.Cache, e.Row, requiredEntity.AcumaticaFeaturesSet.Name))
					continue;

				entity.IsActive = false;
				var updatedRow = Entities.Cache.Update(entity);

				if (e.TranStatus == PXTranStatus.Open)
				{
					Entities.Cache.PersistUpdated(updatedRow);
				}
			}
		}

		private bool GetPropertyStatusByName(PXCache cache, FeaturesSet row, string propertyName)
		{
			return (bool)cache.GetValue(row, propertyName);
		}
	}
}
