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
using PX.Data;

namespace PX.Data.WorkflowAPI
{
	public static class WorkflowExtensions
	{
		/// <summary>
		/// Get a new instance of an anonymous class, that contains <see cref="BoundedTo{TGraph, TPrimary}.Condition"/>s,
		/// but where condition names are taken from their properties' names.
		/// </summary>
		/// <param name="conditionPack">An instance of an anonymous class, that contains only properties of <see cref="BoundedTo{TGraph, TPrimary}.Condition"/> type.</param>
		public static T AutoNameConditions<T>(this T conditionPack)
			where T : class
		{
			if (!typeof(T).IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
				throw new InvalidOperationException("Only instances of anonymous types are allowed");

			return (T)Activator.CreateInstance(
				typeof(T),
				typeof(T)
					.GetProperties()
					.Select(p =>
					(
						Target: p.GetValue(conditionPack),
						WithSharedName: p.PropertyType.GetMethod(nameof(BoundedTo<PXGraph, Table>.Condition.WithSharedName)),
						Name: p.Name,
						GetName: p.PropertyType.GetProperty(nameof(BoundedTo<PXGraph, Table>.Condition.Name)).GetMethod
					))
					.Select(p => p.GetName.Invoke(p.Target, Array.Empty<object>()) == null
						? p.WithSharedName.Invoke(p.Target, new object[] { p.Name })
						: p.Target)
					.ToArray());
		}

		[PXHidden]
		private class Table : IBqlTable { }
	}
}