using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.CO
{
    public class COAutoNumberingAttribute : PXEventSubscriberAttribute,
                                            IPXFieldVerifyingSubscriber,
                                            IPXFieldDefaultingSubscriber,
                                            IPXRowPersistingSubscriber,
                                            IPXRowPersistedSubscriber
    {
        public const string NewValue = "<NEW>";
        private bool _AutoNumbering;
        private Type _AutoNumberingField;
        private BqlCommand _LastNumberCommand;
        public virtual Type LastNumberField { get; private set; }
        public virtual string Prefix { get; private set; }

        public COAutoNumberingAttribute(Type autoNumbering)
        {
            if (autoNumbering != null &&
                (typeof(IBqlSearch).IsAssignableFrom(autoNumbering) ||
                typeof(IBqlField).IsAssignableFrom(autoNumbering) &&
                autoNumbering.IsNested))
            {
                _AutoNumberingField = autoNumbering;
            }
            else
            {
                throw new PXArgumentException("autoNumbering");
            }
        }
        public COAutoNumberingAttribute(Type autoNumbering, Type lastNumberField)
            : this(autoNumbering)
        {
            LastNumberField = lastNumberField;
            CreateLastNumberCommand();
        }
        private void CreateLastNumberCommand()
        {
            _LastNumberCommand = null;
            if (LastNumberField != null)
            {
                if (typeof(IBqlSearch).IsAssignableFrom(LastNumberField))
                    _LastNumberCommand = BqlCommand.CreateInstance(LastNumberField);
                else if (typeof(IBqlField).IsAssignableFrom(LastNumberField) &&
                LastNumberField.IsNested)
                    _LastNumberCommand = BqlCommand.CreateInstance(
                    typeof(Search<>), LastNumberField);
            }
            if (_LastNumberCommand == null)
                throw new PXArgumentException("lastNumberField");
        }

        public override void CacheAttached(PXCache sender)
        {
            BqlCommand command = null;
            Type autoNumberingField = null;
            // Create the BqlCommand from Search<>
            if (typeof(IBqlSearch).IsAssignableFrom(_AutoNumberingField))
            {
                command = BqlCommand.CreateInstance(_AutoNumberingField);
                autoNumberingField = ((IBqlSearch)command).GetField();
            }
            // Otherwise, create the Bql command from the field
            else
            {
                command = BqlCommand.CreateInstance(
                typeof(Search<>), _AutoNumberingField);
                autoNumberingField = _AutoNumberingField;
            }
            // In CacheAttached, we can get the reference to the graph

            PXView view = new PXView(sender.Graph, true, command);
            object row = view.SelectSingle();
            if (row != null)
            {
                _AutoNumbering = (bool)view.Cache.GetValue(
                row, autoNumberingField.Name);
            }
        }
        public virtual void FieldDefaulting(
PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (_AutoNumbering)
            {
                e.NewValue = NewValue;
            }
        }
        public virtual void FieldVerifying(
        PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (_AutoNumbering &&
            PXSelectorAttribute.Select(sender, e.Row, _FieldName, e.NewValue)
            == null)
            {
                e.NewValue = NewValue;
            }
        }

        protected virtual string GetNewNumber(PXCache sender, Type setupType)
        {
            if (_LastNumberCommand == null)
                CreateLastNumberCommand();
            PXView view = new PXView(sender.Graph, false, _LastNumberCommand);
            // Get the record from Setup
            object row = view.SelectSingle();
            if (row == null) return null;
            // Get the last assigned number
            string lastNumber = (string)view.Cache.GetValue(
            row, LastNumberField.Name);
            char[] symbols = lastNumber.ToCharArray();
            // Increment the last number
            for (int i = symbols.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(symbols[i])) break;
                if (symbols[i] < '9')
                {
                    symbols[i]++;
                    break;
                }
                symbols[i] = '0';
            }
            lastNumber = new string(symbols);
            // Update the last number in the PXCache object for Setup
            view.Cache.SetValue(row, LastNumberField.Name, lastNumber);
            PXCache setupCache = sender.Graph.Caches[setupType];
            setupCache.Update(row);
            setupCache.PersistUpdated(row);
            // Insert the document number with the prefix
            if (!string.IsNullOrEmpty(Prefix))
            {
                lastNumber = Prefix + lastNumber;
            }
            return lastNumber;
        }

        public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            // When a new record is inserted into the database table
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
            {
                Type setupType = BqlCommand.GetItemType(_AutoNumberingField);
                string lastNumber = GetNewNumber(sender, setupType);
                if (lastNumber != null)
                {
                    // Updating the document number in the PXCache
                    // object for Document
                    sender.SetValue(e.Row, _FieldOrdinal, lastNumber);
                }
            }
        }

        public virtual void RowPersisted(
           PXCache sender, PXRowPersistedEventArgs e)
        {
            // If the database transaction doesn't succeed
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert &&
            e.TranStatus == PXTranStatus.Aborted)
            {
                // Roll back the document number to the default value
                sender.SetValue(e.Row, _FieldOrdinal, NewValue);
                // If transaction isn't successful, remove the setup record; 
                // it hasn't been saved because of transaction rollback
                Type setupType = BqlCommand.GetItemType(_AutoNumberingField);
                sender.Graph.Caches[setupType].Clear();
            }
        }

        public static void SetLastNumberField<Field>(PXCache sender, object row,
        Type lastNumberField)
        where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attribute in
            sender.GetAttributes<Field>(row))
            {
                if (attribute is COAutoNumberingAttribute)
                {
                    COAutoNumberingAttribute attr = (COAutoNumberingAttribute)attribute;
                    attr.LastNumberField = lastNumberField;
                    attr.CreateLastNumberCommand();
                }
            }
        }

        public static void SetPrefix<Field>(PXCache sender, object row, string prefix)
        where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attribute in
            sender.GetAttributes<Field>(row))
            {
                if (attribute is COAutoNumberingAttribute)
                {
                    ((COAutoNumberingAttribute)attribute).Prefix = prefix;
                }
            }
        } 
    }
}
