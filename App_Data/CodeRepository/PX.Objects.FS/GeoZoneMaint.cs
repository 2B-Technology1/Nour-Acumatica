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

namespace PX.Objects.FS
{
    public class GeoZoneMaint : PXGraph<GeoZoneMaint, FSGeoZone>
    {
        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZone> GeoZoneRecords;

        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZoneEmp,
               Where<
                   FSGeoZoneEmp.geoZoneID, Equal<Current<FSGeoZone.geoZoneID>>>> GeoZoneEmpRecords;

        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZonePostalCode,
               Where<
                   FSGeoZonePostalCode.geoZoneID, Equal<Current<FSGeoZone.geoZoneID>>>> GeoZonePostalCodeRecords;

        #region Virtual Methods

        public virtual void EnableDisableGeoZonePostalCode(PXCache cache, FSGeoZonePostalCode fsGeoZonePostalCodeRow)
        {
            PXUIFieldAttribute.SetEnabled<FSGeoZonePostalCode.postalCode>
                    (cache, fsGeoZonePostalCodeRow, string.IsNullOrEmpty(fsGeoZonePostalCodeRow.PostalCode));
        }

        public virtual void EnableDisableGeoZoneEmployee(PXCache cache, FSGeoZoneEmp fsGeoZoneEmpRow)
        {
            PXUIFieldAttribute.SetEnabled<FSGeoZoneEmp.employeeID>
                    (cache, fsGeoZoneEmpRow, fsGeoZoneEmpRow.EmployeeID == null);
        }

        #endregion

        #region Event Handlers

        #region FSGeoZoneEmp
        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSGeoZoneEmp> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EnableDisableGeoZoneEmployee(e.Cache, (FSGeoZoneEmp)e.Row);
        }

        protected virtual void _(Events.RowInserting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSGeoZoneEmp> e)
        {
        }

        #endregion
        #region FSGeoZonePostalCode

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSGeoZonePostalCode> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EnableDisableGeoZonePostalCode(e.Cache, (FSGeoZonePostalCode)e.Row);
        }

        protected virtual void _(Events.RowInserting<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSGeoZonePostalCode> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSGeoZonePostalCode> e)
        {
        }

        #endregion

        #endregion
    }
}
