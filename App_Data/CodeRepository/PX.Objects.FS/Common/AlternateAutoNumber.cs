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
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    public abstract class AlternateAutoNumberAttribute : AutoNumberAttribute
    {
        private string originalRefNbr = null;

        protected abstract string GetInitialRefNbr(string baseRefNbr);

        public virtual string GetNextRefNbr(string baseRefNbr, string lastRefNbr)
        {
            string newNumberSymbol = GetNewNumberSymbol();
            if (string.IsNullOrWhiteSpace(lastRefNbr)
                || (newNumberSymbol != null && lastRefNbr.Trim() == newNumberSymbol.Trim())
            )
            {
                return GetInitialRefNbr(baseRefNbr);
            }
            else
            {
                lastRefNbr = lastRefNbr.Trim();
                int pos = lastRefNbr.LastIndexOf("-") + 1;
                int intNewNumber = int.Parse(lastRefNbr.Substring(pos)) + 1;
                string strNewNumber = intNewNumber.ToString().Trim();

                if (strNewNumber.Length < lastRefNbr.Length - pos)
                {
                    strNewNumber = strNewNumber.PadLeft(lastRefNbr.Length - pos, '0');
                }

                return lastRefNbr.Substring(0, pos) + strNewNumber;
            }
        }

        public AlternateAutoNumberAttribute(Type setupField, Type dateField)
            : base(setupField, dateField)
        {
        }

        public AlternateAutoNumberAttribute()
            : base(typeof(FSSetup.empSchdlNumberingID), typeof(AccessInfo.businessDate))
        {
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Aborted && e.Operation == PXDBOperation.Insert)
            {
                if (originalRefNbr != null)
                {
                    sender.SetValue(e.Row, _FieldName, originalRefNbr);
                    sender.Normalize();
                }
            }

            if (e.TranStatus == PXTranStatus.Aborted || e.TranStatus == PXTranStatus.Completed)
            {
                originalRefNbr = null;
            }
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.Operation == PXDBOperation.Insert)
            {
                originalRefNbr = (string)sender.GetValue(e.Row, _FieldName);

                if (SetRefNbr(sender, e.Row) == false)
                {
                    throw new AutoNumberException();
                }
                else 
                { 
                    sender.Normalize();
                }
            }
        }

        protected abstract bool SetRefNbr(PXCache cache, object row);
    }
}