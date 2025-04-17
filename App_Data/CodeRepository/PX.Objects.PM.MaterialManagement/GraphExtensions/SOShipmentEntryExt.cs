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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOShipmentEntryExt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM.MaterialManagement
{
	public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.materialManagement>();
		}

		[PXOverride]
		public virtual void CreateShipment(CreateShipmentArgs args,
			Action<CreateShipmentArgs> baseMethod)
		{
			try
			{
				baseMethod(args);
			}
			catch (SOShipmentException ex)
			{
				if (ex.Code == SOShipmentException.ErrorCode.NothingToShipTraced && ex.Item != null)
				{
					PMProject project = PMProject.PK.Find(Base, ex.Item.ProjectID);

					if (project.ContractID != ProjectDefaultAttribute.NonProject())
					{
						INSite site = INSite.PK.Find(Base, ex.Item.SiteID);
						throw new SOShipmentException(ex.Code, ex.Item, PM.Messages.NothingToShipTraced, ex.Item.OrderNbr, site.SiteCD.Trim(), project.ContractCD.Trim(), ex.Item.ShipDate?.ToShortDateString());
					}
					else
					{
						throw new SOShipmentException(ex.Code, ex.Item, SO.Messages.NothingToShipTraced, ex.Item.OrderType, ex.Item.OrderNbr, ex.Item.ShipDate);
					}
				}
				else
				{
					throw;
				}
			}
		}

	}
}
