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

namespace PX.Objects.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsComplex(this object obj)
		{
			return !obj.GetType().IsPrimitive
				   && obj.GetType() != typeof(string)
				   && obj.GetType().IsAssignableFrom(typeof(decimal))
			       && obj.GetType().IsAssignableFrom(typeof(DateTime))
			       && obj.GetType().IsAssignableFrom(typeof(Guid));
		}

		public static TObject[] SingleToArray<TObject>(this TObject obj)
		{
			return new[] { obj };
		}

		public static TObject[] SingleToArrayOrNull<TObject>(this TObject obj)
		{
			if (obj == null)
				return null;

			return new[] { obj };
		}

	    public static List<TObject> SingleToListOrNull<TObject>(this TObject obj)
	    {
	        if (obj == null)
	            return null;

	        return new List<TObject>() {obj};
	    }

        public static object[] SingleToObjectArray<TObject>(this TObject obj, bool dontCreateForNull = true)
	    {
	        if (obj == null && dontCreateForNull)
	            return null; 

	        return new[] { (object)obj };
	    }

        public static List<TObject> SingleToList<TObject>(this TObject obj)
		{
			return new List<TObject>() { obj };
		}
	}
}
