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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.SO;
using PX.SM;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PX.Commerce.Objects
{
	public class SOOrderTypeMaintExt : PXGraphExtension<SOOrderTypeMaint>
	{
		protected virtual void _(Events.FieldVerifying<SOOrderType, SOOrderTypeExt.encryptAndPseudonymizePII> e)
		{
			if (e.NewValue != null)
			{
				if ((bool)e.NewValue)
				{
					ValidateCertificate();
				}
			}
		}
		public void ValidateCertificate()
		{
			try
			{
				bool invalid = false;
				PreferencesSecurity security = new PXSetup<PreferencesSecurity>(Base).Current;
				if (security?.DBCertificateName != null)
				{
					PXDBCryptStringAttribute.SetDecrypted<Certificate.password>(Base.Caches[typeof(CetrificateFile)], true);

					CetrificateFile certificate = PXSelect<CetrificateFile, Where<CetrificateFile.name, Equal<Required<Certificate.name>>>>.Select(Base, security.DBCertificateName);
					if (certificate != null)
					{
						UploadFileRevision revision = PXSelectJoin<UploadFileRevision,
						InnerJoin<UploadFile,
							On<UploadFile.fileID, Equal<UploadFileRevision.fileID>,
								And<UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
						Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>
					.Select(Base, certificate.FileID);

						if (revision != null)
						{
							X509Certificate2 certificatefile = new X509Certificate2(revision.Data, certificate.Password,
								X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

							if (certificatefile?.PrivateKey != null && certificatefile.PrivateKey.KeySize < 2048) invalid = true;
							if (certificatefile?.PublicKey?.Key == null || (certificatefile?.PublicKey?.Key != null && certificatefile?.PublicKey?.Key.KeySize < 2048)) invalid = true;


						}
						else invalid = true;
					}
					else invalid = true;
				}
				else invalid = true;
				if (invalid) throw new PXSetPropertyException(BCMessages.CertificateNotValid);
			}
			catch (CryptographicException e)
			{
				throw new PXSetPropertyException(e.Message);
			}
		}
	}
}
