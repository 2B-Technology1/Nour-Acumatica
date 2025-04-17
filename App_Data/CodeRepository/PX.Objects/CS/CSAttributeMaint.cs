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
using PX.CS;
using PX.Data;
using PX.Metadata;
using PX.Objects.IN.Matrix.Attributes;

namespace PX.Objects.CS
{
	public class CSAttributeMaint : PXGraph<CSAttributeMaint, CSAttribute>
	{
		[InjectDependency]
		protected IScreenInfoCacheControl ScreenInfoCacheControl { get; set; }

		public PXSelect<CSAttribute,
			Where<CSAttribute.attributeID, NotEqual<MatrixAttributeSelectorAttribute.dummyAttributeName>>> Attributes;
		public PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Current<CSAttribute.attributeID>>>> CurrentAttribute;
		[PXImport(typeof(CSAttribute))]
		public PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Current<CSAttribute.attributeID>>>, 
						OrderBy<Asc<CSAttributeDetail.sortOrder,
								Asc<CSAttributeDetail.valueID>>>> AttributeDetails;
		public PXSelect<CSAttributeGroup, Where<CSAttributeGroup.attributeID, Equal<Required<CSAttribute.attributeID>>>> AttributeGroups;
		public PXSelect<CSScreenAttribute, Where<CSScreenAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>> AttributeScreens;

		protected virtual void CSAttribute_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            var row = e.Row as CSAttribute;
			SetControlsState(row, sender);

			ValidateAttributeID(sender, row);

			if (row.ControlType == CSAttribute.GISelector)
			{
				string[] fields = null;

				if (!string.IsNullOrEmpty(row.ObjectName))
				{
					try
					{
						Type objType = System.Web.Compilation.PXBuildManager.GetType(row.ObjectName, true);
						PXCache objCache = Caches[objType];
						fields = objCache.Fields
							.Where(f => objCache.GetBqlField(f) != null || f.EndsWith("_Attributes", StringComparison.OrdinalIgnoreCase))
							.Where(f => !objCache.GetAttributesReadonly(f).OfType<PXDBTimestampAttribute>().Any())
							.Where(f => !string.IsNullOrEmpty((objCache.GetStateExt(null, f) as PXFieldState)?.ViewName))
							.Where(f => f != "CreatedByID" && f != "LastModifiedByID")
							.ToArray();

						if (fields.Length == 0)
							fields = null;
					}
					catch { }
				}

				PXStringListAttribute.SetList<CSAttribute.fieldName>(sender, row, fields, fields);
			}
		}

		

