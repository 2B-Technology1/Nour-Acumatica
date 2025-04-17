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
using System.Collections.Generic;
using PX.SM;
using WebDAVClient.Model;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public interface IBigCommerceWebDAVClient
    {
        Uri ServerHttps { get; set; }
        Uri ServerHttp { get; set; }
        string BasePath { get; set; }
        Item ContentFolder { get; set; }

        IEnumerable<Item> List(string path = "/", int? depth = 1);
        FileInfo Download(string remoteFilePath);
        T Upload<T>(byte[] byteData, string name) where T : IWebDAVEntity, new();

        Item GetFolder(string path = "/");
        Item GetFile(string path = "/");
        IEnumerable<Item> GetServerFilesList();

        bool CreateDir(string remotePath, string name);
        void DeleteFolder(string href);
        void DeleteFile(string href);
        bool MoveFolder(string srcFolderPath, string dstFolderPath);
        bool MoveFile(string srcFilePath, string dstFilePath);
    }
}
