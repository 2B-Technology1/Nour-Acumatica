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
using System.Text;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;

#if false
namespace PX.Objects.CS
{
	public class CompanyLocationMaint : LocationMaintBase<SelectedLocation, SelectedLocation, Where<SelectedLocation.bAccountID, Equal<Current<Company.bAccountID>>>>
    {
		public PXSelect<Building, Where<Building.locationID, Equal<Current<SelectedLocation.locationID>>>> building;
		public PXSetup<Branch> company;

        #region Buttons

		public PXSave<SelectedLocation> Save;
		public PXAction<SelectedLocation> cancel;
		public PXInsert<SelectedLocation> Insert;
		public PXDelete<SelectedLocation> Delete;
		public PXFirst<SelectedLocation> First;
		public PXAction<SelectedLocation> previous;
		public PXAction<SelectedLocation> next;
		public PXLast<SelectedLocation> Last;
		public PXAction<SelectedLocation> viewOnMap;

        #endregion

        #region ButtonDelegates

        [PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
			int? acctid = company.Current != null ? company.Current.BAccountID : null;
			foreach (SelectedLocation loc in (new PXCancel<SelectedLocation>(this, "Cancel")).Press(adapter))
            {
                if (Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted
                        && (acctid != loc.BAccountID || string.IsNullOrEmpty(loc.LocationCD)))
                {
					foreach (SelectedLocation first in First.Press(adapter))
                    {
                        return new object[] { first };
                    }
                    loc.LocationCD = null;
                    return new object[] { loc };
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        [PXUIField(DisplayName = "Prev", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXPreviousButton]
        protected virtual IEnumerable Previous(PXAdapter adapter)
        {
			foreach (SelectedLocation loc in (new PXPrevious<SelectedLocation>(this, "Prev")).Press(adapter))
            {
                if (Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return Last.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        [PXUIField(DisplayName = "Next", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXNextButton]
        protected virtual IEnumerable Next(PXAdapter adapter)
        {
			foreach (SelectedLocation loc in (new PXNext<SelectedLocation>(this, "Next")).Press(adapter))
            {
                if (Location.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return First.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}
        #endregion

		public CompanyLocationMaint()
		{
			if (company.Current.BAccountID.HasValue == false)
			{
				throw new PXSetupNotEnteredExeption(ErrorMessages.SetupNotEntered, typeof(Branch),  typeof(CS.BranchMaint), CS.Messages.BranchMaint);
			}
		}

		
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(Company.bAccountID))]
        [PXUIField(DisplayName = "Business Account ID", TabOrder = 0)]
		protected virtual void SelectedLocation_BAccountID_CacheAttached(PXCache sender)
		{

		}

        [PXBool()]
        [PXUIField(DisplayName = "Same As Company's")]
		protected virtual void SelectedLocation_IsAddressSameAsMain_CacheAttached(PXCache sender)
        {
        	
        }

        [PXBool]
        [PXUIField(DisplayName = "Same As Company's")]
		protected virtual void SelectedLocation_IsContactSameAsMain_CacheAttached(PXCache sender)
        {
        	
        }
    }
}
#endif