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

// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
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
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using PX.Data.SQLTree;

namespace PX.Data
{
	/// <summary>
	/// This is a generic attribute that aggregates other attributes and exposes some of their public properties.
	/// The attribute is usually attached to a field that stores a reference to some entity.
	/// </summary>
	/// <remarks>
	/// Aggregated attributes can be of the following types:
	/// <list type="bullet">
	/// <item><description><see cref="PXDBFieldAttribute"/>, such as <see cref="PXDBIntAttribute"/>, <see cref="PXDBStringAttribute"/>, and their unbound analogs;</description></item>
	/// <item><description><see cref="PXUIFieldAttribute"/>;</description></item>
	/// <item><description><see cref="PXSelectorAttribute"/> or <see cref="PXDimensionSelectorAttribute"/>;</description></item>
	/// <item><description><see cref="PXDefaultAttribute"/>.</description></item>
	/// </list>
	/// </remarks>
	public abstract class PXEntityAttribute : PXAggregateAttribute, IPXInterfaceField, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
	{
		#region Initialization
		protected PXEntityAttribute()
		{
			Initialize();
			Filterable = true;
		}

		protected virtual void Initialize()
		{
			_DBAttrIndex = -1;
			_NonDBAttrIndex = -1;
			_UIAttrIndex = -1;
			_SelAttrIndex = -1;
			_DefAttrIndex = -1;

			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXDBFieldAttribute)
				{
					_DBAttrIndex = _Attributes.IndexOf(attr);
					foreach (PXEventSubscriberAttribute sibling in _Attributes)
					{
						if (!object.ReferenceEquals(attr, sibling) && PXAttributeFamilyAttribute.IsSameFamily(attr.GetType(), sibling.GetType()))
						{
							_NonDBAttrIndex = _Attributes.IndexOf(sibling);
							break;
						}
					}
				}

				if (attr is PXUIFieldAttribute)
					_UIAttrIndex = _Attributes.IndexOf(attr);

				if (attr is PXDimensionSelectorAttribute)
					_SelAttrIndex = _Attributes.IndexOf(attr);

				if (attr is PXSelectorAttribute && _SelAttrIndex < 0)
					_SelAttrIndex = _Attributes.IndexOf(attr);

