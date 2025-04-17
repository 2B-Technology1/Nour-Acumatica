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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.SO.WMS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ShipmentAuditSupport : PXGraphExtension<PickPackShip.Host>
	{
		public PXSelect<SOShipmentUpdate> updShipment;

		public virtual Type[] GetChildTypes() => new[]
		{
			typeof(SOShipLineSplit),
			typeof(SOShipLine),
			typeof(SOPackageDetailEx)
		};

		/// Overrides <see cref="SOShipmentEntry.Persist"/>
		[PXOverride]
		public virtual void Persist(Action baseImpl)
		{
			SOShipmentUpdate shipmentUpdate = null;

			if (!Base.Document.Cache.IsInsertedUpdatedDeleted)
			{
				var shipment = FindModifiedShipment();

				if (shipment != null)
				{
					updShipment.Insert(shipmentUpdate = new SOShipmentUpdate
					{
						ShipmentType = shipment.ShipmentType,
						ShipmentNbr = shipment.ShipmentNbr
					});
				}
			}
			try
			{
				baseImpl();
			}
			catch
			{
				updShipment.Cache.Clear();
				throw;
			}

			if (shipmentUpdate != null)
			{
				Base.Document.Cache.Clear();
				Base.Document.Cache.ClearQueryCacheObsolete();

				Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shipmentUpdate.ShipmentNbr);
			}
		}

		public virtual SOShipment FindModifiedShipment()
		{
			var childTypes = GetChildTypes();
			if (childTypes == null || childTypes.Length == 0)
				return null;

			object findModifiedChild(PXCache cache)
				=> cache.Inserted
				.Concat_(cache.Updated)
				.Concat_(cache.Deleted)
				.FirstOrDefault_();

			SOShipment findModifiedParent(Type childType)
			{
				var childCache = Base.Caches[childType];
				if (!childCache.IsInsertedUpdatedDeleted)
					return null;
				var childRow = findModifiedChild(childCache);
				if (childRow == null)
					return null;
				return PXParentAttribute.SelectParent<SOShipment>(childCache, childRow);
			}

			return childTypes
				.Select(findModifiedParent)
				.FirstOrDefault(s => s != null);
		}

		[SOShipmentUpdate.Accumulator(BqlTable = typeof(SOShipment), SingleRecord = true)]
		[PXHidden]
		public class SOShipmentUpdate : IBqlTable
		{
			#region ShipmentType
			[PXDBString(1, IsKey = true)]
			public virtual string ShipmentType { get; set; }
			public abstract class shipmentType : BqlString.Field<shipmentType> { }
			#endregion

			#region ShipmentNbr
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			public virtual string ShipmentNbr { get; set; }
			public abstract class shipmentNbr : BqlString.Field<shipmentNbr> { }
			#endregion

			#region LastModifiedByID
			[PXDBLastModifiedByID]
			public virtual Guid? LastModifiedByID { get; set; }
			public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
			#endregion
			#region LastModifiedByScreenID
			[PXDBLastModifiedByScreenID]
			public virtual string LastModifiedByScreenID { get; set; }
			public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
			#endregion
			#region LastModifiedDateTime
			[PXDBLastModifiedDateTime]
			public virtual DateTime? LastModifiedDateTime { get; set; }
			public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
			#endregion

			public class AccumulatorAttribute : PXAccumulatorAttribute
			{
				protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
				{
					if (!base.PrepareInsert(sender, row, columns))
						return false;

					var detail = (SOShipmentUpdate)row;
					columns.UpdateOnly = true;
					columns.Update<lastModifiedByID>(detail.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
					columns.Update<lastModifiedByScreenID>(detail.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);
					columns.Update<lastModifiedDateTime>(detail.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);

					return true;
				}
			}
		}
	}
}
