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

namespace PX.Objects.IN
{
	public interface ICostCenterSupport<TLine>
		where TLine : class, IItemPlanMaster, IBqlTable, new()
	{
		int SortOrder { get; }

		IEnumerable<Type> GetFieldsDependOn();

		bool IsSpecificCostCenter(TLine line);

		int GetCostCenterID(TLine line);
	}

	public interface IINTranCostCenterSupport : ICostCenterSupport<INTran>
	{		
		bool IsSupported(string layerType);

		string GetCostLayerType(INTran tran);

		IEnumerable<Type> GetDestinationFieldsDependOn();

		bool IsDestinationSpecificCostCenter(INTran tran);

		int GetDestinationCostCenterID(INTran tran);

		void OnCostLayerTypeChanged(INTran tran, string newCostLayerType);
		void OnDestinationCostLayerTypeChanged(INTran tran, string newCostLayerType);

		void ValidateForPersisting(INTran tran);
		void ValidateDestinationForPersisting(INTran tran);
	}
}
