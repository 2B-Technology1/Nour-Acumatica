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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Common;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using PX.Common.Mail;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Web.UI;
using PX.Api;
using PX.Objects.Common.Exceptions;
using PX.Objects.Common;
using PX.Objects.CR.Workflows;
using System.Reactive.Disposables;

namespace PX.Objects.CR
{
	#region CustomerAndProspectRestrictorAttribute

	[Obsolete("Use properly configured PX.Objects.CR.BAccountAttribute instead")]
	public class CustomerAndProspectRestrictorAttribute : PXRestrictorAttribute
	{
		public CustomerAndProspectRestrictorAttribute()
			: base(typeof(Where<
					BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>>>>),
				Messages.BAccountIsType,
				typeof(BAccount.type))
		{
		}
	}

	#endregion

	#region RestrictBAccountBySource

	public abstract class RestrictBAccountBySource : PXRestrictorAttribute
	{
		protected bool ResetBAccount;
		protected Type SourceType;
		protected Type AcctCD;

		protected RestrictBAccountBySource(Type restriction, Type source, Type acctCD, string message)
			: base(restriction, message)
		{
			SourceType = source;
			AcctCD = acctCD;
		}

		public override object[] GetMessageParameters(PXCache sender, object itemres, object row)
		{
			var result = new List<object>();

			var cacheBAccount = sender.Graph.Caches[BqlCommand.GetItemType(AcctCD)];
			var msgBAccount = cacheBAccount.GetStateExt(PXResult.Unwrap(itemres, BqlCommand.GetItemType(AcctCD)), "acctCD");
			result.Add(msgBAccount);

			var sourceDacType = BqlCommand.GetItemType(SourceType);
			var cacheBranch = sender.Graph.Caches[sourceDacType];
			var sourceRow = PXResult.Unwrap(row, sourceDacType);
			if (sourceRow != null && !sourceDacType.IsAssignableFrom(sourceRow.GetType()))
				sourceRow = null;
			var msgBranch = cacheBranch.GetStateExt(sourceRow ?? cacheBranch.Current, SourceType.Name);
			result.Add(msgBranch);

			return result.ToArray();
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (SourceType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(SourceType),
					SourceType.Name,
					SourceChanged);
			}
		}
		protected virtual void SourceChanged(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (ResetBAccount == true)
			{
				if (cache.GetValuePending(e.Row, _FieldName) != PXCache.NotSetValue) return;
				try
				{
					object val = cache.GetValue(e.Row, _FieldName);
					cache.RaiseFieldVerifying(_FieldName, e.Row, ref val);
				}
				catch (PXSetPropertyException)
				{
					cache.SetValue(e.Row, _FieldName, null);
				}
			}
			else
			{
				object val;
				if (cache.GetValuePending(e.Row, SourceType.Name) != PXCache.NotSetValue
					&& cache.GetValuePending(e.Row, _FieldName) == null)
				{
					//for removing an error mark from Source field when this field is cleared
					val = null;
				}
				else
				{
					val = cache.GetValue(e.Row, _FieldName);
				}

				try
				{
					cache.RaiseFieldVerifying(_FieldName, e.Row, ref val);
				}
				catch (PXSetPropertyException ex)
				{
					string BranchCD = cache.GetValueExt(e.Row, SourceType.Name)?.ToString();
					cache.SetValue(e.Row, SourceType.Name, e.OldValue);

					ClearErrorValueIfTheValueForAnotherField(ex);

					cache.RaiseExceptionHandling(SourceType.Name, e.Row, BranchCD, ex);
				}
			}
		}

