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
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{

	/// <summary>
	/// Represents an external shipping zone that is used for the shipping settings (ship via) mappings for order import.
	/// </summary>		
	public interface IShippingZone
	{
		string Name { get; set; }
		string Type { get; set; }
		bool? Enabled { get; set; }

		List<IShippingMethod> ShippingMethods { get; set; }
	}

	/// <summary>
	/// Represents an external shipping method that is used for the shipping settings (ship via) mappings for order import. 
	/// </summary>	
	public interface IShippingMethod
	{
		string Name { get; set; }
		string Type { get; set; }
		bool? Enabled { get; set; }

		List<String> ShippingServices { get; set; }
	}
}
