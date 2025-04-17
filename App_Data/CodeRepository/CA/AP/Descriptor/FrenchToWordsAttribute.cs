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
using PX.Objects.AP;
using System;

namespace PX.Objects.Localizations.CA.AP
{
    /// <summary>
    /// This class is mostly a copy of PX.Objects.AP.ToWordsAttribute with a twist to be able to convert amounts
    /// to letters when the locale is French.
    /// </summary>
    public class FrenchToWordsAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        protected string _DecimalField = null;
        protected short? _Precision = null;

        public FrenchToWordsAttribute(Type DecimalField)
        {
            _DecimalField = DecimalField.Name;
        }

        public FrenchToWordsAttribute(short Precision)
        {
            _Precision = Precision; ;
        }

        public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 255, null, _FieldName, null, null, null, null, null, false, null);

            object DecimalVal;
            if (!string.IsNullOrEmpty(_DecimalField))
            {
                DecimalVal = sender.GetValue(e.Row, _DecimalField);
                sender.RaiseFieldSelecting(_DecimalField, e.Row, ref DecimalVal, true);
            }
            else
            {
                DecimalVal = PXDecimalState.CreateInstance(e.ReturnValue, (short)_Precision, _FieldName, false, 0, Decimal.MinValue, Decimal.MaxValue);
            }

            if (DecimalVal is PXDecimalState)
            {

                if (((PXDecimalState)DecimalVal).Value == null)
                {
                    e.ReturnValue = string.Empty;
                    return;
                }

                e.ReturnValue = ConvertToWords((decimal) ((PXDecimalState) DecimalVal).Value, ((PXDecimalState) DecimalVal).Precision);
            }
        }

        public virtual string ConvertToWords(decimal value, int precision)
        {
            string amountInWords;

            if (string.Compare(PXLocalesProvider.GetCurrentLocale(), "fr-CA", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                amountInWords = AmountToLetterHelper.NombreLettre(value);
            }
            else
            {
                amountInWords = LangEN.ToWords(value, precision);

                // The replacement below was originaly made at the "report" level but it has been decided
                // to do it here as to not have to not have to remember to do it in every report.
                amountInWords = amountInWords.Replace(" Only", " and 00/100");

                // Again, the conversion to uppercase was originaly made at the "report" level but it has been
                // decided to do it here to be consistent with the French version and also to not have to
                // remember to do it in every report.
                amountInWords = amountInWords.ToUpper(); 
            }

            return amountInWords;
        }
    }
}
