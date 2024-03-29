﻿// -----------------------------------------------------------------------------------------
// <copyright file="BlobTestBase.cs" company="Microsoft">
//    Copyright 2013 Microsoft Corporation
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// -----------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.WindowsAzure.Storage.Pileus
{
    public partial class BlobTestBase : TestBase
    {
        
        public static void WaitForCopy(ICloudBlob blob)
        {
            bool copyInProgress = true;
            while (copyInProgress)
            {
                Thread.Sleep(1000);
                blob.FetchAttributes();
                copyInProgress = (blob.CopyState.Status == CopyStatus.Pending);
            }
        }

#if TASK
        public static void WaitForCopyTask(ICloudBlob blob)
        {
            bool copyInProgress = true;
            while (copyInProgress)
            {
                Thread.Sleep(1000);
                blob.FetchAttributesAsync().Wait();
                copyInProgress = (blob.CopyState.Status == CopyStatus.Pending);
            }
        }
#endif

        public static List<string> CreateBlobs(CloudBlobContainer container, int count, BlobType type)
        {
            string name;
            List<string> blobs = new List<string>();
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case BlobType.BlockBlob:
                        name = "bb" + Guid.NewGuid().ToString();
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
                        blockBlob.PutBlockList(new string[] { });
                        blobs.Add(name);
                        break;

                    case BlobType.PageBlob:
                        name = "pb" + Guid.NewGuid().ToString();
                        CloudPageBlob pageBlob = container.GetPageBlobReference(name);
                        pageBlob.Create(0);
                        blobs.Add(name);
                        break;
                }
            }
            return blobs;
        }

#if TASK
        public static List<string> CreateBlobsTask(CloudBlobContainer container, int count, BlobType type)
        {
            string name;
            List<string> blobs = new List<string>();
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case BlobType.BlockBlob:
                        name = "bb" + Guid.NewGuid().ToString();
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
                        blockBlob.PutBlockListAsync(new string[] { }).Wait();
                        blobs.Add(name);
                        break;

                    case BlobType.PageBlob:
                        name = "pb" + Guid.NewGuid().ToString();
                        CloudPageBlob pageBlob = container.GetPageBlobReference(name);
                        pageBlob.CreateAsync(0).Wait();
                        blobs.Add(name);
                        break;
                }
            }
            return blobs;
        }
#endif

        public static void UploadText(ICloudBlob blob, string text, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            byte[] textAsBytes = encoding.GetBytes(text);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(textAsBytes, 0, textAsBytes.Length);
                if (blob.BlobType == BlobType.PageBlob)
                {
                    int lastPageSize = (int)(stream.Length % 512);
                    if (lastPageSize != 0)
                    {
                        byte[] padding = new byte[512 - lastPageSize];
                        stream.Write(padding, 0, padding.Length);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                blob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = 2;
                blob.UploadFromStream(stream, accessCondition, options, operationContext);
            }
        }

        public static void UploadTextAPM(ICloudBlob blob, string text, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            byte[] textAsBytes = encoding.GetBytes(text);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(textAsBytes, 0, textAsBytes.Length);
                if (blob.BlobType == BlobType.PageBlob)
                {
                    int lastPageSize = (int)(stream.Length % 512);
                    if (lastPageSize != 0)
                    {
                        byte[] padding = new byte[512 - lastPageSize];
                        stream.Write(padding, 0, padding.Length);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                blob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = 2;
                using (AutoResetEvent waitHandle = new AutoResetEvent(false))
                {
                    IAsyncResult result = blob.BeginUploadFromStream(stream, accessCondition, options, operationContext,
                        ar => waitHandle.Set(),
                        null);
                    waitHandle.WaitOne();
                    blob.EndUploadFromStream(result);
                }
            }
        }

#if TASK
        public static void UploadTextTask(ICloudBlob blob, string text, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            byte[] textAsBytes = encoding.GetBytes(text);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(textAsBytes, 0, textAsBytes.Length);
                if (blob.BlobType == BlobType.PageBlob)
                {
                    int lastPageSize = (int)(stream.Length % 512);
                    if (lastPageSize != 0)
                    {
                        byte[] padding = new byte[512 - lastPageSize];
                        stream.Write(padding, 0, padding.Length);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                blob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = 2;
                try
                {
                    blob.UploadFromStreamAsync(stream, accessCondition, options, operationContext).Wait();
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }
        }
#endif

        public static string DownloadText(ICloudBlob blob, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                blob.DownloadToStream(stream, accessCondition, options, operationContext);
                return encoding.GetString(stream.ToArray());
            }
        }

        public static string DownloadTextAPM(ICloudBlob blob, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (AutoResetEvent waitHandle = new AutoResetEvent(false))
                {
                    IAsyncResult result = blob.BeginDownloadToStream(stream, accessCondition, options, operationContext, ar => waitHandle.Set(), null);
                    waitHandle.WaitOne();
                    blob.EndDownloadToStream(result);
                    return encoding.GetString(stream.ToArray());
                }
            }
        }

#if TASK
        public static string DownloadTextTask(ICloudBlob blob, Encoding encoding, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    blob.DownloadToStreamAsync(stream, accessCondition, options, operationContext).Wait();
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
                return encoding.GetString(stream.ToArray());
            }
        }
#endif
    }
}