				if (attr is PXDefaultAttribute)
					_DefAttrIndex = _Attributes.IndexOf(attr);
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXCommandPreparingSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
			{
				if (IsDBField == false)
				{
					if (NonDBAttribute == null)
						subscribers.Add(this as ISubscriber);
					else if (typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
						subscribers.Add(this as ISubscriber);
					else
						NonDBAttribute.GetSubscriber(subscribers);

					for (int i = 0; i < _Attributes.Count; i++)
						if (i != _DBAttrIndex && i != _NonDBAttrIndex)
							_Attributes[i].GetSubscriber(subscribers);
				}
				else
				{
					base.GetSubscriber(subscribers);

					if (NonDBAttribute != null)
						subscribers.Remove(NonDBAttribute as ISubscriber);

					subscribers.Remove(this as ISubscriber);
				}
			}
			else
			{
				base.GetSubscriber(subscribers);

				if (NonDBAttribute != null)
					subscribers.Remove(NonDBAttribute as ISubscriber);
			}
		}
		#endregion

		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.Expr = new SQLConst(string.Empty);
			e.Cancel = true;
		}

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (IsDBField)
				sender.SetValue(e.Row, _FieldOrdinal, null);
		}
		#endregion

		#region DBAttribute delagation
		public bool IsDBField { get; set; } = true;

		protected int _DBAttrIndex = -1;
		protected PXDBFieldAttribute DBAttribute => _DBAttrIndex == -1 ? null : (PXDBFieldAttribute)_Attributes[_DBAttrIndex];

		protected int _NonDBAttrIndex = -1;
		protected PXEventSubscriberAttribute NonDBAttribute => _NonDBAttrIndex == -1 ? null : _Attributes[_NonDBAttrIndex];


		/// <inheritdoc cref="PXEventSubscriberAttribute.FieldName"/>
		public new string FieldName
		{
			get => DBAttribute?.FieldName;
			set => DBAttribute.FieldName = value;
		}

		/// <inheritdoc cref="PXDBFieldAttribute.IsKey"/>
		public bool IsKey
		{
			get => DBAttribute?.IsKey ?? false;
			set => DBAttribute.IsKey = value;
		}

		/// <inheritdoc cref="PXDBStringAttribute.IsFixed"/>
		public bool IsFixed
		{
			get => ((PXDBStringAttribute)DBAttribute)?.IsFixed ?? false;
			set
			{
				((PXDBStringAttribute)DBAttribute).IsFixed = value;
				if (NonDBAttribute != null)
					((PXStringAttribute)NonDBAttribute).IsFixed = value;
			}
		}

		/// <inheritdoc cref="PXDBFieldAttribute.BqlField"/>
		public Type BqlField
		{
			get => DBAttribute?.BqlField;
			set
			{
				DBAttribute.BqlField = value;
				BqlTable = DBAttribute.BqlTable;
			}
		}
		#endregion

		#region UIAttribute delagation
		protected int _UIAttrIndex = -1;
		protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute)_Attributes[_UIAttrIndex];

		/// <inheritdoc cref="PXUIFieldAttribute.Visibility"/>
		public PXUIVisibility Visibility
		{
			get => UIAttribute?.Visibility ?? PXUIVisibility.Undefined;
			set
			{
				if (UIAttribute != null)
					UIAttribute.Visibility = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.Visible"/>
		public bool Visible
		{
			get => UIAttribute?.Visible ?? true;
			set
			{
				if (UIAttribute != null)
					UIAttribute.Visible = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.Enabled"/>
		public bool Enabled
		{
			get => UIAttribute?.Enabled ?? true;
			set
			{
				if (UIAttribute != null)
					UIAttribute.Enabled = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.DisplayName"/>
		public string DisplayName
		{
			get => UIAttribute?.DisplayName;
			set
			{
				if (UIAttribute != null)
					UIAttribute.DisplayName = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.FieldClass"/>
		public string FieldClass
		{
			get => UIAttribute?.FieldClass;
			set
			{
				if (UIAttribute != null)
					UIAttribute.FieldClass = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.Required"/>
		public bool Required
		{
			get => UIAttribute?.Required ?? false;
			set
			{
				if (UIAttribute != null)
					UIAttribute.Required = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.TabOrder"/>
		public virtual int TabOrder
		{
			get => UIAttribute?.TabOrder ?? _FieldOrdinal;
			set
			{
				if (UIAttribute != null)
					UIAttribute.TabOrder = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.ErrorHandling"/>
		public virtual PXErrorHandling ErrorHandling
		{
			get => UIAttribute?.ErrorHandling ?? PXErrorHandling.WhenVisible;
			set
			{
				if (UIAttribute != null)
					UIAttribute.ErrorHandling = value;
			}
		}

		#region IPXInterfaceField Members
		private IPXInterfaceField PXInterfaceField => UIAttribute;

		/// <inheritdoc cref="IPXInterfaceField.ErrorText"/>
		public string ErrorText
		{
			get => PXInterfaceField?.ErrorText;
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorText = value;
			}
		}

		/// <inheritdoc cref="IPXInterfaceField.ErrorValue"/>
		public object ErrorValue
		{
			get => PXInterfaceField?.ErrorValue;
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorValue = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.ErrorLevel"/>
		[PX.Common.PXInternalUseOnly]
		public PXErrorLevel ErrorLevel
		{
			get => PXInterfaceField?.ErrorLevel ?? PXErrorLevel.Undefined;
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorLevel = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.MapEnableRights"/>
		public PXCacheRights MapEnableRights
		{
			get => PXInterfaceField?.MapEnableRights ?? PXCacheRights.Select;
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.MapEnableRights = value;
			}
		}

		/// <inheritdoc cref="PXUIFieldAttribute.MapViewRights"/>
		public PXCacheRights MapViewRights
		{
			get => PXInterfaceField?.MapViewRights ?? PXCacheRights.Select;
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.MapViewRights = value;
			}
		}

		/// <inheritdoc cref="IPXInterfaceField.ViewRights"/>
		public bool ViewRights => PXInterfaceField?.ViewRights ?? true;

		/// <inheritdoc cref="IPXInterfaceField.ForceEnabled"/>
		public void ForceEnabled() => PXInterfaceField?.ForceEnabled();
		#endregion
		#endregion

		#region SelectorAttribute delagation
		protected int _SelAttrIndex = -1;
		protected PXSelectorAttribute NonDimensionSelectorAttribute => _SelAttrIndex == -1 ? null : _Attributes[_SelAttrIndex] as PXSelectorAttribute;
		protected PXDimensionSelectorAttribute SelectorAttribute => _SelAttrIndex == -1 ? null : _Attributes[_SelAttrIndex] as PXDimensionSelectorAttribute;

		/// <inheritdoc cref="PXSelectorAttribute.DescriptionField"/>
		public virtual Type DescriptionField
		{
			get => NonDimensionSelectorAttribute?.DescriptionField ?? SelectorAttribute?.DescriptionField;
			set
			{
				if (NonDimensionSelectorAttribute != null)
					NonDimensionSelectorAttribute.DescriptionField = value;
				if (SelectorAttribute != null)
					SelectorAttribute.DescriptionField = value;
			}
		}

		/// <inheritdoc cref="PXSelectorAttribute.DirtyRead"/>
		public virtual bool DirtyRead
		{
			get => NonDimensionSelectorAttribute?.DirtyRead ?? SelectorAttribute?.DirtyRead ?? false;
			set
			{
				if (NonDimensionSelectorAttribute != null)
					NonDimensionSelectorAttribute.DirtyRead = value;
				if (SelectorAttribute != null)
					SelectorAttribute.DirtyRead = value;
			}
		}

		/// <inheritdoc cref="PXSelectorAttribute.CacheGlobal"/>
		public virtual bool CacheGlobal
		{
			get => NonDimensionSelectorAttribute?.CacheGlobal ?? SelectorAttribute?.CacheGlobal ?? false;
			set
			{
				if (NonDimensionSelectorAttribute != null)
					NonDimensionSelectorAttribute.CacheGlobal = value;
				if (SelectorAttribute != null)
					SelectorAttribute.CacheGlobal = value;
			}
		}

		/// <inheritdoc cref="PXSelectorAttribute.ValidateValue"/>
		public virtual bool ValidateValue
		{
			get => NonDimensionSelectorAttribute?.ValidateValue ?? SelectorAttribute?.ValidateValue ?? false;
			set
			{
				if (NonDimensionSelectorAttribute != null)
					NonDimensionSelectorAttribute.ValidateValue = value;
				if (SelectorAttribute != null)
					SelectorAttribute.ValidateValue = value;
			}
		}

		/// <inheritdoc cref="PXSelectorAttribute.Filterable"/>
		public virtual bool Filterable
		{
			get => NonDimensionSelectorAttribute?.Filterable ?? SelectorAttribute?.Filterable ?? false;
			set
			{
				if (NonDimensionSelectorAttribute != null)
					NonDimensionSelectorAttribute.Filterable = value;
				if (SelectorAttribute != null)
					SelectorAttribute.Filterable = value;
			}
		}

		/// <inheritdoc cref="PXDimensionSelectorAttribute.ValidComboRequired"/>
		public virtual bool ValidComboRequired
		{
			get => SelectorAttribute?.ValidComboRequired ?? false;
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.ValidComboRequired = value;
			}
		}

		/// <inheritdoc cref="PXDimensionSelectorAttribute.SupportNewValues"/>
		public virtual bool SupportNewValues
		{
			get => SelectorAttribute?.SupportNewValues ?? false;
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.SupportNewValues = value;
			}
		}
		#endregion

		#region DefaultAttribute delagation
		protected int _DefAttrIndex = -1;
		protected PXDefaultAttribute DefaultAttribute => _DefAttrIndex == -1 ? null : (PXDefaultAttribute)_Attributes[_DefAttrIndex];

		/// <inheritdoc cref="PXDefaultAttribute.PersistingCheck"/>
		public virtual PXPersistingCheck PersistingCheck
		{
			get => DefaultAttribute?.PersistingCheck ?? PXPersistingCheck.Nothing;
			set
			{
				if (DefaultAttribute != null)
					DefaultAttribute.PersistingCheck = value;
			}
		}
		#endregion
	}
}
