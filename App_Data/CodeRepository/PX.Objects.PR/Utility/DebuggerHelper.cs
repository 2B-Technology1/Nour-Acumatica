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

namespace PX.Objects.PR
{
	/// <summary>
	/// Provide a nice formatting for Watch window to display Key values of records when debugging.
	/// </summary>
	/// <example>
	/// [DebuggerDisplay(@"{DebuggerHelper.Info(this)}")]
	/// public class PRPayment : IBqlTable
	/// </example>
	/// <remarks>
	/// This attribute should not be left in code as it can affect performance. Remove after debugging.
	/// </remarks>
	public class DebuggerHelper
	{
		public static string Info(object obj)
		{
			var debugString = obj.GetType().Name + " :";
			foreach (var prop in obj.GetType().GetProperties())
			{
				foreach (var att in prop.GetCustomAttributes(true))
				{
					if (att is PXDBFieldAttribute dbAtt && dbAtt.IsKey == true)
					{
						debugString += " " + prop.Name + " = {" + prop.GetValue(obj) + "},";
					}
				}
			}

			return debugString;
		}
	}
}
