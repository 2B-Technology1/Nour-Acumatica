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
using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.EndpointAdapters.WorkflowAdapters.IN
{
	internal abstract class InventoryRegisterAdapterBase
	{
		protected void INRegisterInsert(INRegisterEntryBase registerEntry, EntityImpl entity, EntityImpl targetEntity)
		{
			bool isNew = true;

			var nbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ReferenceNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			INRegister register = nbrField?.Value != null
				? registerEntry.INRegisterDataMember.Search<INRegister.refNbr>(nbrField.Value)
				: null;

			if (register == null)
			{
				register = (INRegister)registerEntry.INRegisterDataMember.Cache.CreateInstance();

				if (nbrField != null)
					register.RefNbr = nbrField.Value;
			}
			else
				isNew = false;

			registerEntry.INRegisterDataMember.Current = isNew
				? registerEntry.INRegisterDataMember.Insert(register)
				: registerEntry.INRegisterDataMember.Update(register);

			registerEntry.SubscribeToPersistDependingOnBoolField(holdField, registerEntry.putOnHold, registerEntry.releaseFromHold);
		}

		protected void INRegisterUpdate(INRegisterEntryBase registerEntry, EntityImpl entity, EntityImpl targetEntity)
		{
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			registerEntry.SubscribeToPersistDependingOnBoolField(holdField, registerEntry.putOnHold, registerEntry.releaseFromHold);
		}
	}
}