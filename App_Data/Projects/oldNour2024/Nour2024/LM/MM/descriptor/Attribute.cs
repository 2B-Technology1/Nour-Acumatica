using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using System.Linq;
using PX.Objects.PM;
using PX.SM;
using PX.CS;

using PX.Objects.CS;

namespace MyMaintaince
{
    /**
    public class AutoNumberAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXFieldSelectingSubscriber
    {
        private Type _doctypeField;
        private string[] _doctypeValues;
        private Type[] _setupFields;
        private string[] _setupValues;

        private string _dateField;
        private Type _dateType;

        private string _numberingID;
        private DateTime? _dateTime;

        public static void SetNumberingId<Field>(PXCache cache, string key, string value)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
            {
                if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Cache && ((AutoNumberAttribute)attr)._doctypeValues.Length > 0)
                {
                    int i;
                    if ((i = Array.IndexOf(((AutoNumberAttribute)attr)._doctypeValues, key)) >= 0)
                    {
                        ((AutoNumberAttribute)attr)._setupValues[i] = value;
                    }
                }
            }
        }

        public static void SetNumberingId<Field>(PXCache cache, string value)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
            {
                if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Cache && ((AutoNumberAttribute)attr)._doctypeValues.Length == 0)
                {
                    ((AutoNumberAttribute)attr)._setupValues[0] = value;
                }
            }
        }

        public static NumberingSequence GetNumberingSequence(string numberingID, int? branchID, DateTime? date)
        {
            if (numberingID == null || date == null)
                return null;

            PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
                new PXDataField<NumberingSequence.endNbr>(),
                new PXDataField<NumberingSequence.lastNbr>(),
                new PXDataField<NumberingSequence.startNbr>(),
                new PXDataField<NumberingSequence.warnNbr>(),
                new PXDataField<NumberingSequence.nbrStep>(),
                new PXDataField<NumberingSequence.numberingSEQ>(),
                new PXDataField<NumberingSequence.nBranchID>(),
                new PXDataField<NumberingSequence.startNbr>(),
                new PXDataField<NumberingSequence.startDate>(),
                new PXDataField<NumberingSequence.createdByID>(),
                new PXDataField<NumberingSequence.createdByScreenID>(),
                new PXDataField<NumberingSequence.createdDateTime>(),
                new PXDataField<NumberingSequence.lastModifiedByID>(),
                new PXDataField<NumberingSequence.lastModifiedByScreenID>(),
                new PXDataField<NumberingSequence.lastModifiedDateTime>(),
                new PXDataFieldValue<NumberingSequence.numberingID>(PXDbType.VarChar, 255, numberingID),
                new PXDataFieldValue<NumberingSequence.nBranchID>(PXDbType.Int, 4, branchID, PXComp.EQorISNULL),
                new PXDataFieldValue<NumberingSequence.startDate>(PXDbType.DateTime, 4, date, PXComp.LE),
                new PXDataFieldOrder<NumberingSequence.nBranchID>(true),
                new PXDataFieldOrder<NumberingSequence.startDate>(true)
                );
            {
                if (record == null)
                    return null;

                return new NumberingSequence
                {
                    EndNbr = record.GetString(0),
                    LastNbr = record.GetString(1) ?? record.GetString(2),
                    WarnNbr = record.GetString(3),
                    NbrStep = (int)record.GetInt32(4),
                    NumberingSEQ = (int)record.GetInt32(5),
                    NBranchID = (int?)record.GetInt32(6),
                    StartNbr = record.GetString(7),
                    StartDate = record.GetDateTime(8),
                    CreatedByID = record.GetGuid(9),
                    CreatedByScreenID = record.GetString(10),
                    CreatedDateTime = record.GetDateTime(11),
                    LastModifiedByID = record.GetGuid(12),
                    LastModifiedByScreenID = record.GetString(13),
                    LastModifiedDateTime = record.GetDateTime(14)
                };
            }
        }

        private void getfields(PXCache sender, object row)
        {
            PXCache cache;
            Type _setupType = null;
            string _setupField = null;
            BqlCommand _Select = null;

            _numberingID = null;

            if (_doctypeField != null)
            {
                string doctypeValue = (string)sender.GetValue(row, _doctypeField.Name);

                int i;
                if ((i = Array.IndexOf(_doctypeValues, doctypeValue)) >= 0 && _setupValues[i] != null)
                {
                    _numberingID = _setupValues[i];
                }
                else if (i >= 0 && _setupFields[i] != null)
                {
                    if (typeof(IBqlSearch).IsAssignableFrom(_setupFields[i]))
                    {
                        _Select = BqlCommand.CreateInstance(_setupFields[i]);
                        _setupType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
                        _setupField = ((IBqlSearch)_Select).GetField().Name;
                    }
                    else if (_setupFields[i].IsNested && typeof(IBqlField).IsAssignableFrom(_setupFields[i]))
                    {
                        _setupField = _setupFields[i].Name;
                        _setupType = BqlCommand.GetItemType(_setupFields[i]);
                    }
                }
            }
            else if ((_numberingID = _setupValues[0]) != null)
            {
            }
            else if (typeof(IBqlSearch).IsAssignableFrom(_setupFields[0]))
            {
                _Select = BqlCommand.CreateInstance(_setupFields[0]);
                _setupType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
                _setupField = ((IBqlSearch)_Select).GetField().Name;
            }
            else if (_setupFields[0].IsNested && typeof(IBqlField).IsAssignableFrom(_setupFields[0]))
            {
                _setupField = _setupFields[0].Name;
                _setupType = BqlCommand.GetItemType(_setupFields[0]);
            }

            if (_Select != null)
            {
                PXView view = sender.Graph.TypedViews.GetView(_Select, false);
                int startRow = -1;
                int totalRows = 0;
                List<object> source = view.Select(
                    new object[] { row },
                    null,
                    null,
                    null,
                    null,
                    null,
                    ref startRow,
                    1,
                    ref totalRows);
                if (source != null && source.Count > 0)
                {
                    object item = source[source.Count - 1];
                    if (item != null && item is PXResult)
                    {
                        item = ((PXResult)item)[_setupType];
                    }
                    _numberingID = (string)sender.Graph.Caches[_setupType].GetValue(item, _setupField);
                }
            }
            else if (_setupType != null)
            {
                cache = sender.Graph.Caches[_setupType];
                if (cache.Current != null && _numberingID == null)
                {
                    _numberingID = (string)cache.GetValue(cache.Current, _setupField);
                }
            }

            cache = sender.Graph.Caches[_dateType];
            if (sender.GetItemType() == _dateType)
            {
                _dateTime = (DateTime?)cache.GetValue(row, _dateField);
            }
            else if (cache.Current != null)
            {
                _dateTime = (DateTime?)cache.GetValue(cache.Current, _dateField);
            }
        }

        public AutoNumberAttribute(Type doctypeField, Type dateField, string[] doctypeValues, Type[] setupFields)
        {
            _dateField = dateField.Name;
            _dateType = BqlCommand.GetItemType(dateField);

            _doctypeField = doctypeField;
            _doctypeValues = doctypeValues;
            _setupFields = setupFields;
        }


        public AutoNumberAttribute(Type setupField, Type dateField)
            : this(null, dateField, new string[] { }, new Type[] { setupField })
        {
        }

        public AutoNumberAttribute(Type setupField, Type dateField, Boolean isUserEntered, String stringSymbol, String numberingID)
        {
            _dateField = dateField.Name;
            _dateType = BqlCommand.GetItemType(dateField);
            _UserNumbering = new ObjectRef<bool?>();
            _UserNumbering.Value = isUserEntered;
            _NewSymbol = new ObjectRef<string>();
            _NewSymbol.Value = stringSymbol;
            _numberingID = numberingID;
            //
            _doctypeField = null;
            _doctypeValues = new string[] { };
            _setupFields = new Type[] { setupField };
        }


        protected ObjectRef<bool?> _UserNumbering;
        protected bool? UserNumbering
        {
            get
            {
                return _UserNumbering.Value;
            }
            set
            {
                _UserNumbering.Value = value;
            }
        }

        protected ObjectRef<string> _NewSymbol;
        protected string NewSymbol
        {
            get
            {
                return _NewSymbol.Value;
            }
            set
            {
                _NewSymbol.Value = value;
            }
        }

        public enum NullNumberingMode
        {
            ViewOnly,
            UserNumbering
        }

        protected NullNumberingMode NullMode;
        protected string NullString;

        public class Numberings : IPrefetchable
        {
            protected Dictionary<string, string> _items = new Dictionary<string, string>();

            void IPrefetchable.Prefetch()
            {
                _items.Clear();

                foreach (PXDataRecord record in PXDatabase.SelectMulti<Numbering>(
                    new PXDataField<Numbering.numberingID>(),
                    PXDBLocalizableStringAttribute.GetValueSelect("Numbering", "NewSymbol", false),
                    new PXDataField<Numbering.userNumbering>()))
                {
                    string numberingID = record.GetString(0);
                    string newSymbol = record.GetString(1);
                    bool? userNumbering = record.GetBoolean(2);
                    //string numberingID = record.GetString(0);//might be INISSUE
                    //string newSymbol = "<NEW>";
                    //bool? userNumbering = false;

                    _items[numberingID] = userNumbering == true ? null : newSymbol;
                }
            }

            public bool TryGetValue(string key, out string value)
            {
                return _items.TryGetValue(key, out value);
            }

            public ReadOnlyDictionary<string, string> GetNumberings()
            {
                return new ReadOnlyDictionary<string, string>(_items);
            }
        }

        public string this[string key]
        {
            get
            {
                string currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (!PXDBLocalizableStringAttribute.IsEnabled)
                {
                    currentLanguage = "";
                }
                Numberings items = PXDatabase.GetSlot<Numberings>(typeof(Numberings).Name + currentLanguage, typeof(Numbering));
                if (items != null)
                {
                    string value;
                    if (!items.TryGetValue(key, out value))
                    {
                        throw new PXException(PX.Objects.CS.Messages.NumberingIDNull);
                    }
                    return value;
                }
                return null;
            }
        }

        //poo : Changed
        protected virtual string GetNewNumber()
        {
            return GetNewNumber(null);
        }
        
        protected virtual string GetNewNumber()
        {
            return GetNewNumber("INISSUE");
        }

        protected virtual string GetNewNumber(string numberingID)
        {
            numberingID = numberingID ?? _numberingID;

            if (numberingID != null)
            {
                return (UserNumbering = (NewSymbol = this[numberingID]) == null) != true ? " " + NewSymbol : null;
            }
            else if (NullMode == NullNumberingMode.UserNumbering)
            {
                return NullString;
            }
            else
            {
                return " " + PX.Objects.CS.Messages.NoNumberNewSymbol;
            }
        }

        void IPXFieldDefaultingSubscriber.FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            getfields(sender, e.Row);
            e.NewValue = GetNewNumber();
        }

        void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (sender.Locate(e.Row) == null && !(sender.Graph is PXGenericInqGrph))
            {
                string oldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

                if (UserNumbering != true && oldValue != null)
                {
                    e.NewValue = oldValue;
                }
            }
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _UserNumbering = new ObjectRef<bool?>();
            _NewSymbol = new ObjectRef<string>();
            _setupValues = new string[_setupFields.Length];

            string key = sender.GetItemType().FullName + "_AutoNumber";
            HashSet<string> fields;
            if ((fields = PXContext.GetSlot<HashSet<string>>(key)) == null)
            {
                PXContext.SetSlot<HashSet<string>>(key, fields = new HashSet<string>());
            }

            fields.Add(_FieldName);

            bool IsKey;
            if (!(IsKey = sender.Keys.IndexOf(_FieldName) > 0))
            {
                foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(_FieldName))
                {
                    if (attr is PXDBFieldAttribute && (IsKey = ((PXDBFieldAttribute)attr).IsKey))
                    {
                        break;
                    }
                }
            }

            if (!IsKey)
            {
                NullString = string.Empty;
                NullMode = NullNumberingMode.UserNumbering;
            }
            else
            {
                sender.Graph.RowSelected.AddHandler(sender.GetItemType(), Parameter_RowSelected);
                sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _FieldName, Parameter_CommandPreparing);
            }
        }

        protected virtual void Parameter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row != null)
            {
                if (UserNumbering == null || NewSymbol == null)
                {
                    getfields(sender, e.Row);
                    GetNewNumber();
                }
            }
        }

        protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            string Key;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
                (e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (Key = e.Value as string) != null)
            {
                if (UserNumbering == false && Key.Length > 1 && string.Equals(Key.Substring(1), NewSymbol))
                {
                    e.DataValue = null;
                    e.Cancel = true;
                }
            }
        }

        protected string LastNbr;
        protected string WarnNbr;
        protected string NewNumber;
        protected string NotSetNumber;
        protected int? NumberingSEQ;
        protected object _KeyToAbort = null;
        protected static bool IS_SEPARATE_SCOPE { get { return WebConfig.GetBool("EnableAutoNumberingInSeparateConnection", false); } }

        public static string GetKeyToAbort(PXCache cache, object row, string fieldName)
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(row, fieldName))
            {
                if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Item)
                {
                    return (string)((AutoNumberAttribute)attr)._KeyToAbort;
                }
            }
            return null;
        }

        public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
            {
                return;
            }

            getfields(sender, e.Row);

            if ((NotSetNumber = GetNewNumber()) == NullString)
            {
                object keyValue = sender.GetValue(e.Row, _FieldName);
                Numberings items = PXDatabase.GetSlot<Numberings>(typeof(Numberings).Name, typeof(Numbering));
                if (items != null && keyValue != null)
                {
                    foreach (KeyValuePair<string, string> item in items.GetNumberings())
                    {
                        if (item.Value == (string)keyValue || GetNewNumber(item.Key) == (string)keyValue)
                            throw new PXException(PX.Objects.CS.Messages.DocumentNbrEqualNewSymbol, (string)keyValue);
                    }
                }

                return;
            }

            if (_numberingID != null && _dateTime != null)
            {
                NewNumber = GetNextNumber(sender, e.Row, _numberingID, _dateTime, NewNumber, out LastNbr, out WarnNbr, out NumberingSEQ);

                if (NewNumber.CompareTo(WarnNbr) >= 0)
                {
                    PXUIFieldAttribute.SetWarning(sender, e.Row, _FieldName, PX.Objects.CS.Messages.WarningNumReached);
                }
                _KeyToAbort = sender.GetValue(e.Row, _FieldName);
                sender.SetValue(e.Row, _FieldName, NewNumber);
            }
            else if (string.IsNullOrEmpty(NewNumber = (string)sender.GetValue(e.Row, _FieldName)) || string.Equals(NewNumber, NotSetNumber))
            {
                throw new AutoNumberException(_numberingID);
            }
        }

        public static string GetNextNumber(PXCache sender, object data, string numberingID, DateTime? dateTime)
        {
            string LastNbr;
            string WarnNbr;
            int? NumberingSEQ;

            return GetNextNumber(sender, data, numberingID, dateTime, null, out LastNbr, out WarnNbr, out NumberingSEQ);
        }

        protected static string GetNextNumberInt(PXCache sender, object data, string numberingID, DateTime? dateTime, string lastAssigned, out string LastNbr, out string WarnNbr, out int? NumberingSEQ)
        {
            if (numberingID != null && dateTime != null)
            {
                int? branchID = sender.Graph.Accessinfo.BranchID;
                if (data != null && sender.Fields.Contains("BranchID"))
                {
                    object state = sender.GetStateExt(data, "BranchID");
                    if (state is PXFieldState && ((PXFieldState)state).Required == true)
                    {
                        branchID = (int?)sender.GetValue(data, "BranchID");
                    }
                }

                //LastNbr ,WarnNbr ,NumberingSEQ , NbrStep
                NumberingSequence sequence = GetNumberingSequence(numberingID, branchID, dateTime);
                sequence.NbrStep = 1;
                if (sequence == null)
                    throw new AutoNumberException(numberingID);

                //LastNbr = sequence.LastNbr;
                //WarnNbr = sequence.WarnNbr;
                //NumberingSEQ = sequence.NumberingSEQ;

                LastNbr = sequence.LastNbr;
                WarnNbr = sequence.WarnNbr;
                NumberingSEQ = sequence.NumberingSEQ;

                string newNumber = NextNumber(LastNbr, sequence.NbrStep ?? 0);
                if (String.Equals(lastAssigned, newNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    newNumber = NextNumber(newNumber, sequence.NbrStep ?? 0);
                }

                if (newNumber.CompareTo(sequence.EndNbr) >= 0)
                {
                    throw new PXException(PX.Objects.CS.Messages.EndOfNumberingReached, numberingID);
                }

                try
                {
                    if (LastNbr != sequence.StartNbr)
                    {
                        if (!PXDatabase.Update<NumberingSequence>(
                            new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
                            new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
                            new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ),
                            new PXDataFieldRestrict<NumberingSequence.lastNbr>(LastNbr),
                            PXDataFieldRestrict.OperationSwitchAllowed))
                        {
                            PXDatabase.Update<NumberingSequence>(
                                new PXDataFieldAssign<NumberingSequence.nbrStep>(sequence.NbrStep),
                                new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
                                new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ));
                            using (PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
                                new PXDataField<NumberingSequence.lastNbr>(),
                                new PXDataFieldValue<NumberingSequence.numberingID>(numberingID),
                                new PXDataFieldValue<NumberingSequence.numberingSEQ>(NumberingSEQ)))
                            {
                                if (record != null)
                                {
                                    LastNbr = record.GetString(0);
                                    newNumber = NextNumber(LastNbr, sequence.NbrStep ?? 0);
                                    if (newNumber.CompareTo(sequence.EndNbr) >= 0)
                                    {
                                        throw new PXException(PX.Objects.CS.Messages.EndOfNumberingReached, numberingID);
                                    }
                                }
                            }
                            PXDatabase.Update<NumberingSequence>(
                                new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
                                new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
                                new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ));
                        }
                    }
                    else
                    {
                        PXDatabase.Update<NumberingSequence>(
                            new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
                            new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
                            new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ),
                            PXDataFieldRestrict.OperationSwitchAllowed);
                    }
                }
                catch (PXDbOperationSwitchRequiredException)
                {
                    PXDatabase.Insert<NumberingSequence>(
                        new PXDataFieldAssign<NumberingSequence.endNbr>(PXDbType.VarChar, 15, sequence.EndNbr),
                        new PXDataFieldAssign<NumberingSequence.lastNbr>(PXDbType.VarChar, 15, newNumber),
                        new PXDataFieldAssign<NumberingSequence.warnNbr>(PXDbType.VarChar, 15, sequence.WarnNbr),
                        new PXDataFieldAssign<NumberingSequence.nbrStep>(PXDbType.Int, 4, sequence.NbrStep ?? 0),
                        new PXDataFieldAssign<NumberingSequence.startNbr>(PXDbType.VarChar, 15, sequence.StartNbr),
                        new PXDataFieldAssign<NumberingSequence.startDate>(PXDbType.DateTime, sequence.StartDate),
                        new PXDataFieldAssign<NumberingSequence.createdByID>(PXDbType.UniqueIdentifier, 16, sequence.CreatedByID),
                        new PXDataFieldAssign<NumberingSequence.createdByScreenID>(PXDbType.Char, 8, sequence.CreatedByScreenID),
                        new PXDataFieldAssign<NumberingSequence.createdDateTime>(PXDbType.DateTime, 8, sequence.CreatedDateTime),
                        new PXDataFieldAssign<NumberingSequence.lastModifiedByID>(PXDbType.UniqueIdentifier, 16, sequence.LastModifiedByID),
                        new PXDataFieldAssign<NumberingSequence.lastModifiedByScreenID>(PXDbType.Char, 8, sequence.LastModifiedByScreenID),
                        new PXDataFieldAssign<NumberingSequence.lastModifiedDateTime>(PXDbType.DateTime, 8, sequence.LastModifiedDateTime),
                        new PXDataFieldAssign<NumberingSequence.numberingID>(PXDbType.VarChar, 10, numberingID),
                        new PXDataFieldAssign<NumberingSequence.nBranchID>(PXDbType.Int, 4, sequence.NBranchID)
                        );
                }

                return newNumber;
            }

            LastNbr = null;
            WarnNbr = null;
            NumberingSEQ = null;

            return null;
        }

        protected static string GetNextNumber(PXCache sender, object data, string numberingID, DateTime? dateTime, string lastAssigned, out string LastNbr, out string WarnNbr, out int? NumberingSEQ)
        {
            if (IS_SEPARATE_SCOPE)
            {
                using (new PXConnectionScope())
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        String NewNumber = GetNextNumberInt(sender, data, numberingID, dateTime, lastAssigned, out LastNbr, out WarnNbr, out NumberingSEQ);
                        ts.Complete();
                        return NewNumber;
                    }
                }
            }
            else
                return GetNextNumberInt(sender, data, numberingID, dateTime, lastAssigned, out LastNbr, out WarnNbr, out NumberingSEQ);
        }

        public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted && UserNumbering != true)
            {
                if (e.Exception is PXLockViolationException)
                {
                    try
                    {
                        string number = sender.GetValue(e.Row, _FieldOrdinal) as string;
                        if (!String.IsNullOrEmpty(number) && number == NewNumber)
                        {
                            PXDatabase.Update<NumberingSequence>(
                                new PXDataFieldAssign<NumberingSequence.lastNbr>(number),
                                new PXDataFieldRestrict<NumberingSequence.numberingID>(_numberingID),
                                new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ),
                                new PXDataFieldRestrict<NumberingSequence.lastNbr>(LastNbr));
                            ((PXLockViolationException)e.Exception).Retry = true;
                        }
                    }
                    catch
                    {
                    }
                }
                if (_KeyToAbort != null)
                {
                    sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
                }
            }
            if (e.TranStatus != PXTranStatus.Open)
            {
                _KeyToAbort = null;
            }
        }

        void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            string oldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

            if (UserNumbering == true && oldValue == null && sender.Graph.UnattendedMode && !e.ExternalCall)
            {
                throw new PXException(PX.Objects.CS.Messages.CantManualNumber, _numberingID);
            }
        }

        void IPXFieldSelectingSubscriber.FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, null, null, null, null);
            }
        }
        #region Implementation
        public static string NextNumber(string str, short count)
        {
            return NextNumber(str, (int)count);
        }

        public static string NextNumber(string str, int count)
        {
            int i;
            bool j = true;
            int intcount = Math.Abs(count);
            int sign = Math.Sign(count);

            StringBuilder bld = new StringBuilder();
            for (i = str.Length; i > 0; i--)
            {
                string c = str.Substring(i - 1, 1);

                if (Regex.IsMatch(c, "[^0-9]"))
                {
                    j = false;
                }

                if (j && Regex.IsMatch(c, "[0-9]"))
                {
                    int digit = Convert.ToInt16(c);

                    string s_count = Convert.ToString(intcount);
                    int digit2 = Convert.ToInt16(s_count.Substring(s_count.Length - 1, 1));

                    if (sign >= 0)
                    {
                        bld.Append((digit + digit2) % 10);

                        intcount -= digit2;
                        intcount += ((digit + digit2) - (digit + digit2) % 10);
                    }
                    else
                    {
                        bld.Append((10 + digit - digit2) % 10);

                        intcount -= digit2;
                        intcount -= ((digit - digit2) - (10 + digit - digit2) % 10);
                    }

                    intcount /= 10;

                    if (intcount == 0)
                    {
                        j = false;
                    }
                }
                else
                {
                    bld.Append(c);
                }
            }

            if (intcount != 0)
            {
                throw new AutoNumberException();
            }

            char[] chars = bld.ToString().ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static bool CanNextNumber(string str)
        {
            try
            {
                NextNumber(str, 1);
                return true;
            }
            catch (AutoNumberException)
            { return false; }
        }

        public static string NextNumber(string str)
        {
            try
            {
                return NextNumber(str, 1);
            }
            catch (AutoNumberException)
            {
                return str;
            }
        }

        #endregion
    }
    **/
    
