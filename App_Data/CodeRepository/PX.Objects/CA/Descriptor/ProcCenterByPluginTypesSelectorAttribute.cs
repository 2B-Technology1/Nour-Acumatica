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
using System.Linq;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.CA
{
	public class ProcCenterByPluginTypesSelectorAttribute : PXCustomSelectorAttribute
	{
		Type search;
		string[] pluginTypeNames;
		public ProcCenterByPluginTypesSelectorAttribute(Type search, Type selectType, string[] pluginTypeNames) : base(selectType)
		{
			this.search = search;
			this.pluginTypeNames = pluginTypeNames;
		}

		public IEnumerable GetRecords()
		{
			BqlCommand command = BqlCommand.CreateInstance(search);
			PXView view = new PXView(_Graph, false, command);
			foreach (object obj in view.SelectMulti())
			{
				CCProcessingCenter procCenter = PXResult.Unwrap<CCProcessingCenter>(obj);
				if (CheckPluginType(procCenter))
				{
					yield return obj;
				}
			}
		}

		private bool CheckPluginType(CCProcessingCenter procCenter)
		{
			string pluginTypeName = procCenter.ProcessingTypeName;
			bool res = pluginTypeNames.Contains(pluginTypeName);
			if (res)
			{
				return true;
			}

			try
			{
				Type pluginType = CCPluginTypeHelper.GetPluginType(pluginTypeName);
				foreach (string typeName in pluginTypeNames)
				{
					res = CCPluginTypeHelper.CheckParentClass(pluginType, typeName, 0, 3) ||
						CCPluginTypeHelper.CheckImplementInterface(pluginType, typeName);
					if (res)
					{
						return true;
					}
				}
			}
			catch { }
			return false;
		}
	}
}