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

namespace PX.Objects.FS
{
    internal static class GraphHelper
    {
        public static void RaiseRowPersistingException<Field>(PXCache cache, object row)
            where Field : IBqlField
        {
            var fieldName = typeof(Field).Name;
            string fieldLabel = PXUIFieldAttribute.GetDisplayName<Field>(cache);

            cache.RaiseExceptionHandling<Field>(row,
                                                null,
                                                new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, fieldLabel)));

            throw new PXRowPersistingException(fieldName, null, ErrorMessages.FieldIsEmpty, fieldLabel);
        }

        public static PXAction GetSaveAction(this PXGraph graph)
        {
            PXAction saveAction = null;
            Type primary = !String.IsNullOrEmpty(graph.PrimaryView) ? graph.Views[graph.PrimaryView].GetItemType() : null;

            foreach (PXAction action in graph.Actions.Values)
            {
                PXButtonState state = action.GetState(null) as PXButtonState;

                if (state != null && state.SpecialType == PXSpecialButtonType.Save)
                {
                    if (primary == null || state.ItemType == null || primary == state.ItemType || primary.IsSubclassOf(state.ItemType))
                    {
                        saveAction = action;
                        break;
                    }
                }
            }

            if (saveAction == null)
            {
                throw new PXException("There is not a Save action in the graph " + graph.GetType());
            }

            return saveAction;
        }

        public static PXAction GetDeleteAction(this PXGraph graph)
        {
            PXAction deleteAction = null;
            Type primary = !String.IsNullOrEmpty(graph.PrimaryView) ? graph.Views[graph.PrimaryView].GetItemType() : null;

            foreach (PXAction action in graph.Actions.Values)
            {
                PXButtonState state = action.GetState(null) as PXButtonState;

                if (state != null && state.SpecialType == PXSpecialButtonType.Delete)
                {
                    if (primary == null || state.ItemType == null || primary == state.ItemType || primary.IsSubclassOf(state.ItemType))
                    {
                        deleteAction = action;
                        break;
                    }
                }
            }

            if (deleteAction == null || deleteAction.GetEnabled() == false)
            {
                throw new PXException(TX.Error.APPOINTMENT_NOT_DELETABLE);
            }

            return deleteAction;
        }

        public static void SetValueExtIfDifferent<Field>(this PXCache cache, object data, object newValue, bool verifyAcceptanceOfNewValue = true)
            where Field : IBqlField
        {
            object currentValue = cache.GetValue<Field>(data);

            if ((currentValue == null && newValue != null)
                || (currentValue != null && newValue == null)
                || (currentValue != null && currentValue.Equals(newValue) == false))
            {
                cache.SetValueExt<Field>(data, newValue);

                if (verifyAcceptanceOfNewValue)
                {
                    currentValue = cache.GetValue<Field>(data);

                    if (AreEquivalentValues(currentValue, newValue) == false)
                    {
                        PXFieldState fieldState;
                        string fieldMessage = string.Empty;

                        try
                        {
                            fieldState = (PXFieldState)cache.GetStateExt<Field>(data);
                        }
                        catch
                        {
                            fieldState = null;
                        }

                        if (fieldState != null && fieldState.Error != null)
                        {
                            fieldMessage = fieldState.Error;
                        }

                        throw new PXException(TX.Messages.ERROR_TRYING_TO_SET_A_NEWVALUE, PXUIFieldAttribute.GetDisplayName<Field>(cache), fieldMessage);
                    }
                }
            }
        }

        public static bool AreEquivalentValues(object value1, object value2)
        {
            if (value1 != null)
            {
                return AreEquivalentValuesBasedOnValue1Type(value1, value2);
            }
            else if (value2 != null)
            {
                return AreEquivalentValuesBasedOnValue1Type(value2, value1);
            }
            else
            {
                return true;
            }
        }

        public static bool AreEquivalentValuesBasedOnValue1Type(object value1, object value2)
        {
            if (value1 is null)
            {
                throw new ArgumentException();
            }

            if (value1 is string)
            {
                return AreEquivalentStrings((string)value1, value2);
            }
            else if (value1 is decimal || value1 is double)
            {
                return AreEquivalentDecimals((decimal)value1, value2);
            }
            else
            {
                return value1.Equals(value2);
            }
        }

        public static bool AreEquivalentStrings(string value1, object value2)
        {
            if (value1 is null)
            {
                throw new ArgumentException();
            }

            string str1 = ((string)value1).Trim();
            string str2 = string.Empty;

            if (value2 != null)
            {
                if (!(value2 is string))
                {
                    return false;
                }

                str2 = ((string)value2).Trim();
            }
            // Null can be considered equal to empty string

            return str1.Equals(str2);
        }

        public static bool AreEquivalentDecimals(decimal value1, object value2)
        {
            decimal dec1 = (decimal)value1;
            decimal dec2 = 0.0m;

            if (value2 != null)
            {
                if (!(value2 is decimal || value2 is double))
                {
                    return false;
                }

                dec2 = (decimal)value2;
            }
            else
            {
                // Null is not considered equal to 0
                return false;
            }

            return Math.Round(dec1, 2, MidpointRounding.AwayFromZero)
                == Math.Round(dec2, 2, MidpointRounding.AwayFromZero);
        }

        public static bool IsValueChanging(PXCache cache, object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }

            if (oldValue != null && newValue != null)
            {
                if (newValue is string && (oldValue is DateTime || oldValue is DateTime?))
                {
                    DateTime? newDateTime = SharedFunctions.TryParseHandlingDateTime(cache, newValue);

                    if (newDateTime == null)
                    {
                        return true;
                    }

                    if (newDateTime == (DateTime)oldValue)
                    {
                        return false;
                    }
                }
                else if (oldValue.Equals(newValue) == true)
                {
                    return false;
                }
            }

            return true;
        }

        public static PXSelect<Table> AddViewToPersist<Table>(PXGraph graph)
            where Table : class, IBqlTable, new()
        {
            var view = new PXSelect<Table>(graph);

            if (!graph.Views.Caches.Contains(typeof(Table)))
                graph.Views.Caches.Add(typeof(Table));

            return view;
        }
    }
}