    /**
    public class AutoNumberAttribute : PXEventSubscriberAttribute,
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

        public AutoNumberAttribute(Type autoNumbering)
        {
            if (autoNumbering != null && (typeof(IBqlSearch).IsAssignableFrom(autoNumbering) || typeof(IBqlField).IsAssignableFrom(autoNumbering) && autoNumbering.IsNested))
            {
                _AutoNumberingField = autoNumbering;
            }
            else
            {
                throw new PXArgumentException("autoNumbering");
            }
        }

        public AutoNumberAttribute(Type autoNumbering, Type lastNumberField)
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
            }// Otherwise, create the Bql command from the field
            else
            {
                command = BqlCommand.CreateInstance(typeof(Search<>), _AutoNumberingField);
                autoNumberingField = _AutoNumberingField;
            }
            // In CacheAttached, we can get the reference to the graph
            PXView view = new PXView(sender.Graph, true, command);
            object row = view.SelectSingle();
            if (row != null)
            {
                _AutoNumbering = (bool)view.Cache.GetValue(row, autoNumberingField.Name);
            }
        }



        public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (_AutoNumbering)
            {
                e.NewValue = NewValue;
            }
        }

        public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
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
            string lastNumber = (string)view.Cache.GetValue(row, LastNumberField.Name);
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
                if (_AutoNumbering == true)
                {
                    string lastNumber = GetNewNumber(sender, setupType);
                    if (lastNumber != null)
                    {
                        // Updating the document number in the PXCache
                        // object for Document
                        sender.SetValue(e.Row, _FieldOrdinal, lastNumber);
                    }
                }

            }
        }



        public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            // If the database transaction doesn't succeed
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
            {
                // Roll back the document number to the default value
                sender.SetValue(e.Row, _FieldOrdinal, NewValue);
                // If transaction isn't successful, remove the setup record;
                // it hasn't been saved because of transaction rollback
                Type setupType = BqlCommand.GetItemType(_AutoNumberingField);
                sender.Graph.Caches[setupType].Clear();
            }
        }


        public static void SetLastNumberField<Field>(PXCache sender, object row, Type lastNumberField) where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attribute in
            sender.GetAttributes<Field>(row))
            {
                if (attribute is AutoNumberAttribute)
                {
                    AutoNumberAttribute attr = (AutoNumberAttribute)attribute;
                    attr.LastNumberField = lastNumberField;
                    attr.CreateLastNumberCommand();
                }
            }
        }

        public static void SetPrefix<Field>(PXCache sender, object row, string prefix) where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attribute in
            sender.GetAttributes<Field>(row))
            {
                if (attribute is AutoNumberAttribute)
                {
                    ((AutoNumberAttribute)attribute).Prefix = prefix;
                }
            }
        }

    }
    **/
}