		protected virtual void CSAttributeDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CSAttributeDetail;
			if(row != null)
			{
				CSAnswers ans = PXSelect<CSAnswers, 
					Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>, 
						And<CSAnswers.value, Equal<Required<CSAnswers.value>>>>>.
					SelectWindowed(this, 0, 1, row.AttributeID, row.ValueID);
				CSAttributeGroup group = AttributeGroups.SelectWindowed(0, 1, row.AttributeID);
				if (ans != null && group != null)
					throw new PXSetPropertyException<CSAttributeDetail.attributeID>(Messages.AttributeDetailCanNotBeDeletedAsItUsed, PXErrorLevel.RowError);
			}
		}

		protected virtual void CSAttributeDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CSAttributeDetail row = e.Row as CSAttributeDetail;

			if (row != null && CurrentAttribute.Current != null)
			{
				row.AttributeID = CurrentAttribute.Current.AttributeID;
			}
		}

		protected virtual void CSAttribute_ControlType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetControlsState(e.Row as CSAttribute, sender);
		}

		protected virtual void CSAttribute_ControlType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CSAttribute;

			if (row == null) return;

			int? oldControlType = row.ControlType, newControlType = (int?)e.NewValue;

			if (oldControlType != newControlType)
				ShowCannotChangeSettingsError(row.AttributeID);
		}

		protected virtual void CSAttribute_ObjectName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CSAttribute;

			if (row == null) return;

			string oldSchemaObject = row.ObjectName, newSchemaObject = (string)e.NewValue;

			if (string.Equals(oldSchemaObject, newSchemaObject, StringComparison.OrdinalIgnoreCase))
				ShowCannotChangeSettingsError(row.AttributeID);
		}

		protected virtual void CSAttribute_ObjectName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CSAttribute;

			if (row == null)
				return;

			string oldObjectName = (string)e.OldValue,
				   newObjectName = row.ObjectName;

			if (!string.Equals(oldObjectName, newObjectName, StringComparison.OrdinalIgnoreCase))
				sender.SetValueExt<CSAttribute.fieldName>(row, null);
		}

		protected virtual void CSAttribute_FieldName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CSAttribute;

			if (row == null) return;

			string oldSchemaField = row.FieldName, newSchemaField = (string)e.NewValue;

			if (string.Equals(oldSchemaField, newSchemaField, StringComparison.OrdinalIgnoreCase))
				ShowCannotChangeSettingsError(row.AttributeID);
		}

		private void ShowCannotChangeSettingsError(string attributeID)
		{
			var groups = AttributeGroups.Select(attributeID);
			if (groups.Any())
				throw new PXSetPropertyException(ErrorMessages.CannotChangeAttributeSettings, string.Join(", ", groups.Select(group => group.GetItem<CSAttributeGroup>().EntityType)));

			var screenAttributes = AttributeScreens.Select(attributeID);
			if (screenAttributes.Any())
				throw new PXSetPropertyException(ErrorMessages.CannotChangeUdfSettings, string.Join(", ", screenAttributes.Select(s => s.GetItem<CSScreenAttribute>().ScreenID)));
		}

		protected virtual void CSAttributeDetail_ValueID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			string newValue = (string)e.NewValue;

			if (string.IsNullOrWhiteSpace(newValue))
			{
				e.NewValue = null;
				return;
			}

			var row = e.Row as CSAttributeDetail;

		    if (row == null || newValue.Equals(row.ValueID, StringComparison.OrdinalIgnoreCase))
				return;

			CSAnswers ans = PXSelect<CSAnswers,
				Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
					And<CSAnswers.value, Equal<Required<CSAnswers.value>>>>>.
				SelectWindowed(this, 0, 1, row.AttributeID, row.ValueID);
			CSAttributeGroup group = AttributeGroups.SelectWindowed(0, 1, row.AttributeID);

			if (ans != null && group != null)
				throw new PXSetPropertyException<CSAttributeDetail.valueID>(Messages.AttributeDetailIdCanNotBeChangedAsItUsed, PXErrorLevel.Error);
		}

		protected virtual void CSAttribute_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CSAttribute item = (CSAttribute)e.Row;
			CSAttribute olditem = (CSAttribute)e.OldRow;
			if (item.ControlType != CSAttribute.Text && olditem.ControlType == CSAttribute.Text)
			{
				item.RegExp = null;
				item.EntryMask = null;
			}
		}

		protected virtual void CSAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CSAttribute row = e.Row as CSAttribute;
			if (row != null)
			{
				if ( string.IsNullOrEmpty(row.Description))
				{
					if (sender.RaiseExceptionHandling<CSAttribute.description>(e.Row, row.Description, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CSAttribute.description)}]")))
					{
						throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, nameof(CSAttribute.description));
					}
				}
				if (!ValidateAttributeID(sender, row))
				{
					var displayName = ((PXFieldState)sender.GetStateExt(row, typeof(CSAttribute.attributeID).Name)).DisplayName;
					if (string.IsNullOrEmpty(displayName)) displayName = typeof(CSAttribute.attributeID).Name;
					throw new PXSetPropertyException(
						string.Concat(displayName, ": ", PXUIFieldAttribute.GetError<CSAttribute.attributeID>(sender, row)));
				}
				if (row.ControlType == CSAttribute.GISelector)
				{
					if (string.IsNullOrEmpty(row.ObjectName))
					{
						throw new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, "Schema Object");
					}
					if (string.IsNullOrEmpty(row.FieldName))
					{
						throw new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, "Schema Field");
					}
				}
			}
		}

		// This method was added to fix breaking change and will be removed in later versions
		protected virtual void CSAttributeDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{ }

		protected virtual void CSAttributeDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CSAttributeDetail)e.Row;

			string valueID = row?.ValueID;
			var controlType = Attributes.Current?.ControlType;
			bool isValueIdValid = string.IsNullOrEmpty(valueID) ||
				!(controlType == CSAttribute.MultiSelectCombo &&
				  valueID.Contains(SystemConstants.MultiSelectValuesDelimiterString));
			sender.RaiseExceptionHandling<CSAttributeDetail.valueID>(row, valueID, isValueIdValid ? null :
				new PXSetPropertyException<CSAttributeDetail.valueID>(Messages.AttributeValueIDCannotContainCommas, PXErrorLevel.Error));
		}

		private PXSetPropertyException GetProhibitedCharacterException()
		{
			return new PXSetPropertyException<CSAttributeDetail.valueID>(Messages.AttributeValueIDCannotContainCommas, PXErrorLevel.Error);
		}

		private static bool ValidateAttributeID(PXCache sender, CSAttribute row)
		{
			if (row == null || string.IsNullOrEmpty(row.AttributeID)) return true;

			if (Char.IsDigit(row.AttributeID[0]))
			{
				PXUIFieldAttribute.SetWarning<CSAttribute.attributeID>(sender, row, Messages.CannotStartWithDigit);
				return false;
			}

			if (row.AttributeID.Contains(" "))
			{
				PXUIFieldAttribute.SetWarning<CSAttribute.attributeID>(sender, row, Messages.CannotContainEmptyChars);
				return false;
			}

			return true;
		}        

        private void SetControlsState(CSAttribute row, PXCache cache)
		{
			if (row != null)
			{
				AttributeDetails.Cache.AllowDelete = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);
				AttributeDetails.Cache.AllowUpdate = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);
				AttributeDetails.Cache.AllowInsert = (row.ControlType == CSAttribute.Combo || row.ControlType == CSAttribute.MultiSelectCombo);

				CSScreenAttribute screenAttribute = AttributeScreens.SelectWindowed(0, 1, row.AttributeID);
				CSAnswers ans = PXSelect<CSAnswers, Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>.SelectWindowed(this, 0, 1, row.AttributeID);
                CSAttributeGroup group = null;
                if(ans == null)
                    group = AttributeGroups.SelectWindowed(0, 1, row.AttributeID);
                bool enabled = screenAttribute == null && ans == null && group == null;
                PXUIFieldAttribute.SetEnabled<CSAttribute.controlType>(cache, row, enabled);
				PXUIFieldAttribute.SetEnabled<CSAttribute.objectName>(cache, row, enabled);
				PXUIFieldAttribute.SetEnabled<CSAttribute.fieldName>(cache, row, enabled);
				cache.AllowDelete = enabled;

				PXUIFieldAttribute.SetEnabled<CSAttribute.entryMask>(cache, row, row.ControlType == CSAttribute.Text);
				PXUIFieldAttribute.SetEnabled<CSAttribute.regExp>(cache, row, row.ControlType == CSAttribute.Text);
			}
		}

		public override void Persist()
		{
			if (Attributes.Current != null)
			{
				if (!PXDBLocalizableStringAttribute.IsEnabled)
				{
					string old = Attributes.Current.List;
					Attributes.Current.List = null;
					foreach (CSAttributeDetail det in AttributeDetails.Select())
					{
						if (!String.IsNullOrEmpty(det.ValueID))
						{
							if (Attributes.Current.List == null)
							{
								Attributes.Current.List = det.ValueID + '\0' + det.Description ?? "";
							}
							else
							{
								Attributes.Current.List = Attributes.Current.List + '\t' + det.ValueID + '\0' + det.Description ?? "";
							}
						}
					}
					if (!String.Equals(old, Attributes.Current.List) && Attributes.Cache.GetStatus(Attributes.Current) == PXEntryStatus.Notchanged)
					{
						Attributes.Cache.SetStatus(Attributes.Current, PXEntryStatus.Updated);
					}
				}
				else
				{
					bool isImport = IsImport;
					bool isExport = IsExport;
					bool isCopyPasteContext = IsCopyPasteContext;
					IsImport = false;
					IsExport = false;
					IsCopyPasteContext = false;
					try
					{
						string[] languages = Attributes.Cache.GetValueExt(null, "ListTranslations") as string[];
						if (languages != null)
						{
							languages = new string[languages.Length];
							foreach (CSAttributeDetail det in AttributeDetails.Select())
							{
								if (!String.IsNullOrEmpty(det.ValueID))
								{
									string[] translations = AttributeDetails.Cache.GetValueExt(det, "DescriptionTranslations") as string[];
									for (int i = 0; i < languages.Length; i++)
									{
										if (languages[i] == null)
										{
											languages[i] = det.ValueID;
										}
										else
										{
											languages[i] = languages[i] + '\t' + det.ValueID;
										}
										string descr = det.Description;
										if (translations != null && translations.Any(_ => !String.IsNullOrEmpty(_)))
										{
											if (i < translations.Length && !String.IsNullOrEmpty(translations[i]))
											{
												descr = translations[i];
											}
											else
											{
												descr = translations.FirstOrDefault(_ => !String.IsNullOrEmpty(_));
											}
										}
										languages[i] = languages[i] + '\0' + descr ?? "";
									}
								}
							}
							Attributes.Cache.SetValueExt(Attributes.Current, "ListTranslations", languages);
						}
					}
					finally
					{
						IsImport = isImport;
						IsExport = isExport;
						IsCopyPasteContext = isCopyPasteContext;
					}
				}
			}

			// Since we don't know if any of the attribute/UDF fields are used in GI/reports/dashboards/etc.,
			// we should reset the whole ScreenInfo cache if any changes to the existing attributes have been made
			bool resetScreenInfo = Attributes.Cache.Updated.Cast<CSAttribute>().Any()
			                       || Attributes.Cache.Deleted.Cast<CSAttribute>().Any();

			base.Persist();

			if (resetScreenInfo)
				ScreenInfoCacheControl.InvalidateCache();
		}
	}
}
