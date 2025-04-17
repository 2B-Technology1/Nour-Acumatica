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
using System.Reflection;

namespace PX.Objects.FS
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SkipSetExtensionVisibleInvisibleAttribute : Attribute { }

    public class DACHelper
    {
        public static List<string> GetFieldsName(Type dacType)
        {
            List<string> fieldList = new List<string>();

            foreach (PropertyInfo prop in dacType.GetProperties())
            {
                if (prop.GetCustomAttributes(true).Where(atr => atr is SkipSetExtensionVisibleInvisibleAttribute).Count() == 0)
                {
                    fieldList.Add(prop.Name);
                }
            }

            return fieldList;
        }

        public static void SetExtensionVisibleInvisible<DAC>(PXCache cache, PXRowSelectedEventArgs e, bool isVisible, bool isGrid)
            where DAC : PXCacheExtension
        {
            foreach (string fieldName in DACHelper.GetFieldsName(typeof(DAC)))
            {
                PXUIFieldAttribute.SetVisible(cache, null, fieldName, isVisible);
            }
        }

        public static void SetExtensionVisibleInvisible(Type dacType, PXCache cache, PXRowSelectedEventArgs e, bool isVisible, bool isGrid)
        {
            foreach (string fieldName in DACHelper.GetFieldsName(dacType))
            {
                PXUIFieldAttribute.SetVisible(cache, null, fieldName, isVisible);
            }
        }

        public static string GetDisplayName(Type inpDAC)
        {
            string name = inpDAC.Name;
            if (inpDAC.IsDefined(typeof(PXCacheNameAttribute), true))
            {
                PXCacheNameAttribute attr = (PXCacheNameAttribute)(inpDAC.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
                name = attr.GetName();
            }
            return name;
        }
    }
}