		protected virtual void ClearErrorValueIfTheValueForAnotherField(PXSetPropertyException ex)
		{
			if (ex.ErrorValue != null &&
				!string.Equals(ex.MapErrorTo, SourceType.Name, StringComparison.OrdinalIgnoreCase))
			{
				ex.ErrorValue = null;
			}
		}
	}

	#endregion

	#region RestrictBranchBySource

	public abstract class RestrictBranchBySource : PXRestrictorAttribute
	{
		public bool ResetBranch;
		protected Type SourceType;
		protected Type BranchCD;

		protected RestrictBranchBySource(Type restriction, Type source, Type branchCD, string message)
			: base(restriction, message)
		{
			SourceType = source;
			BranchCD = branchCD;
		}

		public override object[] GetMessageParameters(PXCache sender, object itemres, object row)
		{
			var result = new List<object>();

			var cacheBAccount = sender.Graph.Caches[row.GetType()];
			var msgBAccount = cacheBAccount.GetStateExt(PXResult.Unwrap(row, BqlCommand.GetItemType(SourceType)) ?? cacheBAccount.Current, SourceType.Name);
			result.Add(msgBAccount);

			var cacheBranch = sender.Graph.Caches[BqlCommand.GetItemType(BranchCD)];
			var msgBranch = cacheBranch.GetStateExt(PXResult.Unwrap(itemres, BqlCommand.GetItemType(BranchCD)), "branchCD");
			result.Add(msgBranch);

			return result.ToArray();
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (SourceType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(SourceType),
					SourceType.Name,
					SourceChanged);
			}
		}
		protected virtual void SourceChanged(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SyncBAccountCache(cache, e.Row);

			if (ResetBranch == true)
			{
				if (cache.GetValuePending(e.Row, _FieldName) != PXCache.NotSetValue) return;
				try
				{
					object val = cache.GetValue(e.Row, _FieldName);
					cache.RaiseFieldVerifying(_FieldName, e.Row, ref val);
				}
				catch (PXSetPropertyException)
				{
					cache.SetValue(e.Row, _FieldName, null);
				}
			}
			else
			{
				object val;
				if (cache.GetValuePending(e.Row, SourceType.Name) != PXCache.NotSetValue
					&& cache.GetValuePending(e.Row, _FieldName) == null)
				{
					//for removing an error mark from Source field when this field is cleared
					val = null;
				}
				else
				{
					val = cache.GetValue(e.Row, _FieldName);
				}

				try
				{
					cache.RaiseFieldVerifying(_FieldName, e.Row, ref val);
				}
				catch (PXSetPropertyException ex)
				{
					string AcctCD = cache.GetValueExt(e.Row, SourceType.Name)?.ToString();
					cache.SetValue(e.Row, SourceType.Name, e.OldValue);
					cache.RaiseExceptionHandling(SourceType.Name, e.Row, AcctCD, ex);
				}
			}
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SyncBAccountCache(sender, e.Row);
		}

		protected void SyncBAccountCache(PXCache cache, object row)
		{
			object newBAccountID = cache.GetValue(row, SourceType.Name);
			PXCache bCache = cache.Graph.Caches[typeof(BAccount)];
			if (bCache.Current != null)
			{
				object oldBAccountID = bCache.GetValue(bCache.Current, typeof(BAccount.bAccountID).Name);
				if (Object.Equals(newBAccountID, oldBAccountID))
				{
					return;
				}
			}
			BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID,
					Equal<Required<BAccount.bAccountID>>>>
				.SelectWindowed(cache.Graph, 0, 1, newBAccountID);
			bCache.Current = bAccount;
		}
	}

	#endregion

	#region AddressRevisionIDAttribute

	public class AddressRevisionIDAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		private bool IsInsertOrUpdateOperation(PXDBOperation operation) => operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update);

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsInsertOrUpdateOperation(e.Operation))
			{
				int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal) ?? 0;
				revision++;
				sender.SetValue(e.Row, _FieldOrdinal, revision);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && IsInsertOrUpdateOperation(e.Operation))
			{
				int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal);
				revision--;
				sender.SetValue(e.Row, _FieldOrdinal, revision);
			}
		}
	}

	#endregion

	#region CountryStateSelectorAttribute

	public class CountryAttribute : CountryStateSelectorAttribute
	{
		protected override string Type => "Countries";

		public CountryAttribute() : this(typeof(Search<Country.countryID>)) { }

		public CountryAttribute(Type search) : base(search)
		{
			DescriptionField = typeof(Country.description);
		}
		

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            if (e.NewValue != null
                && !Find(slot.Countries, sender, e)
                && !Find(slot.CountriesDescription, sender, e)
                && !FindRegex(slot.CountriesRegex, sender, e))
            {
                
                throw new PXSetPropertyException(ErrorMessages.ValueDoesntExist, $"[{_FieldName}]", e.NewValue);
            }
			e.Cancel = true;
		}
	}

	public class StateAttribute : CountryStateSelectorAttribute, IPXRowPersistingSubscriber
	{
		protected readonly Type CountryID;
		protected override string Type => "States";
		
	
		public StateAttribute(Type aCountryID)
			: base(BqlCommand.Compose(
				typeof(Search<,>),
				typeof(State.stateID),
				typeof(Where<,>),
				typeof(State.countryID),
				typeof(Equal<>),
				typeof(Optional<>),
				aCountryID))
		{
			CountryID = aCountryID;
			Filterable = true;

			_UnconditionalSelect = _PrimarySelect;
			DescriptionField = typeof(State.name);		
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Graph.IsContractBasedAPI)
			sender.Graph.FieldUpdated.AddHandler(CountryID.DeclaringType, CountryID.Name, CountryChanged);
		}

		protected virtual void CountryChanged(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object value = sender.GetValue(e.Row, _FieldName);
			if (value == null) return;
			try
			{
				sender.RaiseFieldVerifying(_FieldName, e.Row, ref value);
			}
			catch (Exception ex)
			{
				sender.SetValue(e.Row, _FieldName, null);
				sender.RaiseExceptionHandling(_FieldName, e.Row, value, ex);
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null
				|| sender.Graph.IsContractBasedAPI
				|| sender.Graph.IsCopyPasteContext)
				return;

			string countryID = sender.GetValue(e.Row, CountryID.Name) as string;
			if (countryID == null)
			{
				e.NewValue = null;
				return;
			}

			try
			{
				e.NewValue = ValidateStateByCountry(e.NewValue as string, countryID);
				e.Cancel = true;
			}
			catch (LocalizationPreparedException ex)
			{
				throw new PXSetPropertyException(ex.Format, ex.Args);
			}
			finally
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, null);
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			// it requires for correct working of API insert because it depends on order at UI:  state - first
			// but it imposible to write adapter for it
			if (sender.Graph.IsContractBasedAPI)
			{
				string country = sender.GetValue(e.Row, CountryID.Name) as string;
				string state = sender.GetValue(e.Row, FieldOrdinal) as string;
				try
				{
					string newState = ValidateStateByCountry(state, country);
					if (newState != state)
						sender.SetValue(e.Row, FieldOrdinal, newState);
				}
				catch (LocalizationPreparedException ex)
				{
					throw new PXRowPersistingException(FieldName, state, ex.Format, ex.Args);
				}
			}
		}
	}

	public abstract class CountryStateSelectorAttribute : PXSelectorAttribute
	{		
		protected class Definition : IPrefetchable
		{
			public class Item
			{
				public readonly string ID;
				public readonly string Description;
				public readonly string Regex;

				public Item(string id, string description, string regex)
				{
					this.ID = id;
					this.Description = description;
					this.Regex = regex;
				}

				public class List : PX.Common.Collection.KList<string, Item>
				{					
					private readonly bool KeyDescription;

					public List(bool keyDescription = false)
						: base(new CompareIgnoreCase(), DefaultCapacity, true)
					{
						this.KeyDescription = keyDescription;
					}

					protected override string GetKeyForItem(Item item)
					{
						return KeyDescription ? item.Description : item.ID;
					}
				}
			}

			public class CountryItem : Item
			{
				public readonly string Validation;
				public readonly string StateValidation;
				public readonly List States;
				public readonly List StatesDescription;
				public readonly List StatesRegEx;

				public CountryItem(string id, string description, string regex, string validation, string stateValidation)
					:base(id, description,regex)
				{
					this.Validation = validation;
					this.StateValidation = stateValidation;
					this.States = new List();
					this.StatesDescription = new List(true);
					this.StatesRegEx = new List(true);
				}

				public void AddState(Item state)
				{
					if (StateValidation == Country.stateValidationMethod.No) return;

					this.States.Add(state);
					if(StateValidation != Country.stateValidationMethod.ID)
						this.StatesDescription.Add(state);
					if (StateValidation == Country.stateValidationMethod.NameRegex && state.Regex != null)
						this.StatesRegEx.Add(state);
				}
			}
            public class CountyStateLocale
            {
                public string Country { get; set; }
                public string State { get; set; }
            }
			public readonly Item.List Countries = new Item.List();
			public readonly Item.List CountriesDescription = new Item.List(true);
			public readonly Item.List CountriesRegex = new Item.List();
            private List<Definition.CountyStateLocale> locales = new List<Definition.CountyStateLocale>();

            public void Prefetch()
			{
                this.GetLocales();
                this.GetCountryList();
                this.GetStateList();
			}

            /// <summary>
            /// Getting list of countries with needed conditions.
            /// </summary>
            private void GetCountryList()
            {
                ISqlDialect sqlDialect = PXDatabase.Provider.SqlDialect;
                List<PXDataField> fields = new List<PXDataField>();
                fields.Add(new PXDataField("CountryID"));
                fields.Add(new PXDataField("CountryRegexp"));
                fields.Add(new PXDataField("CountryValidationMethod"));
                fields.Add(new PXDataField("StateValidationMethod"));
                fields.Add(new PXDataField("Description"));
                foreach (Definition.CountyStateLocale item in this.locales)
                {
					var queryExpr = new SubQuery(new Query().Field(new Column("ValueString", "CountryKvExt"))
		                .From(new SimpleTable("CountryKvExt"))
		                .Where(new Column("RecordID", "CountryKvExt").EQ(new Column("NoteID", "Country")
			                .And(new Column("FieldName", "CountryKvExt").EQ(new SQLConst(item.Country)))))
		                .Limit(1));
                    fields.Add(new PXDataField(queryExpr));
                }
                int columnsCount = fields.ToArray().Length;
                foreach (PXDataRecord country in PXDatabase.SelectMulti(
                    typeof(Country), fields.ToArray()))
                {
                    for (int i = 4; i < columnsCount; i++)
                    {
                        string description = country.GetString(i);
                        if (description == null) continue;
                        CountryItem item = new CountryItem(country.GetString(0),
                            description, country.GetString(1),
                            country.GetString(2), country.GetString(3));
                        Countries.Add(item);
                        if (item.Validation != Country.countryValidationMethod.ID)
                            CountriesDescription.Add(item);
                        if (item.Validation == Country.countryValidationMethod.NameRegex
                            && item.Regex != null)
                            CountriesRegex.Add(item);
                    }
                }
            }

            /// <summary>
            /// Getting list of states with needed conditions.
            /// </summary>
            private void GetStateList()
            {
                ISqlDialect sqlDialect = PXDatabase.Provider.SqlDialect;
                List<PXDataField> fields = new List<PXDataField>();
                fields.Add(new PXDataField("StateID"));
                fields.Add(new PXDataField("StateRegexp"));
                fields.Add(new PXDataField("CountryID"));
                fields.Add(new PXDataField("Name"));
                foreach (Definition.CountyStateLocale item in this.locales)
                {
	                var queryExpr = new SubQuery(new Query().Field(new Column("ValueString", "StateKvExt"))
		                .From(new SimpleTable("StateKvExt"))
		                .Where(new Column("RecordID", "StateKvExt").EQ(new Column("NoteID", "State")
			                .And(new Column("FieldName", "StateKvExt").EQ(new SQLConst(item.State)))))
		                .Limit(1));
	                fields.Add(new PXDataField(queryExpr));
				}
                int columnsCount = fields.ToArray().Length;
                foreach (PXDataRecord state in PXDatabase.SelectMulti(
                    typeof(State), fields.ToArray()))
                {
                    for (int i = 3; i < columnsCount; i++)
                    {
                        string description = state.GetString(i);
                        if (description == null) continue;
                        var countryID = state.GetString(2);
                        Item item = new Item(state.GetString(0), description, state.GetString(1));
                        ((CountryItem)Countries[countryID]).AddState(item);
                    }
                }
            }
            
            
            /// <summary>
            /// Getting all posible locations.
            /// </summary>
            private void GetLocales()
            {
                foreach (PXDataRecord record in PXDatabase.SelectMulti<Locale>(
                    new PXDataField<Locale.localeName>()))
                {
                    this.locales.Add(new CountyStateLocale()
                    {
                        Country = "Description" + record.GetString(0).Substring(3, 2),
                        State = "Name" + record.GetString(0).Substring(3, 2)
                    });
                }
            }
        }

        protected static Definition slot
		{
			get { return PXDatabase.GetSlot<Definition>("CountiesListDefinition", typeof(Country), typeof(State)); }
		}

		protected abstract string Type { get; }

		protected CountryStateSelectorAttribute(Type search)
			: base(search)
		{			
		}
		

		/// <summary>
		/// Validates state for specified country and if validation failed, throws PXException.
		/// Returns state id if state was found by the description or the regex, or <paramref name="state"/> value.
		/// </summary>
		/// <param name="state"></param>
		/// <param name="countryID"></param>
		/// <returns>StateID</returns>
		/// <exception cref="LocalizationPreparedException">Thrown if validation failed.</exception>
		protected string ValidateStateByCountry(string state, string countryID)
		{
			if (state.IsNullOrEmpty() || countryID.IsNullOrEmpty())
				return state;

			// unknown country (controlled by country selector)
			Definition.Item item;
			if (!(slot.Countries.TryGetValue(countryID, out item)
				&& item is Definition.CountryItem country))
				return state;

			// no validation
			if (country.StateValidation == Country.stateValidationMethod.No)
				return state;

			if (country.StateValidation.IsIn(
					Country.stateValidationMethod.ID,
					Country.stateValidationMethod.Name,
					Country.stateValidationMethod.NameRegex))
			{
				if (country.States.TryGetValue(state, out item))
					return item.ID;

				if (country.StateValidation.IsIn(
						Country.stateValidationMethod.Name,
						Country.stateValidationMethod.NameRegex))
				{
					if (country.StatesDescription.TryGetValue(state, out item))
					{
						return item.ID;
					}

					if (country.StateValidation == Country.stateValidationMethod.NameRegex)
					{
						var matches = country
							.StatesRegEx
							.Where(i => i.Regex != null)
							.Select(i => (id: i.ID, regex: new Regex(i.Regex, RegexOptions.Multiline | RegexOptions.IgnoreCase)))
							.Where(i => i.regex.IsMatch(state))
							.ToArray();

						if (matches.Length > 1)
						{
							throw new LocalizationPreparedException(Messages.ValidationRegexError,
								Type,
								string.Join(", ", matches.Select(i => i.id)));
						}

						if (matches.Length == 1)
						{
							return matches[0].id;
						}
					}
				}
			}
			throw new LocalizationPreparedException(Messages.StateNotFound, state);
		}

		[Obsolete("Use " + nameof(ValidateStateByCountry) + " instead for validation, or Definition.Item.List.TryGetValue to find.")]
		protected bool Find(Definition.Item.List items, PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (items.Contains((string)e.NewValue))
			{
				e.NewValue = items[(string)e.NewValue].ID;
				return true;
			}
			return false;
		}
		
		[Obsolete("Use " + nameof(ValidateStateByCountry) + " instead for validation, or Definition.Item.List.TryGetValue to find.")]
		protected bool FindRegex(Definition.Item.List items, PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string message = null;
			string possibleValue = e.NewValue as string;
			int matchCount = 0;
			foreach (var item in items)
			{
				if (item.Regex == null) continue;
				try
				{
					var regex = new Regex(item.Regex, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (regex.IsMatch((string) e.NewValue))
					{
						matchCount += 1;
						if (message == null)
							possibleValue = item.ID;
						message += (message == null ? string.Empty : ", ") + item.ID;
					}
				}
				catch
				{					
				}
			}

			if (matchCount > 1)
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.ValidationRegexError, Type, message));
			}

			e.NewValue = possibleValue;

			return matchCount > 0;
		}
	}
	
	#endregion

	#region CR Calculation Attributes

	/// <summary>
	/// Base attribute class for dinamic calculation values of DAC fields.
	/// </summary>
	[Obsolete]
	public abstract class CRCalculationAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region Fields

		private readonly BqlCommand _select;

		#endregion

		[Obsolete]
		protected CRCalculationAttribute(Type valueSelect)
		{
			if (valueSelect == null)
			{
				throw new PXArgumentException("valueSelect", ErrorMessages.ArgumentNullException);
			}
			_select = BqlCommand.CreateInstance(valueSelect);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_select, true);

				object value = CalculateValue(view, e.Row);

				sender.SetValue(e.Row, base._FieldName, value);

				var state = PXFieldState.CreateInstance(e.ReturnState, null, false, null,
														-1, null, null, value, base._FieldName, null, null, null,
														PXErrorLevel.Undefined, false, null, null, PXUIVisibility.Undefined, null, null,
														null);
				state.Value = value;
				e.ReturnState = state;
			}
		}

		protected abstract object CalculateValue(PXView view, object row);
	}

	/// <summary>
	/// Dinamicaly calculates count of rows returned by given Bql command.
	/// </summary>
	[Obsolete]
	public class CRCountCalculationAttribute : CRCalculationAttribute
	{
		[Obsolete]
		public CRCountCalculationAttribute(Type valueSelect)
			: base(valueSelect)
		{
		}

		protected override object CalculateValue(PXView view, object row)
		{
			PXResult result = view.SelectSingle() as PXResult;
			return result == null ? 0 : result.RowCount;
		}
	}

	/// <summary>
	/// Dinamicaly calculates field summ of rows returned by given Bql command.
	/// </summary>
	[Obsolete]
	public class CRSummCalculationAttribute : CRCalculationAttribute
	{
		private readonly Type _summField;

		[Obsolete]
		public CRSummCalculationAttribute(Type valueSelect, Type summField)
			: base(valueSelect)
		{
			_summField = summField;
		}

		protected override object CalculateValue(PXView view, object row)
		{
			return view.Cache.GetValue(view.SelectSingle(), _summField.Name);
		}
	}

	#endregion	

	#region CRNowDefaultAttribute

	/// <summary>
	/// Set DateTime.Now as default value
	/// </summary>
	[Obsolete]
	public class CRNowDefaultAttribute : PXDefaultAttribute
	{
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);
			e.NewValue = PXTimeZoneInfo.Now;
		}
	}

	#endregion

	#region OwnedFilter
	[Serializable]
	[PXHidden]
	public partial class OwnedFilter : IBqlTable, PX.TM.IOwnedFilter
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }

		[PXDBInt]
		[CRCurrentOwnerID]
		public virtual int? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PX.TM.SubordinateOwner]
		public virtual int? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion		
		#region FilterSet
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		[PXDefault(false)]
		[PXDBBool]
		public virtual Boolean? FilterSet
		{
			get
			{
				return 
					this.OwnerID != null ||
					this.WorkGroupID != null || 
					this.MyWorkGroup == true;
			}
		}
		#endregion
	}
	#endregion

	#region SubordinateOwnedFilter
	[Serializable]
	[PXHidden]
	[Obsolete("Will be removed in 7.0 version")]
	public partial class SubordinateOwnedFilter : PX.TM.OwnedFilter
	{
		#region CurrentOwnerID
		public new abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }
		#endregion
		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		[PX.TM.SubordinateOwner(DisplayName = "Assigned to")]
		public override int? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public new abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		#endregion
		#region WorkGroupID
		public new abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }		
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PX.TM.PXSubordinateGroupSelector]
		public override Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region WorkGroup
		public new abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		#endregion
	}
	#endregion

	#region OwnedEscalatedFilter
	[Serializable]
	[PXHidden]
	[Obsolete]
	public partial class OwnedEscalatedFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }

		[PXDBInt]
		[CRCurrentOwnerID]
		public virtual int? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PX.TM.SubordinateOwner(DisplayName = "Assigned To")]
		public virtual int? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion		
		#region MyEscalated
		public abstract class myEscalated : PX.Data.BQL.BqlBool.Field<myEscalated> { }
		protected Boolean? _MyEscalated;
		[PXDefault(true)]
		[PXDBBool]
		[PXUIField(DisplayName = "Display Escalated", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyEscalated
		{
			get
			{
				return _MyEscalated;
			}
			set
			{
				_MyEscalated = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		[PXDefault(false)]
		[PXDBBool]
		public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true ||
					this.MyEscalated == true;
			}
		}
		#endregion
	}
	#endregion

	#region ContactDisplayNameAttribute

	public class ContactDisplayNameAttribute : PXDBStringAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber/*, IPXRowSelectedSubscriber*/
	{
		private readonly Type _lastNameBqlField;
		private readonly Type _firstNameBqlField;
		private readonly Type _midNameBqlField;
		private readonly Type _titleBqlField;
		private readonly bool _reversed;

		private int _lastNameFieldOrdinal;
		private int _firstNameFieldOrdinal;
		private int _midNameFieldOrdinal;
		private int _titleFieldOrdinal;

		public ContactDisplayNameAttribute(Type lastNameBqlField, Type firstNameBqlField,
			Type midNameBqlField, Type titleBqlField, bool reversed)
			: base(255)
		{
			if (lastNameBqlField == null) throw new ArgumentNullException("lastNameBqlField");
			if (firstNameBqlField == null) throw new ArgumentNullException("firstNameBqlField");
			if (midNameBqlField == null) throw new ArgumentNullException("midNameBqlField");
			if (titleBqlField == null) throw new ArgumentNullException("titleBqlField");

			_lastNameBqlField = lastNameBqlField;
			_firstNameBqlField = firstNameBqlField;
			_midNameBqlField = midNameBqlField;
			_titleBqlField = titleBqlField;
			_reversed = reversed;

			IsUnicode = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_lastNameFieldOrdinal = GetFieldOrdinal(sender, _lastNameBqlField);
			_firstNameFieldOrdinal = GetFieldOrdinal(sender, _firstNameBqlField);
			_midNameFieldOrdinal = GetFieldOrdinal(sender, _midNameBqlField);
			_titleFieldOrdinal = GetFieldOrdinal(sender, _titleBqlField);
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Handler(sender, e.Row);
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Handler(sender, e.Row);
		}

		/*public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null) 
				Handler(sender, e.Row);
		}*/

		private string FormatDisplayName(PXCache sender, string aLastName, string aFirstName, string aMidName, string aTitle, bool aReversed)
		{
			if (aLastName == null) aLastName = string.Empty;
			if (aFirstName == null) aFirstName = string.Empty;
			if (aMidName == null) aMidName = string.Empty;
			var locolizedTitle = GetLocolizedValue(sender, aTitle);
			if (locolizedTitle == null) locolizedTitle = string.Empty;

			if (string.IsNullOrEmpty(locolizedTitle))
				return Concat(aLastName, ",", aFirstName, aMidName);

			if (aReversed)
				return Concat(aLastName, aFirstName, aMidName, ",", locolizedTitle);

			return Concat(locolizedTitle, aFirstName, aMidName, aLastName);
		}

		private static string Concat(params string[] args)
		{
			var res = new System.Text.StringBuilder();
			foreach (string item in args)
			{
				var s = item.Trim();
				if (s.Length == 0) continue;

				if (s == ",")
				{
					if (res.Length > 0) res.Append(s);
				}
				else
				{
					if (res.Length > 0) res.Append(" ");
					res.Append(s);
				}
			}
			return res.ToString().TrimEnd(',');
		}

		private string GetLocolizedValue(PXCache cache, string message)
		{
			var value = PXUIFieldAttribute.GetNeutralDisplayName(cache, _titleBqlField.Name) + " -> " + message;
			var temp = PXLocalizer.Localize(value, _BqlTable.FullName);
			if (!string.IsNullOrEmpty(temp) && temp != value)
				return temp;
			return message;
		}

		private static int GetFieldOrdinal(PXCache sender, Type bqlField)
		{
			return sender.GetFieldOrdinal(sender.GetField(bqlField));
		}

		private void Handler(PXCache sender, object data)
		{
			var newValue = FormatDisplayName(sender, 
				sender.GetValue(data, _lastNameFieldOrdinal) as string,
				sender.GetValue(data, _firstNameFieldOrdinal) as string,
				sender.GetValue(data, _midNameFieldOrdinal) as string,
				sender.GetValue(data, _titleFieldOrdinal) as string,
				_reversed);
			sender.SetValue(data, _FieldOrdinal, newValue);
		}
	}

	#endregion

	#region ContactSynchronizeAttribute
	[Serializable]
	public class ContactSynchronizeAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool? isActive = (bool?)sender.GetValue<Contact.isActive>(e.Row);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, FieldName, isActive ?? true);
		}
	}
	#endregion

	#region CRMSourcesAttribute

	/// <summary>
	/// Attribute to specify system values for <see cref="Contact.source"/>, <see cref="CRLead.source"/> and <see cref="CROpportunity.source"/>.
	/// Values for those screen should be adjusted by adjusting System Workflow for those screens.
	/// </summary>
	public class CRMSourcesAttribute : PXStringListAttribute
	{
		/// <exclude/>
		[Obsolete("This source is used only for backward compatibility.")]
		public const string
			_WEB = "W",
			_PHONE_INQ = "H",
			_REFERRAL = "R",
			_PURCHASED_LIST = "L",
			_OTHER = "O";

		/// <exclude/>
		public const string
			OrganicSearch = "S",
			Campaign = "C",
			Referral = "R",
			Other = "O",
			Web = "W",
			PhoneInquiry = "H",
			PurchasedList = "L";

		/// <exclude/>
		public CRMSourcesAttribute() : 
			base(
				Values,
				new[]
				{
					"Organic Search",
					"Campaign",
					"Referral",
					"Other",
					"Web",
					"Phone Inquiry",
					"Purchased List"
				}
			)
		{ }

		internal static readonly string[] Values = new string[] {
			OrganicSearch,
			Campaign,
			Referral,
			Other,
			Web,
			PhoneInquiry,
			PurchasedList
		};

	}

	#endregion

	#region CRDefaultOwnerAttribute
	/// <exclude/>
	public class CRDefaultOwnerAttribute : PXStringListAttribute
	{
		public const string DoNotChange = "N";
		public const string Creator = "C";
		public const string AssignmentMap = "A";
		public const string Source = "S";

		public CRDefaultOwnerAttribute() :
			base(new[] { DoNotChange, Creator, AssignmentMap, Source },
				new[] { "Do Not Change", "Creator", "Assignment map", "From source entity" })
		{
		}

		public class creator : PX.Data.BQL.BqlString.Constant<creator>
		{
			public creator() : base(Creator) { }
		}
		public class assignmentMap : PX.Data.BQL.BqlString.Constant<assignmentMap>
		{
			public assignmentMap() : base(AssignmentMap) { }
		}
		public class source : PX.Data.BQL.BqlString.Constant<source>
		{
			public source() : base(Source) { }
		}
	}

	#endregion

	#region ActivityMajorStatuses

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[Obsolete]
	public class ActivityMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _JUST_CREATED = -1;
		public const int _OPEN = 0;
		public const int _PREPROCESS = 2;
		public const int _PROCESSING = 3;
		public const int _PROCESSED = 4;
		public const int _FAILED = 5;
		public const int _CANCELED = 6;
		public const int _COMPLETED = 7;
		public const int _DELETED = 8;
		public const int _RELEASED = 9;

		[Obsolete]
		public ActivityMajorStatusesAttribute()
			: base(new[] { _JUST_CREATED, _OPEN, _PREPROCESS, _PROCESSING, _PROCESSED, _FAILED, _CANCELED, _COMPLETED, _DELETED, _RELEASED },
			new[] { Messages.JustCreatedFlag, Messages.OpenFlag, Messages.PreprocessFlag, Messages.ProcessingFlag, Messages.ProcessedFlag, Messages.FailedFlag, 
				Messages.CanceledFlag, Messages.CompletedFlag, Messages.DeletedFlag, Messages.ReleasedFlag }) 
		{ }

		public class open : PX.Data.BQL.BqlInt.Constant<open>
		{
			public open() : base(_OPEN) { }
		}

		public class preProcess : PX.Data.BQL.BqlInt.Constant<preProcess>
		{
			public preProcess() : base(_PREPROCESS) { }
		}

		public class processing : PX.Data.BQL.BqlInt.Constant<processing>
		{
			public processing() : base(_PROCESSING) { }
		}

		public class processed : PX.Data.BQL.BqlInt.Constant<processed>
		{
			public processed() : base(_PROCESSED) { }
		}

		public class failed : PX.Data.BQL.BqlInt.Constant<failed>
		{
			public failed() : base(_FAILED) { }
		}

		public class canceled : PX.Data.BQL.BqlInt.Constant<canceled>
		{
			public canceled() : base(_CANCELED) { }
		}

		public class completed : PX.Data.BQL.BqlInt.Constant<completed>
		{
			public completed() : base(_COMPLETED) { }
		}

		public class deleted : PX.Data.BQL.BqlInt.Constant<deleted>
		{
			public deleted() : base(_DELETED) { }
		}

		public class released : PX.Data.BQL.BqlInt.Constant<released>
		{
			public released() : base(_RELEASED) { }
		}
	}

	#endregion

	#region AnnouncementMajorStatusesAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[Obsolete]
	public class AnnouncementMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _DRAFT = 0;
		public const int _PUBLISHED = 1;
		public const int _ARCHIVED = 2;

		[Obsolete]
		public AnnouncementMajorStatusesAttribute()
			: base(new[] { _DRAFT, _PUBLISHED, _ARCHIVED },
			new[] { "Draft", "Published", "Archived" })
		{ }
	}
    #endregion

    #region CRQuoteStatusAttribute

    public class CRQuoteStatusAttribute : PXStringListAttribute
	{
		public const string Draft = "D";
		public const string Approved = "A";
		public const string Sent = "S";
		public const string PendingApproval = "P";
		public const string Rejected = "R";
		public const string Accepted = "T";
		public const string Converted = "O";
		public const string Declined = "L";
		public const string QuoteApproved = "V"; 

		public CRQuoteStatusAttribute()
			: base(
				new[] {
					Draft,
					Approved,
					Sent,
					PendingApproval,
					Rejected,
					Accepted,
					Converted,
					Declined,
					QuoteApproved},
				new[] {
					Messages.Draft,
					Messages.Prepared,
					Messages.Sent,
					Messages.PendingApproval,
					Messages.Rejected,
					Messages.Accepted,
					Messages.Converted,
					Messages.Declined,
					Messages.Approved})
		{}

		protected CRQuoteStatusAttribute(string[] allowedValues, string[] allowedLabels) : base(allowedValues, allowedLabels)
		{ }

		public sealed class draft : PX.Data.BQL.BqlString.Constant<draft>
		{
			public draft() : base(Draft) { }
		}

		public sealed class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved) { }
		}

		public sealed class sent : PX.Data.BQL.BqlString.Constant<sent>
		{
			public sent() : base(Sent) { }
		}

		public sealed class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

		public sealed class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}

		public sealed class accepted : PX.Data.BQL.BqlString.Constant<accepted>
		{
			public accepted() : base(Accepted) { }
		}

		public sealed class converted : PX.Data.BQL.BqlString.Constant<converted>
		{
			public converted() : base(Converted) { }
		}

		public sealed class declined : PX.Data.BQL.BqlString.Constant<declined>
		{
			public declined() : base(Declined) { }
		}

		public sealed class quoteApproved : PX.Data.BQL.BqlString.Constant<quoteApproved>
		{
			public quoteApproved() : base(QuoteApproved) { }
		}
	}
	#endregion

	#region CRQuoteStatusAttribute

	public class CRQuoteTypeAttribute : PXStringListAttribute
    {
        public const string Distribution = "D";
        public const string Project = "P";

        public CRQuoteTypeAttribute()
            : base(new[] { Distribution, Project },
                    new[] { Messages.QuoteTypeDistribution, Messages.QuoteTypeProject })
        { }

        public sealed class distribution : PX.Data.BQL.BqlString.Constant<distribution>
		{
            public distribution() : base(Distribution) { }
        }

        public sealed class project : PX.Data.BQL.BqlString.Constant<project>
		{
            public project() : base(Project) { }
        }
    }

    public class CROpportunityProductLineTypeAttribute : PXStringListAttribute
    {
        public const string Distribution = "D";
        public const string ScopeOfWork = "SOW";

        public CROpportunityProductLineTypeAttribute()
            : base(new[] { Distribution, ScopeOfWork },
                    new[] { Messages.OpportunityLineTypeDistribution, Messages.OpportunityLineTypeScopeOfWork })
        { }

        public sealed class distribution : PX.Data.BQL.BqlString.Constant<distribution>
		{
            public distribution() : base(Distribution) { }
        }

        public sealed class scopeOfWork : PX.Data.BQL.BqlString.Constant<scopeOfWork>
		{
            public scopeOfWork() : base(ScopeOfWork) { }
        }
    }
    #endregion

	#region DuplicateStatusAttribute

	public class DuplicateStatusAttribute : PXStringListAttribute
	{
		public const string NotValidated = "NV";
		public const string PossibleDuplicated = "PD";
		public const string Validated = "VA";
		public const string Duplicated = "DD";

		public DuplicateStatusAttribute() :
			base(
				new[] { NotValidated, Validated, PossibleDuplicated, Duplicated },
				new[] { "Not Validated", "Validated", "Possible Duplicate", "Duplicated" }
			) { }

		public sealed class notValidated : PX.Data.BQL.BqlString.Constant<notValidated>
		{
			public notValidated() : base(NotValidated) {}
		}
		public sealed class possibleDuplicated : PX.Data.BQL.BqlString.Constant<possibleDuplicated>
		{
			public possibleDuplicated() : base(PossibleDuplicated) { }
		}
		public sealed class duplicated : PX.Data.BQL.BqlString.Constant<duplicated>
		{
			public duplicated() : base(Duplicated) { }
		}
		public sealed class validated : PX.Data.BQL.BqlString.Constant<validated>
		{
			public validated() : base(Validated) { }
		}
	}

	#endregion

	#region TransformationRulesAttribute
	public class TransformationRulesAttribute : PXStringListAttribute
	{
		public const string DomainName = "DN";
		public const string None = "NO";
		public const string SplitWords = "SW";
		public const string SplitEmailAddresses = "SE";

		public TransformationRulesAttribute(): base(new[] {DomainName, None, SplitWords, SplitEmailAddresses },
													new[] {Messages.DomainName, Messages.None, Messages.SplitWords, Messages.SplitEmailAddresses})
		{}

		public sealed class domainName : PX.Data.BQL.BqlString.Constant<domainName>
		{
			public domainName() : base(DomainName) { }
		}

		public sealed class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(None) { }
		}

		public sealed class splitWords : PX.Data.BQL.BqlString.Constant<splitWords>
		{
			public splitWords() : base(SplitWords) { }
		}

		public sealed class splitEmailAddresses : PX.Data.BQL.BqlString.Constant<splitEmailAddresses>
		{
			public splitEmailAddresses() : base(SplitEmailAddresses) { }
		}
	}

	#endregion

	#region CreateOnEntryAttribute
	public class CreateOnEntryAttribute : PXStringListAttribute
	{
		public const string Block = "B";
		public const string Allow = "A";
		public const string Warn = "W";

		public CreateOnEntryAttribute(bool stricted = true) : base(stricted ? new[] { Block, Allow, Warn } : new[] { Allow, Warn },
			stricted ? new[] { Messages.Block, Messages.Allow, Messages.Warn } : new[] { Messages.Allow, Messages.Warn })
		{ }

		public sealed class block : PX.Data.BQL.BqlString.Constant<block>
		{
			public block() : base(Block) { }
		}

		public sealed class allow : PX.Data.BQL.BqlString.Constant<allow>
		{
			public allow() : base(Allow) { }
		}

		public sealed class warn : PX.Data.BQL.BqlString.Constant<warn>
		{
			public warn() : base(Warn) { }
		}
	}

	#endregion

	#region LanguageDBStringAttribute

	[Obsolete]
	public sealed class LanguageDBStringAttribute : PXDBStringAttribute
	{
		private const string _FIELD_POSTFIX = "_DisplayName";

		private string _displayNameFieldName;

		[Obsolete]
		public LanguageDBStringAttribute()
		{
		}

		[Obsolete]
		public LanguageDBStringAttribute(int length) : base(length)
		{
		}

		public string DisplayName { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_displayNameFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_displayNameFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _displayNameFieldName, _FieldName_DisplayName_FieldSelecting);
		}

		private void _FieldName_DisplayName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var langVal = sender.GetValue(args.Row, _FieldOrdinal) as string;
			var displayNameVal = string.Empty;
			if (!string.IsNullOrEmpty(langVal))
				try
				{
					displayNameVal = CultureInfo.GetCultureInfo(langVal).DisplayName;
				}
				catch (ArgumentException) { } //NOTE: incorrect language value
			args.ReturnState = PXFieldState.CreateInstance(displayNameVal, typeof(string), null, null, null, null, null,
								   null, _displayNameFieldName, null, DisplayName, null, PXErrorLevel.Undefined, false,
								   true, null, PXUIVisibility.Visible, null, null, null);
		}
	}

	#endregion

	#region PXNotificationContactSelectorAttribute	
	public class PXNotificationContactSelectorAttribute : PXSelectorAttribute
	{
		public PXNotificationContactSelectorAttribute()
			: this(null)
		{
		}

		public PXNotificationContactSelectorAttribute(Type contactType)
					:base(typeof(Search2<Contact.contactID,
				LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<Contact.bAccountID>>,			
				LeftJoin<EPEmployee, 
					  On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
					  And<EPEmployee.defContactID, Equal<Contact.contactID>>>>>,
				Where2<
						Where<Current<NotificationRecipient.contactType>, Equal<NotificationContactType.employee>,
			  And<EPEmployee.acctCD, IsNotNull>>,
					 Or<Where<Current<NotificationRecipient.contactType>, Equal<NotificationContactType.contact>,
								And<BAccountR.noteID, Equal<Current<NotificationRecipient.refNoteID>>,
								And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>>>>))
		{
			SubstituteKey = typeof(Contact.displayName);
			this.contactType = contactType;
		}

		public PXNotificationContactSelectorAttribute(Type contactType, Type search)
			:base(search)
		{
			SubstituteKey = typeof(Contact.displayName);
			this.contactType = contactType;
		}

		private Type contactType;
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if(contactType != null)
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), OnRowSelected);
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || contactType == null) return;
			
			PXCache sourceCache = sender.Graph.Caches[BqlCommand.GetItemType(contactType)];
			object value = 
				(sourceCache == sender) ?
				sender.GetValue(e.Row, contactType.Name) :
				sourceCache.GetValue(sourceCache.Current, contactType.Name);

			string type = value != null ? value.ToString() : null;				

			bool enabled = 
				type == NotificationContactType.Contact ||
				 type == NotificationContactType.Employee;
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, enabled);
			PXDefaultAttribute.SetPersistingCheck(sender, _FieldName, e.Row, enabled ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}
	}
	#endregion

	public class String<ListField> : BqlFormulaEvaluator<ListField>, IBqlOperand
		where ListField : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			return PXFieldState.GetStringValue(cache.GetStateExt<ListField>(item) as PXFieldState, null, null);
		}
	}

	#region PXLocationID
	[Obsolete]
	public class PXLocationIDAttribute : PXAggregateAttribute
	{
		private string _DimensionName = "LOCATION";

		[Obsolete]
		public PXLocationIDAttribute(Type type, params Type[] fieldList):
			base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXNavigateSelectorAttribute selectorattr = new PXNavigateSelectorAttribute(type, fieldList);
			_Attributes.Add(selectorattr);
		}

		[Obsolete]
		public PXLocationIDAttribute(Type type):
			base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);
			
			PXNavigateSelectorAttribute selectorattr = new PXNavigateSelectorAttribute(type);
			_Attributes.Add(selectorattr);
		}
	}
	#endregion

	#region DefLocationID
	public class DefLocationIDAttribute : PXAggregateAttribute
	{
		private string _DimensionName = "LOCATION";

		public Type DescriptionField
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().DescriptionField;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().DescriptionField = value;
			}
		}

		public Type SubstituteKey
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().SubstituteKey;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().SubstituteKey = value;
			}
		}

		public DefLocationIDAttribute(Type type, params Type[] fieldList) 
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXSelectorAttribute selattr = new PXSelectorAttribute(type, fieldList);
			selattr.DirtyRead = true;
			selattr.CacheGlobal = false;
			_Attributes.Add(selattr);
		}

		public DefLocationIDAttribute(Type type) 
			: this(type, typeof(Location.locationCD), typeof(Location.descr))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
		}

		protected virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PXFieldUpdating fu = this.GetAttribute<PXDimensionAttribute>().FieldUpdating;
			fu(sender, e);
			e.Cancel = false;

			fu = this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
			fu(sender, e);
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) != typeof(IPXFieldUpdatingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
			
			if (SubstituteKey == null || String.Compare(SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
				if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
				else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
			}
		}
	}
	#endregion

	#region PXNavigateSelector

	public class PXNavigateSelectorAttribute : PXSelectorAttribute
	{
		public PXNavigateSelectorAttribute(Type type) : base(type)
		{
		}

		public PXNavigateSelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			
		}

		protected override bool IsReadDeletedSupported
		{
			get
			{
				return false;
			}
		}

		protected override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)_SelectorFields).SyncRoot)
			{
				List<KeyValuePair<string, Type>> list;
				if (_SelectorFields.TryGetValue(bqlTable, out list) && list.Count > 0 && list[list.Count - 1].Key == base.FieldName)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
		}
	}

	#endregion

	#region CaseRelationTypeAttribute

	public class CaseRelationTypeAttribute : PXStringListAttribute
	{
		public const string _BLOCKS_VALUE = "P";
		public const string _DEPENDS_ON_VALUE = "C";
		public const string _RELATED_VALUE = "R";
		public const string _DUBLICATE_OF_VALUE = "D";

		public CaseRelationTypeAttribute()
			: base(new[] { _BLOCKS_VALUE, _DEPENDS_ON_VALUE, _RELATED_VALUE, _DUBLICATE_OF_VALUE }, new[] { "Blocks", "Depends On", "Related", "Duplicate Of" })
		{
			
		}
	}

	#endregion

	#region CRPreviewAttribute

	public class CRPreviewAttribute : PXPreviewAttribute
	{
		private readonly PXSelectDelegate _select;
		private GeneratePreview _handler;
		private object _current;

		public delegate object GeneratePreview(object row);

		public CRPreviewAttribute(Type primaryViewType, Type previewType)
			: base(primaryViewType, previewType)
		{
			_select = new PXSelectDelegate(delegate() { PerformRefresh(); return new[] { _current }; });
		}

		protected override PXSelectDelegate SelectHandler
		{
			get
			{
				return _select;
			}
		}

		protected override IEnumerable GetPreview()
		{
			if (_handler != null)
			{
				var row = Graph.Caches[CacheType].Current;
				yield return _handler(row);
			}
		}

		protected override void PerformRefresh()
		{
			foreach(object row in GetPreview())
				_current = row;
		}

		public virtual void Attach(PXGraph graph, string viewName, GeneratePreview getPreviewHandler)
		{
			if (_handler != null) throw new InvalidOperationException(Messages.AttributesAlreadyAttached);

			_handler = getPreviewHandler ?? (o => o);
			ViewCreated(graph, viewName);
		}
	}

	#endregion

	#region LeadRawAttribute

	[Obsolete]
	public class LeadRawAttribute : PXEntityAttribute
	{
		public const string DimensionName = "LEAD";

		[Obsolete]
		public LeadRawAttribute()
		{
			Type searchType = typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>>);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(EPEmployee.acctCD))); //TODO: need implementation (substituteKey)
			attr.DescriptionField = typeof(Contact.displayName);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}
	}

	#endregion

	#region CRContactsViewAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CREmailContactsViewAttribute : Attribute
	{
		private readonly BqlCommand _select;

		public CREmailContactsViewAttribute(Type select)
		{
			if (select == null) throw new ArgumentNullException("select");
			if (!typeof(BqlCommand).IsAssignableFrom(select)) 
				throw new ArgumentException(string.Format("type '{0}' must inherit PX.Data.BqlCommand", select.Name), "select");
			_select = BqlCommand.CreateInstance(select);
		}

		public BqlCommand Select
		{
			get { return _select; }
		}

		[Obsolete]
		public static PXView GetView(PXGraph graph, Type objType)
		{
			if (graph == null || objType == null) return null;

			var contactsViewAtt = GetCustomAttribute(objType, typeof(CREmailContactsViewAttribute), true) as CREmailContactsViewAttribute;
			if (contactsViewAtt != null && contactsViewAtt.Select != null)
				return new PXView(graph, true, contactsViewAtt.Select);
			return null;
		}
	}

	#endregion

	#region CREmailSelectorAttribute

	public sealed class CREmailSelectorAttribute : ContactSelectorAttribute
	{		

		public CREmailSelectorAttribute() : this(false) { }
		
		[Obsolete("Will be removed in 7.0 version")]
		public CREmailSelectorAttribute(bool all)
			: base(typeof(Contact.eMail), 
                  false, 
                  new []{
                  typeof(ContactTypesAttribute.person), 
                  typeof(ContactTypesAttribute.lead), 
                  typeof(ContactTypesAttribute.employee)
                  },
                  new[] { typeof(Contact.displayName), typeof(Contact.eMail) }
                  )
		{
		}

		protected override bool IsReadDeletedSupported => false;

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var value = e.NewValue == null
				? String.Empty
				: e.NewValue.ToString();

			e.NewValue = PXDBEmailAttribute.ToRFC(value);
		}

		public override void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}		
	}

	#endregion

	#region  CRQuotePrimaryGraphAttribute

	public sealed class CRQuotePrimaryGraphAttribute : CRCacheIndependentPrimaryGraphListAttribute
	{
		public CRQuotePrimaryGraphAttribute() : base(
			new[]
			{
				typeof (PM.PMQuoteMaint),
				typeof (PM.PMQuoteMaint),
				typeof (QuoteMaint),
				typeof (QuoteMaint)
			},
			new[]
			{
				typeof (Select<PM.PMQuote,
					Where<PM.PMQuote.quoteID, Equal<Current<CRQuote.quoteID>>>>),

				typeof (Select<PM.PMQuote,
					Where<PM.PMQuote.quoteID, Equal<Current<CRQuote.noteID>>>>),

				typeof (Select<CRQuote,
					Where<CRQuote.quoteID, Equal<Current<CRQuote.quoteID>>,
						And<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>>>),

				typeof (Select<CRQuote,
					Where<CRQuote.quoteID, Equal<Current<CRQuote.noteID>>>>),
			})
		{ }
		protected override void OnAccessDenied(Type graphType)
		{
			throw new AccessViolationException(Messages.FormNoAccessRightsMessage(graphType));
		}
	}

	#endregion

	#region  CRTimeActivityPrimaryGraphAttribute

	public sealed class CRTimeActivityPrimaryGraphAttribute : CRCacheIndependentPrimaryGraphListAttribute
	{
		public CRTimeActivityPrimaryGraphAttribute() : base(
			new[]
			{
				typeof (CRActivityMaint),
				typeof (CREmailActivityMaint),
				typeof (CRTaskMaint),
				typeof (EPEventMaint)
			},
			new[]
			{
				typeof (Select<CRActivity,
					Where<CRActivity.noteID, Equal<Current<PMTimeActivity.refNoteID>>,
						And<CRActivity.classID, Equal<CRActivityClass.activity>>>>),

				typeof (Select<CRSMEmail,
					Where<CRSMEmail.noteID, Equal<Current<PMTimeActivity.refNoteID>>>>),

				typeof (Select<CRPMTimeActivity,
					Where<CRPMTimeActivity.noteID, Equal<Current<PMTimeActivity.refNoteID>>,
						And<CRPMTimeActivity.classID, Equal<CRActivityClass.task>>>>),

				typeof (Select<CRActivity,
					Where<CRActivity.noteID, Equal<Current<PMTimeActivity.refNoteID>>,
						And<CRActivity.classID, Equal<CRActivityClass.events>>>>)
			})
		{ }
		
		protected override void OnAccessDenied(Type graphType)
		{
			throw new AccessViolationException(Messages.FormNoAccessRightsMessage(graphType));
		}
	}

	#endregion


	#region  CRSMEmailPrimaryGraphAttribute

	public sealed class CRSMEmailPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		private static readonly Type[] _possibleGraphTypes = {typeof(CREmailActivityMaint), typeof(CRSMEmailMaint)};

		public override  Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			SMEmail record = (SMEmail) row;

			if (record.NoteID != record.RefNoteID)
			{
				PXGraph graph = new PXGraph();

				CRSMEmail _row = PXSelect<CRSMEmail, Where<CRSMEmail.noteID, Equal<Required<CRSMEmail.noteID>>>>.Select(graph, record.RefNoteID);
			    if (_row != null)
			    {
			        row = _row;
			        return typeof(CREmailActivityMaint);
			    }
			}
			return typeof(CRSMEmailMaint); 

		}

		 void OnAccessDenied(Type graphType)
		{
			throw new AccessViolationException(Messages.FormNoAccessRightsMessage(graphType));
		}

		 public override IEnumerable<Type> GetAllGraphTypes() => _possibleGraphTypes;
	}

	#endregion


	#region CRContactMethodsAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class CRContactMethodsAttribute : PXStringListAttribute
	{
		public const string Any = "A";
		public const string Email = "E";
		public const string Mail = "M";
		public const string Fax = "F";
		public const string Phone = "P";

		public CRContactMethodsAttribute()
			: base(new [] { Any, Email, Mail, Fax, Phone },
					new [] { Messages.MethodAny, Messages.MethodEmail, Messages.MethodMail, Messages.MethodFax, Messages.MethodPhone })
		{
			
		}

		public class any : PX.Data.BQL.BqlString.Constant<any>
		{
			public any() : base(Any) { }
		}

		public class email : PX.Data.BQL.BqlString.Constant<email>
		{
			public email() : base(Email) { }
		}

		public class mail : PX.Data.BQL.BqlString.Constant<mail>
		{
			public mail() : base(Mail) { }
		}

		public class fax : PX.Data.BQL.BqlString.Constant<fax>
		{
			public fax() : base(Fax) { }
		}

		public class phone : PX.Data.BQL.BqlString.Constant<phone>
		{
			public phone() : base(Phone) { }
		}
	}

	#endregion

	#region MaritalStatusesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class MaritalStatusesAttribute : PXStringListAttribute
	{
		public const string Single = "S";
		public const string Married = "M";
		public const string Divorced = "D";
		public const string Widowed = "W";

		public MaritalStatusesAttribute()
			: base(new [] { Single, Married, Divorced, Widowed }, 
					new [] { Messages.Single, Messages.Married, Messages.Divorced, Messages.Widowed })
		{
			
		}

		public class single : PX.Data.BQL.BqlString.Constant<single>
		{
			public single() : base(Single) { }
		}

		public class married : PX.Data.BQL.BqlString.Constant<married>
		{
			public married() : base(Married) { }
		}

		public class divorced : PX.Data.BQL.BqlString.Constant<divorced>
		{
			public divorced() : base(Divorced) { }
		}

		public class widowed : PX.Data.BQL.BqlString.Constant<widowed>
		{
			public widowed() : base(Widowed) { }
		}
	}

	#endregion

	#region GendersAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class GendersAttribute : PXStringListAttribute
	{
		public const string Male = "M";
		public const string Female = "F";

		private readonly Type _titleField;
		
		public GendersAttribute(Type titleField)
			: this()
		{
			if (titleField == null) throw new ArgumentNullException("titleField");
			_titleField = titleField;
		}

		public GendersAttribute()
			: base(new [] { Male, Female }, new [] { Messages.Male, Messages.Female })
		{
			
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_titleField != null)
			{
				sender.Graph.RowInserted.AddHandler(_BqlTable, RowInsertedHandler);
				sender.Graph.RowUpdated.AddHandler(_BqlTable, RowUpdatedHandler);
			}
		}

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var gender = sender.GetValue(e.Row, _FieldName);
			if (gender == null)
			{
				var title = sender.GetValue(e.Row, _titleField.Name) as string;
				if (title != null)
				{
					object newVal = null;
					switch (title)
					{
						case TitlesAttribute.Mr:
							newVal = Male;
							break;
						case TitlesAttribute.Ms:
						case TitlesAttribute.Miss:
						case TitlesAttribute.Mrs:
							newVal = Female;
							break;
					}
					sender.SetValue(e.Row, _FieldName, newVal);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var gender = sender.GetValue(e.Row, _FieldName);
			var oldGender = sender.GetValue(e.OldRow, _FieldName);
			var title = sender.GetValue(e.Row, _titleField.Name) as string;
			var oldlTitle = sender.GetValue(e.OldRow, _titleField.Name) as string;
			if (gender == oldGender && title != null && title != oldlTitle)
			{
				object newVal = null;
				switch (title)
				{
					case TitlesAttribute.Mr:
						newVal = Male;
						break;
					case TitlesAttribute.Ms:
					case TitlesAttribute.Miss:
					case TitlesAttribute.Mrs:
						newVal = Female;
						break;
				}
				if (newVal != null) sender.SetValue(e.Row, _FieldName, newVal);
			}
		}

		public class male : PX.Data.BQL.BqlString.Constant<male>
		{
			public male() : base(Male) { }
		}

		public class female : PX.Data.BQL.BqlString.Constant<female>
		{
			public female() : base(Female) { }
		}
	}

	#endregion

	#region CRPrimaryGraphRestrictedAttribute
	public class CRPrimaryGraphRestrictedAttribute : CRCacheIndependentPrimaryGraphListAttribute
	{
		public CRPrimaryGraphRestrictedAttribute() { }

		public CRPrimaryGraphRestrictedAttribute(Type[] graphTypes, Type[] conditions)
			: base(graphTypes, conditions) { }

		protected override void OnNoItemFound(Type graphType)
		{
			throw new AccessViolationException(Messages.NoItemsFound);
		}
	}
	#endregion

	#region PXPrimaryGraphAttribute
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CRCacheIndependentPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		private readonly CRCacheIndependentPrimaryGraphListAttribute _att;

		public CRCacheIndependentPrimaryGraphAttribute(Type graphType, Type condition)
		{
			_att = new CRCacheIndependentPrimaryGraphListAttribute { { graphType, condition } };
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			return _att.GetGraphType(cache, ref row, checkRights, preferedType);
		}

		public override IEnumerable<Type> GetAllGraphTypes() => _att.GetAllGraphTypes();
	}
	#endregion

	#region CRCacheIndependantPrimaryGraphListAttribute

	public class CRCacheIndependentPrimaryGraphListAttribute : PXPrimaryGraphBaseAttribute, IEnumerable
	{
		#region PrimaryGraph

		private sealed class PrimaryGraph
		{
			private readonly Type _graphType;
			private readonly Type _condition;

			public PrimaryGraph(Type graphType, Type condition)
			{
				if (graphType == null) throw new ArgumentNullException("graphType");
				if (!typeof(PXGraph).IsAssignableFrom(graphType))
					throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.NeedGraphType, graphType.GetLongName()));
				if (condition == null) throw new ArgumentNullException("condition");
				if (!typeof(BqlCommand).IsAssignableFrom(condition) && !typeof(IBqlWhere).IsAssignableFrom(condition))
					throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.NeedBqlCommandType, condition.GetLongName()));

				_graphType = graphType;
				_condition = condition;
			}

			public Type GraphType
			{
				get { return _graphType; }
			}

			public Type Condition
			{
				get { return _condition; }
			}
		}

		#endregion

		private readonly IList<PrimaryGraph> _items;

		public CRCacheIndependentPrimaryGraphListAttribute()
		{
			_items = new List<PrimaryGraph>();
		}

		public CRCacheIndependentPrimaryGraphListAttribute(Type[] graphTypes, Type[] conditions) 
			: this()
		{
			if (graphTypes == null) throw new ArgumentNullException("graphTypes");
			if (conditions == null) throw new ArgumentNullException("conditions");
			if (graphTypes.Length != conditions.Length)
				throw new ArgumentException(Messages.GraphTypesAndConditionsLengthException);

			for (int index = 0; index < conditions.Length; index++)
			{
				var graphType = graphTypes[index];
				var condition = conditions[index];
				Add(graphType, condition);
			}
		}

		public virtual void Add(Type graphType, Type condition)
		{
			_items.Add(new PrimaryGraph(graphType, condition));
		}

		private Dictionary<Type, HashSet<Type>> getPossibleDACs(Type itemType)
		{
			Dictionary<Type, HashSet<Type>> dict = new Dictionary<Type, HashSet<Type>>();
			foreach (PrimaryGraph pg in _items)
			{
				Type possibleDAC = null;
				if (typeof (IBqlWhere).IsAssignableFrom(pg.Condition))
				{
					possibleDAC = itemType;
				}
				else
				{
					BqlCommand command = BqlCommand.CreateInstance(pg.Condition);
					possibleDAC = command.GetTables()[0];
				}
				if (possibleDAC != null)
				{
					if (!dict.ContainsKey(pg.GraphType))
					{
						dict[pg.GraphType] = new HashSet<Type>();
					}
					dict[pg.GraphType].Add(possibleDAC);
				}
			}
			return dict;
		}

		private bool typeIsPossible(Type testType, HashSet<Type> possibleTypes)
		{
			foreach (Type posType in possibleTypes)
			{
				if (posType.IsAssignableFrom(testType)) return true;
			}
			return false;
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			PXGraph graph = null;
			Dictionary<Type, HashSet<Type>> possibleDACs = null;
			if (preferedType != null)
			{
				possibleDACs = getPossibleDACs(cache.GetItemType());
			}
			Type lastGraphType = null;
			bool anyFound = false;
			foreach (PrimaryGraph pg in _items)
			{
				var itemType = cache.GetItemType();

				if (graph == null)
				{
					graph = PXGraph.CreateInstance<PXGraph>();

					// hack to make CurrentMatch<> working
					graph.Caches[typeof(AccessInfo)].Current = graph.Accessinfo;

				if (row != null)
				{
					var rowStatus = cache.GetStatus(row);
						graph.Caches[itemType].SetStatus(row, rowStatus);
					}
				}

				if (typeof(IBqlWhere).IsAssignableFrom(pg.Condition))
				{
					IBqlWhere where = (IBqlWhere)Activator.CreateInstance(pg.Condition);
					bool? result = null;
					object value = null;
					BqlFormula.Verify(cache, row, where, ref result, ref value);

					anyFound = anyFound || (result ?? false);

					if (result == true && (preferedType == null || possibleDACs.ContainsKey(preferedType) && typeIsPossible(itemType, possibleDACs[preferedType])))
					{
						lastGraphType = pg.GraphType;
						if (!checkRights || PXAccess.VerifyRights(pg.GraphType))
							return pg.GraphType;
					}
				}
				else if (row != null)
				{
					var command = BqlCommand.CreateInstance(pg.Condition);
					var view = new PXView(graph, false, command);
					var item = view.SelectSingleBound(new[] { row });

					anyFound = anyFound || (item != null);

					if (item != null && (preferedType == null || possibleDACs.ContainsKey(preferedType) && typeIsPossible(view.GetItemType(), possibleDACs[preferedType])))
					{
						lastGraphType = pg.GraphType;

						if (!checkRights || PXAccess.VerifyRights(pg.GraphType))
						{
							row = item;

							if (row is PXResult)
							{
								row = ((PXResult)row)[0];
							}

							return pg.GraphType;
						}
					}
				}
			}
			if (!anyFound && row != null)
			{
				OnNoItemFound(lastGraphType);
			}
			if (lastGraphType != null)
			{
				OnAccessDenied(lastGraphType);
			}
			return null;
		}

		protected virtual void OnNoItemFound(Type graphType)
		{
		}

		protected virtual void OnAccessDenied(Type graphType)
		{
		}

		public virtual IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		public override IEnumerable<Type> GetAllGraphTypes()
		{
			return _items.Select(i => i.GraphType);
		}
	}

	#endregion

	#region CRMassMailStatusesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRMassMailStatusesAttribute : PXStringListAttribute
	{
		public const string Hold = "H";
		public const string Prepared = "P";
		public const string Send = "S";

		public CRMassMailStatusesAttribute()
			: base(
				new[] { Hold, Prepared, Send },
				new[] { Messages.Hold_MassMailStatus, Messages.Prepared_MassMailStatus, Messages.Sent_MassMailStatus })
		{
			
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { }
		}

		public class prepared : PX.Data.BQL.BqlString.Constant<prepared>
		{
			public prepared() : base(Prepared) { }
		}
		
		public class send : PX.Data.BQL.BqlString.Constant<send>
		{
			public send() : base(Send) { }
		}

	}

	#endregion

	#region CRMassMailSourcesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRMassMailSourcesAttribute : PXIntListAttribute
	{
		public const int MailList = 0;
		public const int Campaign = 1;
		public const int Lead = 2;

		public CRMassMailSourcesAttribute()
			: base(new[] { MailList, Campaign, Lead },
				new[] { Messages.MailList_MassMailSource, Messages.Campaign_MassMailSource, Messages.LeadContacts_MassMailSource })
		{
			
		}

		public class hold : PX.Data.BQL.BqlInt.Constant<hold>
		{
			public hold() : base(Campaign) { }
		}

		public class pending : PX.Data.BQL.BqlInt.Constant<pending>
		{
			public pending() : base(Lead) { }
		}

		public class rejected : PX.Data.BQL.BqlInt.Constant<rejected>
		{
			public rejected() : base(MailList) { }
		}
	}
	#endregion

	#region ContactTypesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class ContactTypesAttribute : PXStringListAttribute
	{
		public const string Person = "PN";
		public const string SalesPerson = "SP";
		public const string BAccountProperty = "AP";
		public const string Employee = "EP";
		public const string Lead = "LD";
		public const string Broker = "BR"; 

		public ContactTypesAttribute()
			: base(
			new string[] { Person, SalesPerson, BAccountProperty, Employee, Lead, Broker },
			new string[] { Messages.Person, Messages.SalesPerson, Messages.BAccountProperty, Messages.Employee, Messages.Lead, Messages.Broker })
		{

		}

		public class person : PX.Data.BQL.BqlString.Constant<person>
		{
			public person() : base(Person) { ; }
		}
		public class bAccountProperty : PX.Data.BQL.BqlString.Constant<bAccountProperty>
		{
			public bAccountProperty() : base(BAccountProperty) { ; }
		}
		public class salesPerson : PX.Data.BQL.BqlString.Constant<salesPerson>
		{
			public salesPerson() : base(SalesPerson) { ; }
		}
		public class employee : PX.Data.BQL.BqlString.Constant<employee>
		{
			public employee() : base(Employee) { ; }
		}
		public class lead : PX.Data.BQL.BqlString.Constant<lead>
		{
			public lead() : base(Lead) { ; }
		}
		public class broker : PX.Data.BQL.BqlString.Constant<broker>
		{
			public broker() : base(Broker) { ; }
		}


		public class Priority
		{
			public class bAccountPriority : PX.Data.BQL.BqlInt.Constant<bAccountPriority>
			{
				public bAccountPriority() : base(-10) { }
			}

			public class salesPersonPriority : PX.Data.BQL.BqlInt.Constant<salesPersonPriority>
			{
				public salesPersonPriority() : base(-5) { }
			}

			public class employeePriority : PX.Data.BQL.BqlInt.Constant<employeePriority>
			{
				public employeePriority() : base(-1) { }
			}

			public class personPriority : PX.Data.BQL.BqlInt.Constant<personPriority>
			{
				public personPriority() : base(0) { }
			}

			public class leadPriority : PX.Data.BQL.BqlInt.Constant<leadPriority>
			{
				public leadPriority() : base(10) { }
			}
		}
	}

	#endregion

	#region PhoneTypesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PhoneTypesAttribute : PXStringListAttribute
	{
		public const string Business1 = "B1";
		public const string Business2 = "B2";
		public const string Business3 = "B3";
		public const string BusinessAssistant1 = "BA1";
		public const string BusinessFax = "BF";
		public const string Home = "H1";
		public const string HomeFax = "HF";
		public const string Cell = "C";

		public PhoneTypesAttribute()
			: base(
				new string[] { Business1, Business2, Business3, Cell, BusinessAssistant1, BusinessFax, Home, HomeFax },
				new string[] { Messages.Business1, Messages.Business2, Messages.Business3, Messages.Cell, Messages.BusinessAssistant1, Messages.BusinessFax, Messages.Home, Messages.HomeFax })
		{
		}

		public class business1 : PX.Data.BQL.BqlString.Constant<business1>
		{
			public business1() : base(Business1) { ;}
		}
		public class business2 : PX.Data.BQL.BqlString.Constant<business2>
		{
			public business2() : base(Business2) { ;}
		}
		public class business3 : PX.Data.BQL.BqlString.Constant<business3>
		{
			public business3() : base(Business3) { ;}
		}
		public class businessAssistant1 : PX.Data.BQL.BqlString.Constant<businessAssistant1>
		{
			public businessAssistant1() : base(BusinessAssistant1) { ;}
		}
		public class businessFax : PX.Data.BQL.BqlString.Constant<businessFax>
		{
			public businessFax() : base(BusinessFax) { ;}
		}
		public class home : PX.Data.BQL.BqlString.Constant<home>
		{
			public home() : base(Home) { ;}
		}
		public class homeFax : PX.Data.BQL.BqlString.Constant<homeFax>
		{
			public homeFax() : base(HomeFax) { ;}
		}
		public class cell : PX.Data.BQL.BqlString.Constant<cell>
		{
			public cell() : base(Cell) { ;}
		}
	}

	#endregion

	#region TitlesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class TitlesAttribute : PXStringListAttribute
	{
		public TitlesAttribute()
			: base(
				new string[] { Doctor, Miss, Mr, Mrs, Ms, Prof },
				new string[] { Messages.Doctor, Messages.Miss, Messages.Mr, Messages.Mrs, Messages.Ms, Messages.Prof })
		{
		}

		public const string Doctor = "Dr.";
		public const string Miss = "Miss";
		public const string Mr = "Mr.";
		public const string Mrs = "Mrs.";
		public const string Ms = "Ms.";
		public const string Prof = "Prof.";
	}

	#endregion

	#region PhoneValidationAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class PhoneValidationAttribute : PX.SM.PXPhoneValidationAttribute
	{
		private class Definition : IPrefetchable
		{
			public string PhoneMask;
			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord pref =
						PXDatabase.SelectSingle<GL.Company>(
						new PXDataField(typeof(GL.Company.phoneMask).Name)))
				{
					if (pref != null)
					{
						PhoneMask = pref.GetString(0);
					}
				}
			}
		}
		public PhoneValidationAttribute() : base("") { }
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			Definition def = PXDatabase.GetSlot<Definition>("CompanyPhoneMask", typeof(GL.Company));
			if (def != null)
			{
				_mask = def.PhoneMask ?? "";
			}
		}
	}
	#endregion

	#region CRTimeSpanCalcedAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRTimeSpanCalcedAttribute : PXDBCalcedAttribute
	{
		// private static readonly long _1900YEAR_TICKS = new DateTime(1900, 1, 1).Ticks;

		public CRTimeSpanCalcedAttribute(Type operand) 
			: base(operand, typeof(int))
		{
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			// excepting DATEDIFF(mi, d1, d2)
			object value = e.Record.GetValue(e.Position);
			if (value != null)
			{
				int v = Convert.ToInt32(value);
				value = v > 0 ? value : 0;
				// new TimeSpan(ticks > _1900YEAR_TICKS ? ticks - _1900YEAR_TICKS : 0).TotalMinutes;
			}
			sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(value));
			e.Position++;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			
		}
	}

	#endregion

	#region CRContactCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class CRContactCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public CRContactCacheNameAttribute(string name) 
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var contact = row as Contact;
			if (contact == null) return base.GetName();
			
			var result = Messages.ContactType;
			switch (contact.ContactType)
			{
				case ContactTypesAttribute.Lead:
					result = Messages.Lead;
					break;
				/*case ContactTypesAttribute.Person:
					result = Messages.Person;
					break;
				case ContactTypesAttribute.SalesPerson:
					result = Messages.SalesPerson;
					break;
				case ContactTypesAttribute.BAccountProperty:
					result = Messages.BAccountProperty;
					break;*/
				case ContactTypesAttribute.Employee:
					result = Messages.Employee;
					break;
			}
			return result;
		}
	}

	#endregion

	#region CRCaseSeverityAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CRCaseSeverityAttribute : PXStringListAttribute
	{
		public const string _LOW = "L";
		public const string _MEDIUM = "M";
		public const string _HIGH = "H";
		public const string _URGENT = "U";

		public CRCaseSeverityAttribute()
			: base(new[] { _LOW, _MEDIUM, _HIGH, _URGENT },
			new[] { "Low", "Medium", "High", "Urgent" })
		{

		}

		public sealed class low : PX.Data.BQL.BqlString.Constant<low>
		{
			public low() : base(_LOW) { }
		}

		public sealed class medium : PX.Data.BQL.BqlString.Constant<medium>
		{
			public medium() : base(_MEDIUM) { }
		}

		public sealed class high : PX.Data.BQL.BqlString.Constant<high>
		{
			public high() : base(_HIGH) { }
		}

		public sealed class urgent : PX.Data.BQL.BqlString.Constant<urgent>
		{
			public urgent() : base(_URGENT) { }
		}
	}
	#endregion

	#region CRCasePriorityAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CRCasePriorityAttribute : PXStringListAttribute
	{
		public const string _LOW = "L";
		public const string _MEDIUM = "M";
		public const string _HIGH = "H";

		public CRCasePriorityAttribute()
			: base(new[] { _LOW, _MEDIUM, _HIGH },
			new[] { "Low", "Medium", "High"})
		{

		}

		public sealed class low : PX.Data.BQL.BqlString.Constant<low>
		{
			public low() : base(_LOW) { }
		}

		public sealed class medium : PX.Data.BQL.BqlString.Constant<medium>
		{
			public medium() : base(_MEDIUM) { }
		}

		public sealed class high : PX.Data.BQL.BqlString.Constant<high>
		{
			public high() : base(_HIGH) { }
		}
	}
	#endregion

	#region CRCurrentOwnerIDAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRCurrentOwnerIDAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeMaint.GetCurrentOwnerID(sender.Graph);
		}
	}

	#endregion

	#region PXActivityApplicationList
	public class PXActivityApplicationAttribute : PXIntListAttribute
	{
		public const int Portal = 0;
		public const int Backend = 1;
		public PXActivityApplicationAttribute()
			: base(new int[] {Portal, Backend},
				   new string[] {Messages.Portal, Messages.Backend})
		{			
		}

		public class portal : PX.Data.BQL.BqlInt.Constant<portal> { public portal() : base(Portal){}}
		public class backend : PX.Data.BQL.BqlInt.Constant<backend> { public backend() : base(Backend) { } }
	}
	#endregion

	#region ActivityStatusListAttribute

	public class ActivityStatusListAttribute : PXStringListAttribute
	{
		public const string Draft = "DR";
		public const string Open = "OP";
		public const string Completed = "CD";
		public const string Approved = "AP";
		public const string Rejected = "RJ";
		public const string Canceled = "CL";
		public const string InProcess = "IP";

		public const string PendingApproval = "PA";
		public const string Released = "RL";


		public ActivityStatusListAttribute()
			: base(
				new[]
				{
					Draft,
					Open,
					InProcess,
					Completed,
					Approved,
					Rejected,
					Canceled,
					PendingApproval,
					Released
				},
				new[]
				{
					EP.Messages.Draft,
					EP.Messages.Open,
					EP.Messages.InProcess,
					EP.Messages.Completed,
					EP.Messages.Approved,
					EP.Messages.Rejected,
					EP.Messages.Canceled,
					EP.Messages.Balanced,
					EP.Messages.Released
				})
		{
			RestictedMode = false;
		}

		public bool RestictedMode { get; set; }
		/// <summary>Sets the restricted mode of the specified field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>ActivityStatusList</tt> type.</param>
		/// <param name="restictedMode">The new restricted mode value.</param>
		public static void SetRestictedMode<Field>(PXCache cache, bool restictedMode)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is ActivityStatusListAttribute)
				{
					((ActivityStatusListAttribute)attr).RestictedMode = restictedMode;
					break;
				}
			}
		}
		public class draft : PX.Data.BQL.BqlString.Constant<draft>
		{
			public draft() : base(Draft) { }
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { }
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { }
		}		
		public class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved) { }
		}

		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}

		public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
		{
			public canceled() : base(Canceled) { }
		}

		public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { }
		}

		public class inprocess : PX.Data.BQL.BqlString.Constant<inprocess>
		{
			public inprocess() : base(InProcess) { }
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if (!RestictedMode) return;

			PXStringState state = e.ReturnState as PXStringState;
			if (state == null) return;

			string[] allowedValues = AllowedState(sender, e.Row);
			if (allowedValues != null)
			{
				string[] allowedLabels = new string[allowedValues.Length];
				for (int i = 0; i < allowedValues.Length; i++)
				{
					int index = Array.IndexOf(state.AllowedValues, allowedValues[i]);
					allowedLabels[i] = state.AllowedLabels[index];
				}
				state.AllowedValues = allowedValues;
				state.AllowedLabels = allowedLabels;
			}			
		}

		protected virtual string[] AllowedState(PXCache sender, object row)
		{
			return null;
		}
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!RestictedMode) return;
			string[] allowedStates = AllowedState(sender, e.Row);
			if (!allowedStates.Contains(e.NewValue))
				throw new PXSetPropertyException(Messages.ActivityStatusValidation);
		}

	}

	#endregion

	#region ApprovalStatusListAttribute

	public class ApprovalStatusListAttribute : PXStringListAttribute
	{
		public const string NotRequired = "NR";
		public const string Approved = "AP";
		public const string Rejected = "RJ";
		public const string PartiallyApprove = "PR";
		public const string PendingApproval = "PA";

		public ApprovalStatusListAttribute()
			: base(
				new[]
				{
					NotRequired,
					Approved,
					Rejected,
					PartiallyApprove,
					PendingApproval
				},
				new[]
				{
					EP.Messages.NotRequired,
					EP.Messages.Approved,
					EP.Messages.Rejected,
					EP.Messages.PartiallyApprove,
					EP.Messages.PendingApproval
				})
		{
		}


		public class notRequired : PX.Data.BQL.BqlString.Constant<notRequired>
		{
			public notRequired() : base(NotRequired) { }
		}

		public class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved) { }
		}

		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}

		public class partiallyApprove : PX.Data.BQL.BqlString.Constant<partiallyApprove>
		{
			public partiallyApprove() : base(PartiallyApprove) { }
		}

		public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

	}

	#endregion

	#region ActivityStatusAttribute
	public class ActivityStatusAttribute : ActivityStatusListAttribute, IPXFieldVerifyingSubscriber
	{
	    public ActivityStatusAttribute()
	    {            
        }
		protected override string[] AllowedState(PXCache sender, object row)
		{
			bool skipReading = false;
			if (row is CRActivity)
				skipReading = sender.GetStatus(row) == PXEntryStatus.Inserted;

			var status = skipReading ? null : sender.GetValueOriginal(row, _FieldName);

			if (row == null || status == null)
				status = Open;

			switch ((string) status)
			{
				case Draft:
					return new[] {Draft, Open, Completed, Canceled };
				case Open:					
				case Canceled:					
				case Completed:
					return new[] {Open, Completed, Canceled };
			}

			return base.AllowedState(sender, row);
		}
	}
	#endregion

	#region ApprovalStatusAttribute
	public class ApprovalStatusAttribute : ActivityStatusListAttribute
	{		
		protected override string[] AllowedState(PXCache sender, object row)
		{
			bool skipReading = false;
			if (row is PMTimeActivity)
				skipReading = sender.GetStatus(row) == PXEntryStatus.Inserted;

			var status = skipReading ? null : sender.GetValueOriginal(row, _FieldName);

			if (row == null || status == null)
				status = Open;

			switch ((string)status)
			{
				case Open:					
				case Canceled:					
				case Completed:
					return new[] { Open, Completed, Canceled };				
				case PendingApproval:
					return new[] { Open, PendingApproval, Canceled };
				case Approved:
					return new[] { Open, Approved, Canceled };
				case Rejected:
					return new[] { Open, Rejected, Canceled };
				case Released:
					return new[] { Released };
			}
			return base.AllowedState(sender, row);
		}		
	}
	#endregion

	#region TaskStatusAttribute
	public class TaskStatusAttribute : ActivityStatusListAttribute
	{
	    public TaskStatusAttribute()
	    {	        
	    }
        protected override string[] AllowedState(PXCache sender, object row)
		{
			bool skipReading = false;
			if (row is CRActivity)
				skipReading = sender.GetStatus(row) == PXEntryStatus.Inserted;
			var status = skipReading ? null : sender.GetValueOriginal(row, _FieldName);

			if (row != null && status == null)
				status = Open;

			switch ((string) status)
			{
				case Draft:
					return new[] {Draft, Open, Canceled};
                case InProcess:
                case Open:
					return new[] {Open, Draft, InProcess, Canceled, Completed};                                    
                case Canceled:
					return new[] {Open, InProcess, Canceled};
				case Completed:
					return new[] {Open, InProcess, Completed};
			}
			return base.AllowedState(sender, row);
		}
	}
	#endregion

	#region MailStatusListAttribute

	public class MailStatusListAttribute : PXStringListAttribute
	{
		public const string Draft = "DR";
		public const string PreProcess = "PP";
		public const string InProcess = "IP";
		public const string Scheduled = "SC";
		public const string Processed = "PD";
		public const string Failed = "FL";
		public const string Canceled = "CL";
		public const string Deleted = "DL";
		[Obsolete("This object is obsolete and will be removed. Rewrite your code without this object or contact your partner for assistance.")]
		public const string Archived = "AR";

		public MailStatusListAttribute()
			: base(
				new[]
				{
					Draft,
					PreProcess,
					InProcess,
					Scheduled,
					Processed,
					Canceled,
					Failed,
					Deleted
				},
				new[]
				{
					EP.Messages.Draft,
					EP.Messages.PreProcess,
					EP.Messages.InProcess,
					EP.Messages.Scheduled,
					EP.Messages.Processed,
					EP.Messages.Canceled,
					EP.Messages.Failed,
					EP.Messages.EmailDeleted
				})
		{
		}

		public class draft : PX.Data.BQL.BqlString.Constant<draft>
		{
			public draft() : base(Draft) { }
		}
		public class preProcess : PX.Data.BQL.BqlString.Constant<preProcess>
		{
			public preProcess() : base(PreProcess) { }
		}
		public class inProcess : PX.Data.BQL.BqlString.Constant<inProcess>
		{
			public inProcess() : base(InProcess) { }
		}

		public class processed : PX.Data.BQL.BqlString.Constant<processed>
		{
			public processed() : base(Processed) { }
		}

		public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
		{
			public canceled() : base(Canceled) { }
		}

		public class failed : PX.Data.BQL.BqlString.Constant<failed>
		{
			public failed() : base(Failed) { }
		}

		public class deleted : PX.Data.BQL.BqlString.Constant<deleted>
		{
			public deleted() : base(Deleted) { }
		}
	}

	#endregion

	#region MergeMethodsAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[Obsolete]
	public class MergeMethodsAttribute : PXIntListAttribute
	{
		public const int _FIRST = 0;
		public const int _SELECT = 1;
		public const int _CONCAT = 2;
		public const int _SUM = 3;
		public const int _MAX = 4;
		public const int _MIN = 5;
		public const int _COUNT = 6;

		private const string _FIRST_LABEL = "First";
		private const string _SELECT_LABEL = "Select";
		private const string _CONCAT_LABEL = "Concat";
		private const string _SUM_LABEL = "Sum";
		private const string _MAX_LABEL = "Max";
		private const string _MIN_LABEL = "Min";
		private const string _COUNT_LABEL = "Count";

		private static readonly int[] _NUMBER_VALUES = 
			new[] { _FIRST, _SELECT, _SUM, _MAX, _MIN, _COUNT };
		private static readonly string[] _NUMBER_LABELS = 
			new[] { _FIRST_LABEL, _SELECT_LABEL, _SUM_LABEL, _MAX_LABEL, _MIN_LABEL, _COUNT_LABEL };

		private static readonly int[] _STRING_VALUES =
			new[] { _FIRST, _SELECT, _CONCAT };
		private static readonly string[] _STRING_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL, _CONCAT_LABEL };

		private static readonly int[] _DATE_VALUES =
			new[] { _FIRST, _SELECT, _MAX, _MIN };
		private static readonly string[] _DATE_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL, _MAX_LABEL, _MIN_LABEL };

		private static readonly int[] _COMMON_VALUES =
			new[] { _FIRST, _SELECT };
		private static readonly string[] _COMMON_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL };

		[Obsolete]
		public MergeMethodsAttribute()
			: base(new [] { _FIRST, _SELECT, _CONCAT, _SUM, _MAX, _MIN, _COUNT }, 
			new [] { _FIRST_LABEL, _SELECT_LABEL, _CONCAT_LABEL, _SUM_LABEL, _MAX_LABEL, _MIN_LABEL, _COUNT_LABEL })
		{
			
		}

		public static void SetNumberList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _NUMBER_VALUES, _NUMBER_LABELS);
		}

		public static void SetStringList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _STRING_VALUES, _STRING_LABELS);
		}

		public static void SetDateList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _DATE_VALUES, _DATE_LABELS);
		}

		public static void SetCommonList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _COMMON_VALUES, _COMMON_LABELS);
		}
	}

	#endregion

	#region MergableTypesSelectorAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	[Serializable]
	[Obsolete]
	public sealed class MergableTypesSelectorAttribute : PXCustomSelectorAttribute
	{
		#region EntityInfo

		[Serializable]
		[PXHidden]
		public class EntityInfo : IBqlTable
		{
			#region Key

			public abstract class key : PX.Data.BQL.BqlString.Field<key> { }

			[PXString(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual String Key { get; set; }

			#endregion

			#region Name

			public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

			[PXString(IsUnicode = true)]
			[PXUIField(DisplayName = "Name")]
			public virtual String Name { get; set; }

			#endregion
		}

		#endregion
		[Obsolete]
		public MergableTypesSelectorAttribute()
			: base(typeof(EntityInfo.key), typeof(EntityInfo.name))
		{
			base.DescriptionField = typeof (EntityInfo.name);
		}

		public override Type DescriptionField
		{
			get
			{
				return base.DescriptionField;
			}
			set
			{
			}
		}

		public IEnumerable GetRecords()
		{
			//TODO: need implement collection types by special marker attribute
			yield return GenerateInfo(typeof(Contact));
			yield return GenerateInfo(typeof(BAccount));
			yield return GenerateInfo(typeof(CRCampaign));
		}

		public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
		{
			var deleted = false;
			if (e.Row != null)
			{
				var infoKey =  sender.GetValue(e.Row, _FieldOrdinal) as string;
				var infoType = string.IsNullOrEmpty(infoKey) ? null : System.Web.Compilation.PXBuildManager.GetType(infoKey, false);
				if (infoType == null)
				{
					deleted = true;
					e.ReturnValue = infoKey;
				}
				else
					e.ReturnValue = GenerateInfo(infoType).Name;
			}

			if (e.Row == null || e.IsAltered)
			{
				int? length;
				string displayname = getDescriptionName(sender, out length);
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, length, null,
					alias, null, displayname, deleted ? ErrorMessages.ForeignRecordDeleted : null, 
					deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined, false, true, null, PXUIVisibility.Invisible, null, null, null);
			}
		}

		private EntityInfo GenerateInfo(Type type)
		{
			var displayName = EntityHelper.GetFriendlyEntityName(type);
			return new EntityInfo { Key = type.GetLongName(), Name = displayName };
		}
	}

	#endregion

	#region MergeMatchingTypesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[Obsolete]
	public class MergeMatchingTypesAttribute : PXIntListAttribute
	{
		public const int _EQUALS_TO = 0;
		public const int _LIKE = 1;
		public const int _THE_SAME = 2;
		public const int _GREATER_THAN = 3;
		public const int _LESS_THAN = 4;

		private static readonly int[] _commonValues;
		private static readonly string[] _commonLabels;

		private static readonly int[] _comparableValues;
		private static readonly string[] _comparableLabels;

		[Obsolete]
		static MergeMatchingTypesAttribute()
		{
			_commonValues = new[] { _EQUALS_TO, _LIKE, _THE_SAME };
			_commonLabels = new[] { "Equals To", "Like", "The Same" };
			_comparableValues = new[] { _EQUALS_TO, _LIKE, _THE_SAME, _GREATER_THAN, _LESS_THAN };
			_comparableLabels = new[] { "Equals To", "Like", "The Same", "Greater Than", "Less Than" };
		}

		[Obsolete]
		public MergeMatchingTypesAttribute()
			: base(CommonValues, CommonLabels)
		{
			
		}

		public static int[] CommonValues
		{
			get { return _commonValues; }
		}

		public static string[] CommonLabels
		{
			get { return _commonLabels; }
		}

		public static int[] ComparableValues
		{
			get { return _comparableValues; }
		}

		public static string[] ComparableLabels
		{
			get { return _comparableLabels; }
		}
	}

	#endregion

	#region CaseSourcesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CaseSourcesAttribute : PXStringListAttribute
	{
		public const string _EMAIL = "EM";
		public const string _PHONE = "PH";
		public const string _WEB = "WB";
		public const string _CHAT = "CH";

		public CaseSourcesAttribute()
			: base(new [] { _EMAIL, _PHONE, _WEB, _CHAT }, 
					new [] { Messages.CaseSourceEmail, Messages.CaseSourcePhone, Messages.CaseSourceWeb, Messages.CaseSourceChat })
		{}

		public sealed class Email : PX.Data.BQL.BqlString.Constant<Email>
		{
			public Email() : base(_EMAIL) { }
		}

		public sealed class Phone : PX.Data.BQL.BqlString.Constant<Phone>
		{
			public Phone() : base(_PHONE) { }
		}

		public sealed class Web : PX.Data.BQL.BqlString.Constant<Web>
		{
			public Web() : base(_WEB) { }
		}

		public sealed class Chat : PX.Data.BQL.BqlString.Constant<Chat>
		{
			public Chat() : base(_CHAT) { }
		}
	}

	#endregion

	#region PXCheckCurrentAttribute

	public class PXCheckCurrentAttribute : PXViewExtensionAttribute
	{
		private string _hostViewName;

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_hostViewName = viewName;

			graph.Initialized += sender =>
			{
				var cache = GetCache(sender);
				var record = cache.Current;
				if (record == null)
				{
					var itemType = cache.GetItemType();
					string name = itemType.Name;
					if (itemType.IsDefined(typeof(PXCacheNameAttribute), true))
					{
						PXCacheNameAttribute attr = (PXCacheNameAttribute)(itemType.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
						name = attr.GetName();
					}
					throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, itemType, name);
				}
			};
		}

		private PXCache GetCache(PXGraph graph)
		{
			return graph.Views[_hostViewName].Cache;
		}
	}

	#endregion
	
	#region CRLeadFullNameAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[PXDBString(255, IsUnicode = true)]
	public class CRLeadFullNameAttribute : PXAggregateAttribute
	{
		private readonly Type _accountIdBqlField;

		private int _accountIdfieldOrdinal;

		public CRLeadFullNameAttribute(Type accountIdBqlField)
		{
			if (accountIdBqlField == null) throw new ArgumentNullException("accountIdBqlField");

			_accountIdBqlField = accountIdBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();
			if (!typeof(Contact).IsAssignableFrom(itemType))
				throw new Exception(string.Format("Attribute '{0}' can be used only with DAC '{1}' or its inheritors",
					GetType().Name, typeof(Contact).Name));

			_accountIdfieldOrdinal = GetFieldOrdinal(sender, itemType, _accountIdBqlField);

			sender.Graph.RowSelected.AddHandler(itemType, Handler);
		}

		private int GetFieldOrdinal(PXCache sender, Type itemType, Type bqlField)
		{
			var fieldName = sender.GetField(bqlField);
			if (string.IsNullOrEmpty(fieldName))
				throw new Exception(string.Format("Field '{0}' can not be not found in table '{1}'",
					bqlField.Name, itemType.Name));
			return sender.GetFieldOrdinal(fieldName);
		}

		private void Handler(PXCache sender, PXRowSelectedEventArgs e)
		{
            Contact row = e.Row as Contact;
            if (row == null) return;
            if (row.ContactType == ContactTypesAttribute.Person || row.ContactType == ContactTypesAttribute.Lead)
            {
                var accountId = sender.GetValue(e.Row, _accountIdfieldOrdinal);
                if (accountId != null)
                {
                    PXUIFieldAttribute.SetEnabled<Contact.fullName>(sender, e.Row, false);

                    var account = (BAccount)PXSelect<BAccount,
                        Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
                        SelectSingleBound(sender.Graph, null, accountId);
                    var newValue = account.With(_ => _.AcctName);
                    if (newValue == null) return;
                    sender.SetValue(e.Row, _FieldOrdinal, newValue);
                }
            }
        }
	}

	#endregion

	#region ContactSelectorAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]    
    public class ContactSelectorAttribute : PXSelectorAttribute
    {
	    public ContactSelectorAttribute(bool showContactsWithNullEmail, params Type[] contactTypes)
           : base(GetQuery(typeof(Contact.contactID), showContactsWithNullEmail, contactTypes))
        {
            if (contactTypes == null || contactTypes.Length == 0)
                throw new ArgumentNullException(nameof(contactTypes));

            DescriptionField = typeof(Contact.displayName);			
        }
            
	    protected ContactSelectorAttribute(Type searchField, bool showContactsWithNullEmail,  Type[] contactTypes, Type[] fieldList)
            : base(GetQuery(searchField, showContactsWithNullEmail, contactTypes), fieldList)
        {
            if (contactTypes == null || contactTypes.Length == 0)
                throw new ArgumentNullException(nameof(contactTypes));

            DescriptionField = typeof(Contact.displayName);
        }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			this.SelectorMode = PXSelectorMode.DisplayModeHint;			
		}

		private static Type GetQuery(Type searchField, bool showContactsWithNullEmail, Type[] contactTypes)
		{
			Type groupMatchClause = BqlCommand.Compose(
				typeof(Where<,,>),
					typeof(BAccount.bAccountID), typeof(IsNull),
					typeof(Or<>), typeof(Match<,>), typeof(BAccount), typeof(Current<>), typeof(AccessInfo.userName));

			Type contactTypeClause = null;

			if (contactTypes.Length == 1)
            {
                contactTypeClause = BqlCommand.Compose(typeof(Where<,,>), typeof(Contact.contactType),
                    typeof(Equal<>), contactTypes.Single(), typeof(And<>), groupMatchClause);
            }
            else
            {
                var allContactTypes =
                    typeof(ContactTypesAttribute).GetNestedTypes().Where(x => typeof(IConstant<string>).IsAssignableFrom(x));
                var invertedContactTypes = allContactTypes.Except(contactTypes);

                foreach (var contactType in invertedContactTypes)
                {
                    if (contactTypeClause == null)
                    {
                        contactTypeClause = BqlCommand.Compose(typeof(Where<,,>), typeof(Contact.contactType),
                            typeof(NotEqual<>), contactType, typeof(And<>), groupMatchClause);
                    }
                    else
                    {
                        contactTypeClause = BqlCommand.Compose(typeof(Where<,,>), typeof(Contact.contactType),
                            typeof(NotEqual<>), contactType, typeof(And<>), contactTypeClause);
                    }
                }
            }

            Type command;

            if (showContactsWithNullEmail)
            {
                command = BqlCommand.Compose(
                    typeof(Search2<,,>), searchField,
                    typeof(LeftJoin<,>), typeof(BAccount),
                    typeof(On<,>), typeof(BAccount.bAccountID), typeof(Equal<>), typeof(Contact.bAccountID),
                    contactTypeClause);
            }
            else
            {
                var notNullClause = BqlCommand.Compose(typeof(Where<,>), typeof(Contact.eMail), typeof(IsNotNull));

                command = BqlCommand.Compose(
                    typeof(Search2<,,>), searchField,
                    typeof(LeftJoin<,>), typeof(BAccount),
                    typeof(On<,>), typeof(BAccount.bAccountID), typeof(Equal<>), typeof(Contact.bAccountID),
                    typeof(Where2<,>), notNullClause, typeof(And2<,>), contactTypeClause, typeof(And<>), groupMatchClause);
            }

            return command;
        }
    }

	#endregion

	#region CRLeadSelectorAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class CRLeadSelectorAttribute : PXSelectorAttribute
	{
		public CRLeadSelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			this.SelectorMode = PXSelectorMode.DisplayModeHint;
		}
	}

	#endregion

	#region AssignedDateAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[PXDBDate(PreserveTime = true)]
	public class AssignedDateAttribute : PXAggregateAttribute
	{
		private readonly Type _workgorupID;
		private readonly Type _ownerID;        

		public AssignedDateAttribute(Type workgroupID, Type ownerID)
		{
			_workgorupID = workgroupID;
			_ownerID = ownerID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();

			if (string.IsNullOrEmpty(sender.GetField(_workgorupID)))
				throw new Exception(string.Format("Field '{0}' can not be not found in table '{1}'",
					_workgorupID.Name, itemType.Name));


			if (string.IsNullOrEmpty(sender.GetField(_ownerID)))
				throw new Exception(string.Format("Field '{0}' can not be not found in table '{1}'",
					_ownerID.Name, itemType.Name));

			sender.Graph.RowInserted.AddHandler(itemType, RowInsertedHandler);
			sender.Graph.RowUpdated.AddHandler(itemType, RowUpdatedHandler);
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (sender.GetValue(e.Row, _ownerID.Name) == null && 
				sender.GetValue(e.Row, _workgorupID.Name) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
			else
			{
				if (!object.Equals(sender.GetValue(e.Row, _ownerID.Name), sender.GetValue(e.OldRow, _ownerID.Name)) ||
					!object.Equals(sender.GetValue(e.Row, _workgorupID.Name), sender.GetValue(e.OldRow, _workgorupID.Name)))
					sender.SetValue(e.Row, _FieldOrdinal, PXTimeZoneInfo.Now);
			}
		}

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			object newValue = null;			
			if (sender.GetValue(e.Row, _ownerID.Name) != null ||
				sender.GetValue(e.Row, _workgorupID.Name) != null)
				newValue = PXTimeZoneInfo.Now;
			sender.SetValue(e.Row, _FieldOrdinal, newValue);
		}
	}

	#endregion

	#region CRAttributesFieldAttribute
	public interface IBqlAttributes : PX.Data.BQL.IBqlDataType { }
	public sealed class BqlAttributes : PX.Data.BQL.BqlType<IBqlAttributes, string[]> { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class CRAttributesFieldAttribute : PXDBAttributeAttribute
    {
		protected readonly string EntityType;

        public Type ClassIdField { get; private set; }
		public Type[] RelatedEntityTypes { get; private set; }

		public CRAttributesFieldAttribute(Type classIdField) : this(classIdField, null, null)
	    {
	    }

	    public CRAttributesFieldAttribute(Type classIdField, Type noteIdField) : this(classIdField, noteIdField, null)
	    {
	    }

	    public CRAttributesFieldAttribute(Type classIdField, Type[] relatedEntityTypes)
		    : this(classIdField, null, relatedEntityTypes)
	    {
	    }

	    public CRAttributesFieldAttribute(Type classIdField, Type noteIdField, Type[] relatedEntityTypes)
		    : base(
			    GetValueSearchCommand(classIdField, noteIdField), typeof (CSAnswers.attributeID),
				    GetAttributesSearchCommand(classIdField))
	    {
		    ClassIdField = classIdField;
		    if (classIdField != null && classIdField.DeclaringType != null)
			    EntityType = classIdField.DeclaringType.FullName;
		    RelatedEntityTypes = relatedEntityTypes;
	    }

		protected CRAttributesFieldAttribute(Type classIdField, Type attributeID, Type valueSearch, Type attributeSearch)
			: base(valueSearch, attributeID, attributeSearch ?? GetAttributesSearchCommand(classIdField))
		{
			ClassIdField = classIdField;
			if (classIdField != null && classIdField.DeclaringType != null)
				EntityType = classIdField.DeclaringType.FullName;
		}

		protected static Type GetAttributesSearchCommand(Type classIdField)
        {
            var cmd = BqlCommand.Compose(typeof (Search2<,,>), typeof (CSAttribute.attributeID),
                typeof (InnerJoin<,>), typeof (CSAttributeGroup),
                typeof (On<,>), typeof (CSAttributeGroup.attributeID), typeof (Equal<>),
                typeof (CSAttribute.attributeID),
                typeof(Where<,,>), typeof(CSAttributeGroup.entityType), typeof(Equal<>), typeof(Required<>), typeof(CSAttributeGroup.entityType),
                typeof (And<,>), typeof (CSAttributeGroup.entityClassID), typeof (Equal<>), typeof (Current<>),
                classIdField);
            return cmd;
        }

		protected static Type GetValueSearchCommand(Type classIdField, Type noteIdField)
        {
            var entityType = classIdField.DeclaringType;

            noteIdField = noteIdField ?? EntityHelper.GetNoteType(entityType);

            Type cmd = BqlCommand.Compose(typeof (Search<,>), typeof (CSAnswers.value),
                typeof (Where<,>), typeof (CSAnswers.refNoteID), typeof (Equal<>), noteIdField);

            return cmd;
        }

		protected class CRDefinition : IPrefetchable<CRDefinition.Parameters>
        {
            public class Parameters
            {
                public readonly string FieldName;
                public readonly string EntityType;

                public Parameters(string fieldName, string entityType)
                {
                    this.FieldName = fieldName;
                    this.EntityType = entityType;
                }
            }

            public PXFieldState[] Fields;
            public void Prefetch(Parameters parameters)
            {
                List<PXFieldState> fields = new List<PXFieldState>();
                var list = CRAttribute.EntityAttributes(parameters.EntityType);
                foreach (string key in list.Keys.OrderBy(s => s))
                {
                    CRAttribute.Attribute attr = list[key];
                    var state = PXDBAttributeAttribute.Definition.CreateFieldState(attr.ID + '_' + parameters.FieldName,
                        attr.Description,
                        attr.ControlType.GetValueOrDefault(),
                        attr.EntryMask,
                        attr.List);

                    if (state != null)
                        fields.Add(state);
                }
                Fields = fields.ToArray();
            }
        }

        protected override PXFieldState[] GetSlot(string name, DefinitionParams definitionParams, Type[] tables)
        {
            if (EntityType != null)
            {
                CRDefinition def = PXDatabase.GetSlot<CRDefinition, CRDefinition.Parameters>(name,
                    new CRDefinition.Parameters(definitionParams.FieldName, EntityType), tables);
                return def == null ? null : def.Fields;
            }
            else
                return base.GetSlot(name, definitionParams, tables);
        }
    }

    #endregion

	#region CROpportunityStagesAttribute

    public class CROpportunityStagesAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber, IPXRowUpdatedSubscriber, IPXRowSelectedSubscriber
    {
        private readonly Type _oppClassID;
        private readonly Type _stageChangedDate;

		public override bool IsLocalizable => true;
		public bool OnlyActiveStages { get; set; }

		public CROpportunityStagesAttribute(Type oppClassID, Type stageChangedDate = null) : base()
        {
            if (oppClassID == null)
                throw new ArgumentNullException(nameof(oppClassID));

            if (!typeof(IBqlField).IsAssignableFrom(oppClassID))
                throw new ArgumentException(typeof(IBqlField).GetLongName() + " expected.", nameof(oppClassID));

            if (stageChangedDate != null && !typeof(IBqlField).IsAssignableFrom(stageChangedDate))
                throw new ArgumentException(typeof(IBqlField).GetLongName() + " expected.", nameof(stageChangedDate));

            _oppClassID = oppClassID;
            _stageChangedDate = stageChangedDate;

			TryLocalize(null);
        }

		#region Events

        public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            base.FieldSelecting(cache, e);

            if (e.Row == null) return;

            // get all stages if we need not only active stages or we have not got classID value
            string classID, stageID;

            List<CROppClassStage> stages =
                (!OnlyActiveStages || !TryGetClassAndStage(cache, e.Row, out classID, out stageID)) ?
                    GetClassStages(string.Empty) :
                    GetClassStages(classID).Where(s => s.IsActive == true || s.StageID == stageID).ToList();

            string[] values = stages.Select(x => x.StageID).ToArray();
            string[] labels = stages.Select(x => Messages.GetLocal(x.Name)).ToArray();

            e.ReturnState = (PXStringState)PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, labels, true, null, _NeutralAllowedLabels);
        }

        public virtual void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null) return;

            string classID, stageID;
            if (TryGetClassAndStage(cache, e.Row, out classID, out stageID))
            {
                List<CROppClassStage> classStages = GetClassStages(classID);
                CROppClassStage firstActiveStage = classStages.FirstOrDefault(s => s.IsActive == true);
                e.NewValue = firstActiveStage?.StageID;
            }
        }

        public virtual void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null) return;

            string classID, stageID, oldClassID, oldStageID;
            if (TryGetClassAndStage(cache, e.Row, out classID, out stageID) && TryGetClassAndStage(cache, e.OldRow, out oldClassID, out oldStageID))
            {
				if (classID == null && stageID != null)
				{
					stageID = null;
				}

                if (classID != oldClassID && stageID == null)
	{
                    object newStageID;
                    cache.RaiseFieldDefaulting(_FieldName, e.Row, out newStageID);
                    cache.SetValue(e.Row, _FieldName, newStageID);
                    stageID = cache.GetValue(e.Row, _FieldName) as string;
                }

                if (stageID != oldStageID && _stageChangedDate != null)
		{
                    string stateChangedDateField = cache.GetField(_stageChangedDate);
                    DateTime? stateChangedDate = cache.GetValue(e.Row, stateChangedDateField) as DateTime?;
			
                    // if current StateChangedDate has been set earlier, then update it
                    if (stateChangedDate != null)
                    {
                        cache.SetValue(e.Row, stateChangedDateField, PXTimeZoneInfo.Now);
                    }
                    else
                    {
                        object defaultStage;
                        cache.RaiseFieldDefaulting(_FieldName, e.Row, out defaultStage);
                        // if current StateChangedDate has not been set earlier and stage != default stage, then update StateChangedDate
                        if (stageID != (string)defaultStage)
                        {
                            cache.SetValue(e.Row, stateChangedDateField, PXTimeZoneInfo.Now);
                        }
                    }
		}
            }
        }

        public virtual void RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            string classID, stageID;
            if (TryGetClassAndStage(cache, e.Row, out classID, out stageID))
            {
                CROppClassStage currentStage = GetClassStages(classID).SingleOrDefault(s => s.StageID == stageID);

                if (classID != null && currentStage?.IsActive == false)
                {
                    cache.RaiseExceptionHandling(_FieldName, e.Row, stageID, new PXSetPropertyException(Messages.StageIsDisabledInOpportunityClass, PXErrorLevel.Error));
                }
                else
                {
                    var state = (PXFieldState)cache.GetStateExt(e.Row, _FieldName);
                    if (state.ErrorLevel == PXErrorLevel.Error)
                        cache.RaiseExceptionHandling(_FieldName, e.Row, null, null);
                }
            }
        }

        protected bool TryGetClassAndStage(PXCache cache, object row, out string classID, out string stageID)
        {
            string classIdFieldName = cache.GetField(_oppClassID);

            if (cache.Fields.Contains(classIdFieldName) && cache.Fields.Contains(_FieldName))
		{
                classID = cache.GetValue(row, classIdFieldName) as string;
                stageID = cache.GetValue(row, _FieldName) as string;
                return true;
            }
            else
			{
                classID = null;
                stageID = null;
                return false;
            }
        }
				
		#endregion

        #region Opportunity Class Stages Cache
        protected class CROppClassStage
        {
            public string StageID { get; set; }
            public string Name { get; set; }
			public string NeutralName { get; set; }
            public int? SortOrder { get; set; }
            public int? Probability { get; set; }
            public bool? IsActive { get; set; }
			}

		protected override void TryLocalize(PXCache sender)
        {
			// if don't do it for _AllowedValues during localization, GI would contain wrong labels for values
			// set default list with all stages
			List<CROppClassStage> stages;
			try
			{
				stages = GetClassStages(string.Empty);
			}
			catch(PXInvalidOperationException e)
			{
				PXTrace.WriteError(e);
				return;
			}

			_AllowedValues = stages.Select(x => x.StageID).ToArray();
			_AllowedLabels = stages.Select(x => x.Name).ToArray();
			_NeutralAllowedLabels = stages.Select(x => x.NeutralName).ToArray();
		}

		protected virtual List<CROppClassStage> GetClassStages(string classID)
		{
			var definitions = Definitions;
			if (definitions == null)
				throw new PXInvalidOperationException(MessagesNoPrefix.OpportunityStagesSlotWasNotInitialized);
			
			return definitions.ClassStages.TryGetValue(classID ?? string.Empty, out var classStages)
				? classStages
				: definitions.ClassStages[string.Empty];
		}

        private Definition Definitions
			=> PXContext.GetSlot<Definition>()
				?? PXContext.SetSlot(
					PXDatabase.GetLocalizableSlot<Definition>(
						typeof(Definition).FullName,
						typeof(CROpportunityProbability),
						typeof(CROpportunityClassProbability)));

        private class Definition : IPrefetchable
        {
            public readonly Dictionary<string, List<CROppClassStage>> ClassStages = new Dictionary<string, List<CROppClassStage>>();

            public void Prefetch()
            {
                ClassStages.Clear();

                using (new PXConnectionScope())
                {
                    // create full list of stages
					var allStages =
                        PXDatabase.SelectMulti<CROpportunityProbability>(
                            new PXDataField<CROpportunityProbability.stageCode>(),
							new PXDataField<CROpportunityProbability.name>(),
							PXDBLocalizableStringAttribute.GetValueSelect(
								nameof(CROpportunityProbability),
								nameof(CROpportunityProbability.Name),
								false),
                            new PXDataField<CROpportunityProbability.sortOrder>(),
                            new PXDataField<CROpportunityProbability.probability>())
						.Select(x => new CROppClassStage() {
                                        StageID = x.GetString(0),
							NeutralName = x.GetString(1),
							Name = x.GetString(2),
							SortOrder = x.GetInt32(3),
							Probability = x.GetInt32(4),
                                        IsActive = false
                                    })
						.OrderBy(x => x.SortOrder)
						.ThenBy(x => x.Probability)
						.ThenBy(x => x.StageID)
						.ToList();

                    // add full list for empty classID
					ClassStages.Add(string.Empty, allStages);

                    // set IsActive flag for corresponded stages in classes
                    foreach (PXDataRecord record in PXDatabase.SelectMulti<CROpportunityClassProbability>(
                        new PXDataField<CROpportunityClassProbability.classID>(),
                        new PXDataField<CROpportunityClassProbability.stageID>()))
		{
                        string classID = record.GetString(0);
                        string stageID = record.GetString(1);

                        if (!ClassStages.ContainsKey(classID))
			{
                            ClassStages.Add(classID, CloneStagesList(allStages));
                        }

                        CROppClassStage stage = ClassStages[classID].FirstOrDefault(s => s.StageID == stageID);
                        if (stage != null)
                            stage.IsActive = true;
                    }
                }
            }

            private static List<CROppClassStage> CloneStagesList(List<CROppClassStage> stages)
            {
                return stages.Select(s =>
					new CROppClassStage() {
                        StageID = s.StageID,
                        Name = s.Name,
						NeutralName = s.NeutralName,
                        SortOrder = s.SortOrder,
                        Probability = s.Probability,
						IsActive = s.IsActive,
			}
                ).ToList();
            }
		}
        #endregion
	}

	#endregion	

	#region CRCaseBillableTimeAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	internal sealed class CRCaseBillableTimeAttribute : PXEventSubscriberAttribute
	{
		private PXGraph _graph;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_graph = sender.Graph;
			_graph.RowSelected.AddHandler<CRCase>((s, e) => CalculateBillableTime(e.Row as CRCase));
		}

		private void CalculateBillableTime(CRCase caseRow)
		{
			if (caseRow == null) return;

			var billPerActivities = false;
			CRCaseClass caseClass = (CRCaseClass)PXSelectorAttribute.Select<CRCase.caseClassID>(_graph.Caches[typeof(CRCase)], caseRow);
			if (caseClass != null)
				billPerActivities = caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity;

			var recalcBillableTimes = billPerActivities || caseRow.ManualBillableTimes != true;
			if (!recalcBillableTimes) return;

			caseRow.TimeSpent = 0;
			caseRow.OvertimeSpent = 0;
			caseRow.TimeBillable = 0;
			caseRow.OvertimeBillable = 0;

			foreach (CRPMTimeActivity activity in PXSelect<CRPMTimeActivity,
				Where<CRPMTimeActivity.refNoteID, Equal<Required<CRPMTimeActivity.refNoteID>>, 
					And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.task>,
					And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.events>,
					And<Where<
						   CRPMTimeActivity.approvalStatus, Equal<ActivityStatusAttribute.completed>,
                         Or<CRPMTimeActivity.approvalStatus, Equal<ActivityStatusAttribute.pendingApproval>,
                         Or<CRPMTimeActivity.approvalStatus, Equal<ActivityStatusAttribute.approved>,
                        Or<CRPMTimeActivity.approvalStatus, Equal<ActivityStatusAttribute.released>>>>>>>>>>.
				Select(_graph, caseRow.NoteID))
			{
				caseRow.TimeSpent += activity.TimeSpent.GetValueOrDefault();
				caseRow.OvertimeSpent += activity.OvertimeSpent.GetValueOrDefault();

				var isBillable = activity.IsBillable == true;
				if (isBillable)
				{
					int timeBillable = activity.TimeBillable ?? 0;
					int overtimeBillable = activity.OvertimeBillable ?? 0;
					if (caseClass != null && caseClass.RoundingInMinutes > 1)
					{
						if (timeBillable > 0)
						{
							decimal fraction = Convert.ToDecimal(timeBillable)/Convert.ToDecimal(caseClass.RoundingInMinutes);
							int points = Convert.ToInt32(Math.Ceiling(fraction));
							timeBillable = points * (caseClass.RoundingInMinutes ?? 0);

							if (caseClass.MinBillTimeInMinutes > 0)
							{
								timeBillable = Math.Max(timeBillable, (int)caseClass.MinBillTimeInMinutes);
							}
						}

						if (overtimeBillable > 0)
						{
							decimal fraction = Convert.ToDecimal(overtimeBillable)/Convert.ToDecimal(caseClass.RoundingInMinutes);
							int points = Convert.ToInt32(Math.Ceiling(fraction));
							overtimeBillable = points * (caseClass.RoundingInMinutes ?? 0);

							if (caseClass.MinBillTimeInMinutes > 0)
							{
								overtimeBillable = Math.Max(overtimeBillable, (int)caseClass.MinBillTimeInMinutes);
							}
						}
					}

					caseRow.TimeBillable += timeBillable;
					caseRow.OvertimeBillable += overtimeBillable;
				}
			}
		}
	}

	#endregion

	#region CRDropDownAutoValueAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[Obsolete]
	public sealed class CRDropDownAutoValueAttribute : PXEventSubscriberAttribute
	{
		private readonly Type _refField;
		private int _refFieldOrdinal;
		private BqlCommand _bqlCommand;

		[Obsolete]
		public CRDropDownAutoValueAttribute(Type dependsOnField)
		{
			if (dependsOnField == null)
				throw new ArgumentNullException("dependsOnField");
			if (!typeof(IBqlField).IsAssignableFrom(dependsOnField))
				throw new ArgumentException(string.Format("'{0}' is expected.", typeof(IBqlField).GetLongName()));

			_refField = dependsOnField;
		}

		public bool CheckOnInsert { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_refFieldOrdinal = sender.GetFieldOrdinal(sender.GetField(_refField));
			if (CheckOnInsert) sender.Graph.RowInserting.AddHandler(_refField.DeclaringType, RowInsertingHandler);
			sender.Graph.RowUpdating.AddHandler(_refField.DeclaringType, RowUpdatingHandler);

			_bqlCommand = BqlCommand.CreateInstance(typeof(Select<>), _BqlTable);
		}

		private void RowInsertingHandler(PXCache sender, PXRowInsertingEventArgs e)
		{
			var currentValue = sender.GetValue(e.Row, _FieldOrdinal);

			var graph = sender.Graph;
			if (graph.Views[graph.PrimaryView].GetItemType() != sender.GetItemType()) 
				return;

			sender.SetValue(e.Row, _FieldOrdinal, null);
			graph.ApplyWorkflowState(e.Row);
			
			var state = sender.GetStateExt(e.Row, _FieldName) as PXStringState;
			sender.SetValue(e.Row, _FieldOrdinal, currentValue);
			if (state != null)
			{
				var allowedValues = state.AllowedValues;
				if (allowedValues != null && (string.IsNullOrEmpty(currentValue as string) || Array.IndexOf(allowedValues, (string)currentValue) < 0))
					sender.SetValue(e.Row, _FieldOrdinal, allowedValues.FirstOrDefault());
			}
			else if (currentValue != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var oldRefValue = sender.GetValue(e.Row, _refFieldOrdinal);
			var newRefValue = sender.GetValue(e.NewRow, _refFieldOrdinal);

			if (!Object.Equals(oldRefValue ,newRefValue))
			{
				var graph = sender.Graph;

				var currentValue = sender.GetValue(e.NewRow, _FieldOrdinal);
				sender.SetValue(e.NewRow, _FieldOrdinal, null);

				graph.ApplyWorkflowState(e.NewRow);

				var state = sender.GetStateExt(e.NewRow, _FieldName) as PXStringState;
				sender.SetValue(e.NewRow, _FieldOrdinal, currentValue);
				if (state != null)
				{
					var allowedValues = state.AllowedValues;
					if (allowedValues != null && (string.IsNullOrEmpty(currentValue as string) || Array.IndexOf(allowedValues, (string)currentValue) < 0))
						sender.SetValue(e.NewRow, _FieldOrdinal, allowedValues.FirstOrDefault());
				}
				else if (sender.GetValue(e.NewRow, _FieldOrdinal) == sender.GetValue(e.Row, _FieldOrdinal))
				{
					sender.SetValue(e.NewRow, _FieldOrdinal, null);
				}
			}
		}
	}
	#endregion

	#region CRValidateDateAttribute
	public sealed class CRValidateDateAttribute : PXDBDateAndTimeAttribute
	{
		public static readonly DateTime DefaultDate = new DateTime(1900, 1, 1);
		public class defaultDate : PX.Data.BQL.BqlDateTime.Constant<defaultDate>
		{
			public defaultDate() : base(DefaultDate) { }
		}
		public override void CacheAttached(PXCache sender)
		{
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), (cache, e) =>
			{
				if (cache.GetValue(e.Row, _FieldName) == null)
					cache.SetValue(e.Row, _FieldName, DefaultDate);
			});
			base.CacheAttached(sender);
		}
	}
	#endregion

	#region Minutes
	[Obsolete]
	public sealed class Minutes<Operand> : IBqlOperand, IBqlCreator
	where Operand : IBqlOperand
	{
		
		private IBqlCreator _operand;

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;
			if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
			{
				info.Fields?.Add(typeof(Operand));
				return status;
			}

			if (_operand == null) _operand = Activator.CreateInstance<Operand>() as IBqlCreator;
			if (_operand == null) {
				throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
			}

			status &= _operand.AppendExpression(ref exp, graph, info, selection);
			return status;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			BqlFunction.getValue<Operand>(ref _operand, cache, item, pars, ref result, out value);
			if (value != null)
			{
				value = TimeSpan.FromMinutes(Convert.ToDouble(value));
			}
		}
	}

	#endregion
	#region Minutes
	public sealed class Round30Minutes<Operand> : IBqlOperand, IBqlCreator
	where Operand : IBqlOperand
	{
		private IBqlCreator _operand;

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) 
		{
			bool status = true;
			if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
			{
				info.Fields?.Add(typeof(Operand));
				return true;
			}

			if (_operand == null) _operand = Activator.CreateInstance<Operand>() as IBqlCreator;
			if (_operand == null) {
				throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
			}

			_operand.AppendExpression(ref exp, graph, info, selection);
			return status;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			BqlFunction.getValue<Operand>(ref _operand, cache, item, pars, ref result, out value);
			if (value != null)
			{				
				DateTime date = Convert.ToDateTime(value);
				var minutes = date.Minute;
				if (minutes != 0)
				{
					minutes = minutes <= 30 ? 30 : 60;
				}
				value = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).AddMinutes(minutes);
			}
		}
	}

	#endregion

	#region PXViewSavedDetailsButtonAttribute

	public class PXViewSavedDetailsButtonAttribute : PX.SM.PXViewDetailsButtonAttribute
	{		
		public PXViewSavedDetailsButtonAttribute(Type primaryType):base(primaryType)
		{		
		}

		public PXViewSavedDetailsButtonAttribute(Type primaryType, Type select)
			:base(primaryType , select)
		{			
		}

		protected override void Redirect(PXAdapter adapter, object record)
		{
			var status = adapter.View.Graph.Caches[record.GetType()].GetStatus(record);
			if (status == PXEntryStatus.Deleted || status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated) return;

			base.Redirect(adapter, record);
		}
	}

	#endregion

    #region CRTaskSelectorAttribute

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CRTaskSelectorAttribute : PXSelectorAttribute
    {
        public CRTaskSelectorAttribute()
            :base(typeof(Search<CRActivity.noteID, 
				Where<CRActivity.classID, Equal<CRActivityClass.task>,
					Or<CRActivity.classID, Equal<CRActivityClass.events>>>, OrderBy<Desc<CRActivity.createdDateTime>>>),
                 typeof(CRActivity.subject),
                 typeof(CRActivity.priority),
                 typeof(CRActivity.startDate),
                 typeof(CRActivity.endDate))
        {
            this.DescriptionField = typeof(CRActivity.subject);
        }
    }

	#endregion

	#region OUEntityIDSelectorAttribute

	[Obsolete]
    public class OUEntityIDSelectorAttribute : PXEntityIDSelectorAttribute
	{
		private readonly Type contactBqlField;
		private readonly Type baccountBqlField;

		[Obsolete]
      public OUEntityIDSelectorAttribute(Type typeBqlField, Type contactBqlField, Type baccountBqlField)
			:base(typeBqlField)
		{
			this.contactBqlField = contactBqlField;
	      this.baccountBqlField = baccountBqlField;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			PXFieldState state = e.ReturnState as PXFieldState;			
			if(state != null)
				state.DescriptionName = "Description";			
		}

		protected override void CreateSelectorView(PXGraph graph, Type itemType, PXNoteAttribute noteAtt, out string viewName, out string[] fieldList, out string[] headerList)
		{
			Type search = null;
			if (itemType == typeof (CRCase))
				search =
					BqlCommand.Compose(typeof (Search<,>), typeof (CRCase.caseCD),
						typeof (Where<,,>), typeof (CRCase.contactID), typeof (Equal<>), typeof (Current<>), contactBqlField,
						typeof (Or<,>), typeof (CRCase.customerID), typeof (Equal<>), typeof (Current<>), baccountBqlField);

			if (itemType == typeof(CROpportunity))
				search =
					BqlCommand.Compose(typeof(Search<,>), typeof(CROpportunity.opportunityID), 
					typeof(Where<,,>), typeof(CROpportunity.contactID), typeof(Equal<>), typeof(Current<>), contactBqlField,
						typeof(Or<,>), typeof(CROpportunity.bAccountID), typeof(Equal<>), typeof(Current<>), baccountBqlField);

			if (search != null)
			{
				viewName = AddSelectorView(graph, search);
				PXFieldState state = AddFieldView(graph, search.GenericTypeArguments[0]);
				fieldList = state.FieldList;
				headerList = state.HeaderList;
			}
			else
				base.CreateSelectorView(graph, itemType, noteAtt, out viewName, out fieldList, out headerList);
		}

	}

	#endregion

    #region CampaignAttribute

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SendFilterSourcesAttribute : PXStringListAttribute
    {
        public const string _ALL = "A";
        public const string _NEVERSENT = "N";
       
        public SendFilterSourcesAttribute()
            : base(new[] { _ALL, _NEVERSENT, },
                    new[] { Messages.All, Messages.NeverSent })
        { }

        public sealed class all : PX.Data.BQL.BqlString.Constant<all>
		{
            public all() : base(_ALL) { }
        }

        public sealed class neverSent : PX.Data.BQL.BqlString.Constant<neverSent>
		{
            public neverSent() : base(_NEVERSENT) { }
        } 
    }

    #endregion

	#region PXQuoteProjectionAttribute

	public class PXQuoteProjectionAttribute : PXProjectionAttribute
	{
		public PXQuoteProjectionAttribute(Type select)		
			:base(select, new Type[] { typeof(Standalone.CRQuote), typeof(Standalone.CROpportunityRevision) })
		{	
					
		}
    }
	#endregion

	#region CRPopupFilter

	/// <exclude/>
	[Obsolete("Use CRValidationFilter instead.")]
	public class CRPopupFilter<Table> : PXFilter<Table>
		where Table : class, IBqlTable, new()
	{
		#region Ctor

		public CRPopupFilter(PXGraph graph)
			: base(graph) { }

		public CRPopupFilter(PXGraph graph, Delegate handler)
			: base(graph, handler) { }

		#endregion

		public override bool VerifyRequired(bool suppressError)
		{
			bool result = true;

			foreach (string field in Cache.Fields)
			{
				object newValue = Cache.GetValue(Cache.Current, field);
				try
				{
					Cache.RaiseFieldVerifying(field, Cache.Current, ref newValue);
				}
				catch (PXException exc)
				{
					if (!suppressError)
					{
						Cache.RaiseExceptionHandling(field, Cache.Current, newValue, exc);
					}
					result = false;
				}
			}
			return result;
		}
	}

	#endregion

	#region CRPopupValidator

	/// <exclude/>
	public class CRPopupValidator : ICRValidationFilter, ICRPreserveCachedRecordsFilter
	{
		private readonly ICRValidationFilter[] _filters;
		private CRPopupValidator(ICRValidationFilter[] filters)
		{
			_filters = filters ?? throw new ArgumentNullException(nameof(filters));
		}

		public void Validate()
		{
			var exs = new List<PXOuterException>(_filters.Length);
			foreach (var filter in _filters)
			{
				try
				{
					filter.Validate();
				}
				catch (PXOuterException e)
				{
					exs.Add(e);
				}
			}
			if (exs.Count > 0)
			{
				// can create outer exception only for same graph and row.
				var gr = exs.GroupBy(e => (type: e.GraphType, row: e.Row)).First();

				void FillDictionary(Dictionary<string, string> dict_, PXOuterException ex_)
				{
					var keys = ex_.InnerFields;
					var value = ex_.InnerMessages;
					for (int i = 0; i < keys.Length; i++)
					{
						// rewrite to avoid argument exceptions
						dict_[keys[i]] = value[i];
					}
				}
				var dict = new Dictionary<string, string>(17);

				foreach (var ex in gr)
				{
					FillDictionary(dict, ex);
				}

				throw new PXOuterException(dict, gr.Key.type, gr.Key.row, gr.First().Message);
			}
		}

		public void Validate(params CRPopupValidator[] additionalValidators)
		{
			if(additionalValidators == null)
			{
				Validate();
				return;
			}

			PXOuterException firstEx = null;
			try
			{
				Validate();
			}
			catch(PXOuterException ex)
			{
				firstEx = ex;
			}
			foreach (var validator in additionalValidators)
			{
				try
				{
					validator.Validate();
				}
				catch (PXOuterException ex)
				{
					firstEx = firstEx ?? ex;
				}
			}
			if (firstEx != null)
				throw firstEx;
		}

		public bool TryValidate()
		{
			bool result = true;
			foreach (var filter in _filters)
			{
				// validate all and then return, to normally show in the ui
				result &= filter.TryValidate();
			}
			return result;
		}

		public bool TryValidate(params CRPopupValidator[] additionalValidators)
		{
			bool result = TryValidate();
			if (additionalValidators == null)
				return result;

			foreach (var validator in additionalValidators)
			{
				result &= validator.TryValidate();
			}
			return result;
		}

		public void Reset()
		{
			foreach (var filter in _filters)
			{
				filter.Reset();
			}
		}

		public void Reset(params CRPopupValidator[] additionalValidators)
		{
			Reset();

			if (additionalValidators == null)
				return;

			foreach (var validator in additionalValidators)
			{
				validator.Reset();
			}
		}

		public IDisposable PreserveCachedRecords()
		{
			var disposable = new CompositeDisposable(_filters.Length);
			foreach (var preserve in _filters.OfType<ICRPreserveCachedRecordsFilter>())
			{
				disposable.Add(preserve.PreserveCachedRecords());
			}

			return disposable;
		}

		public static Generic<TTable> Create<TTable>(CRValidationFilter<TTable> filter, params ICRValidationFilter[] filters) where TTable : class, IBqlTable, new()
		{
			return new Generic<TTable>(filter, filters);
		}

		public class Generic<TTable> : CRPopupValidator where TTable : class, IBqlTable, new()
		{
			public Generic(CRValidationFilter<TTable> filter, params ICRValidationFilter[] filters)
				: base((filters ?? Array.Empty<ICRValidationFilter>()).Prepend(filter).ToArray())
			{
				Filter = filter ?? throw new ArgumentNullException(nameof(filter));
			}

			public CRValidationFilter<TTable> Filter { get; }

			[Obsolete("Use AskExt() && TryValidate() instead")]
			public WebDialogResult? AskExtValidation(PXView.InitializePanel initializeHandler = null, bool reset = true, IEnumerable<CRPopupValidator> additionalValidators = null)
			{
				WebDialogResult? result = AskExt(initializeHandler, reset);
				return TryValidate(additionalValidators?.ToArray()) ? result : null;
			}

			public WebDialogResult AskExt(PXView.InitializePanel initializeHandler = null, bool reset = true)
			{
				// to avoid circular reference
				void Initializer(PXGraph g, string v)
				{
					if (reset) Reset();
					initializeHandler?.Invoke(g, v);
				}

				return Filter.AskExt(Initializer, reset);
			}
		}

	}

	/// <exclude/>
	public interface ICRValidationFilter
	{
		bool TryValidate();
		/// <summary>
		/// Validate Filter and throw <see cref="PXOuterException"/> if validation failed.
		/// </summary>
		/// <exception cref="PXOuterException"></exception>
		void Validate();
		void Reset();
	}

	public interface ICRPreserveCachedRecordsFilter
	{
		IDisposable PreserveCachedRecords();
	}

	/// <exclude/>
	public sealed class CRValidationFilter<TTable> : PXFilter<TTable>, ICRValidationFilter, ICRPreserveCachedRecordsFilter
		where TTable : class, IBqlTable, new()
	{
		public CRValidationFilter(PXGraph graph) : base(graph)
		{
		}

		public CRValidationFilter(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		public override bool VerifyRequired(bool suppressError)
		{
			return TryValidate();
		}

		public bool TryValidate()
		{
			return VerifyRequiredFields() & base.VerifyRequired(false);
		}

		public void Validate()
		{
			bool result = TryValidate();
			if (!result)
			{
				// could not work correctly if VerifyRequiredFields catch exception for not first entity
				// maybe should consider that
				TTable entity = Select().First();
				throw new PXOuterException(
					PXUIFieldAttribute.GetErrors(Cache, entity),
					Cache.Graph.GetType(),
					entity,
					ErrorMessages.RecordRaisedErrors, "Validating", entity.GetType().Name);
			}
		}

		public IDisposable PreserveCachedRecords()
		{
			var current = Current;
			var cached  = Cache.Cached.OfType<object>().Select(i => (i, Cache.GetStatus(i))).ToList();
			return Disposable.Create(() =>
			{
				Current = current;
				foreach (var (item, status) in cached)
				{
					Cache.SetStatus(item, status);
				}
			});
		}

		private bool VerifyRequiredFields()
		{
			bool result = true;
			foreach (TTable entity in Select())
			{
				foreach (string field in Cache.Fields)
				{
					object newValue = Cache.GetValue(entity, field);
					try
					{
						Cache.RaiseFieldVerifying(field, entity, ref newValue);
					}
					catch (PXException exc)
					{
						Cache.RaiseExceptionHandling(field, entity, newValue, exc);
						result = false;
					}
					// exception could be added without exceptions, so need to recheck it
					if (!string.IsNullOrEmpty(PXUIFieldAttribute.GetErrorOnly(Cache, entity, field)))
						result = false;
				}
			}
			return result;
		}
	}

	#endregion

	#region RestrictOrganizationAttribute
	[PXDBInt]
	[PXUIField()]
	public class RestrictOrganizationAttribute : GL.Attributes.BaseOrganizationTreeAttribute, IPXRowPersistingSubscriber
	{
		public RestrictOrganizationAttribute()
			: base(null, true, false)
		{

		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{	
				if (Required && (int?)sender.GetValue(e.Row, _FieldOrdinal) == 0)
				{
					e.Cancel = true;

					throw new PXRowPersistingException(FieldName, null, Messages.EmptyRestrictVisibilityTo);
				}
			}
		}
	}
	#endregion

	#region PXConstant

	public class PXConstant : PXStringAttribute, IPXRowSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		private readonly string _constant;

		public PXConstant(string constant)
		{
			_constant = constant;
		}

		#region Implementation

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (string key in sender.Keys)
			{
				if (sender.GetValue(e.Row, key) == null)
				{
					return;
				}
			}

			sender.SetValue(e.Row, _FieldOrdinal, _constant);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _constant;
		}

		#endregion
	}

	#endregion

	#region LeadLastNameOrCompanyNameRequiredAttribute

	public class LeadLastNameOrCompanyNameRequiredAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public LeadLastNameOrCompanyNameRequiredAttribute()
		{
		}
		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRLead;
			if (row == null) return;
			if (string.IsNullOrEmpty(row.LastName) && string.IsNullOrEmpty(row.FullName))
				throw new PXSetPropertyException(Messages.LastNameOrFullNameReqired);
		}
	}

	#endregion
}
