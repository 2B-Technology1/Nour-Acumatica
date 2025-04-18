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

namespace PX.Objects.AM
{
    public class UniqueProductionNbrList : UniqueStringCollection
    {
        protected const char seperator = '~';

        protected static string Combine(string orderType, string prodOrdID)
        {
            if (string.IsNullOrWhiteSpace(orderType) && string.IsNullOrWhiteSpace(prodOrdID))
            {
                return string.Empty;
            }

            return string.Join(seperator.ToString(), orderType, prodOrdID);
        }

        protected static string[] Split(string orderTypeProdOrdIDString)
        {
            if (!string.IsNullOrWhiteSpace(orderTypeProdOrdIDString) && orderTypeProdOrdIDString.Contains(seperator.ToString()))
            {
                var split = orderTypeProdOrdIDString.Split(seperator);
                if (split.Length != 2)
                {
                    throw new ArgumentException(nameof(orderTypeProdOrdIDString));
                }
                return split;
            }

            return null;
        }

        protected static AMProdItem AMProdItemKeys(string orderTypeProdOrdIDString)
        {
            var strings = Split(orderTypeProdOrdIDString);
            if (strings != null && strings.Length == 2)
            {
                return new AMProdItem {OrderType = strings[0], ProdOrdID = strings[1]};
            }

            return null;
        }

        protected static string FormatString(string aString, int length)
        {
            if (string.IsNullOrWhiteSpace(aString))
            {
                return string.Empty.PadRight(length);
            }

            var newString = aString.Trim();

            if (newString.Length < length)
            {
                return newString.PadRight(length);
            }

            return newString.Substring(0,length);
        }

        public virtual bool Add(AMMTran ammTran)
        {
            if (ammTran == null 
                || string.IsNullOrWhiteSpace(ammTran.OrderType)
                || string.IsNullOrWhiteSpace(ammTran.ProdOrdID))
            {
                return false;
            }

            return base.Add(Combine(ammTran.OrderType, ammTran.ProdOrdID));
        }

        public virtual bool Add(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return false;
            }

            return base.Add(Combine(amProdItem.OrderType, amProdItem.ProdOrdID));
        }

        public virtual List<AMProdItem> KeysList()
        {
            var list = new List<AMProdItem>();
            foreach (var aString in List)
            {
                list.Add(AMProdItemKeys(aString));
            }
            return list;
        }

    }
}