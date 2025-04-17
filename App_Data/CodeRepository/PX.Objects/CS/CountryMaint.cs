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
using PX.Data;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace PX.Objects.CS
{
	public class CountryMaint : PXGraph<CountryMaint, Country>
	{
		public PXSelect<Country> Country;
		public PXSelect<State, Where<State.countryID, Equal<Current<Country.countryID>>>, OrderBy<Asc<State.stateID>>> CountryStates;

		public CountryMaint()
		{
			PXUIFieldAttribute.SetVisible<Country.languageID>(Country.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Country_CountryID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void Country_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				Country country = e.Row as Country;
				if (string.IsNullOrEmpty(country.Description) || string.IsNullOrEmpty(country.Description.Trim()))
				{
					cache.RaiseExceptionHandling("Description", e.Row, country.Description, new PXSetPropertyException(Messages.CountryNameEmptyError));
				}
				if (string.IsNullOrEmpty(country.CountryID) || string.IsNullOrEmpty(country.CountryID.Trim()))
				{
					cache.RaiseExceptionHandling("CountryID", e.Row, country.CountryID, new PXSetPropertyException(Messages.CountryIDEmptyError));
				}
			}	
		}

		protected virtual void Country_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
				PXZipValidationAttribute.Clear<Country>();
		}

		private bool IsKeyExists(State aState) 
		{
			foreach (State state in this.CountryStates.Select())
			{
				if (state.StateID == aState.StateID ) return true;
			}
			return false;
		}

		
		protected virtual void Country_CountryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void State_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			State state = e.Row as State;
			ValidateState(state);
		}

		protected virtual void State_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			State state = e.Row as State;
			if (!ValidateState(state)) return;
			if (IsKeyExists(state))
			{
				throw new  PXException(Messages.StateIDMustBeUnique);
			}

			/*if (string.IsNullOrEmpty(state.Name) || string.IsNullOrEmpty(state.Name.Trim()))
			{
				e.Cancel = true;
				throw new Exception("State Name can't be empty");
			}*/
		}
		private bool ValidateState(State state) 
		{
			if (string.IsNullOrEmpty(state.StateID) || string.IsNullOrEmpty(state.StateID.Trim()))
			{
				throw new PXException(Messages.StateIDCantBeEmpty);
			}

			/*if (string.IsNullOrEmpty(state.Name) || string.IsNullOrEmpty(state.Name.Trim()))
			{
				throw new PXException(Messages.StateNameCantBeEmpty);
			}*/
			return true;
		}
	}
}
