﻿// -----------------------------------------------------------------------------------------
// <copyright file="BlobTestBase.Common.cs" company="Microsoft">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Pileus;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration;
using Microsoft.WindowsAzure.Storage.Pileus.Utils;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Constraint;
using PileusApp;
using PileusApp.Utils;
using System.Diagnostics;
using PileusApp.YCSB;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Actions;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Pileus
{

    public partial class BlobTestBase : TestBase
    {
        public static string GetRandomContainerName()
        {
            return string.Concat("testc", Guid.NewGuid().ToString("N"));
        }

        public static CloudBlobContainer GetRandomContainerReference()
        {
            CloudBlobClient blobClient = GenerateCloudBlobClient();

            string name = GetRandomContainerName();
            CloudBlobContainer container = blobClient.GetContainerReference(name);

            return container;
        }

        public static List<string> GetBlockIdList(int count)
        {
            List<string> blocks = new List<string>();
            for (int i = 0; i < count; i++)
            {
                blocks.Add(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
            }
            return blocks;
        }

        public static void AssertAreEqual(ICloudBlob expected, ICloudBlob actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.BlobType, actual.BlobType);
                Assert.AreEqual(expected.Uri, actual.Uri);
                Assert.AreEqual(expected.StorageUri, actual.StorageUri);
                Assert.AreEqual(expected.SnapshotTime, actual.SnapshotTime);
                Assert.AreEqual(expected.IsSnapshot, actual.IsSnapshot);
                Assert.AreEqual(expected.SnapshotQualifiedUri, actual.SnapshotQualifiedUri);
                AssertAreEqual(expected.Properties, actual.Properties);
                AssertAreEqual(expected.CopyState, actual.CopyState);
            }
        }

        public static void AssertAreEqual(BlobProperties expected, BlobProperties actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.CacheControl, actual.CacheControl);
                Assert.AreEqual(expected.ContentDisposition, actual.ContentDisposition);
                Assert.AreEqual(expected.ContentEncoding, actual.ContentEncoding);
                Assert.AreEqual(expected.ContentLanguage, actual.ContentLanguage);
                Assert.AreEqual(expected.ContentMD5, actual.ContentMD5);
                Assert.AreEqual(expected.ContentType, actual.ContentType);
                Assert.AreEqual(expected.ETag, actual.ETag);
                Assert.AreEqual(expected.LastModified, actual.LastModified);
                Assert.AreEqual(expected.Length, actual.Length);
            }
        }

        public static void AssertAreEqual(CopyState expected, CopyState actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.BytesCopied, actual.BytesCopied);
                Assert.AreEqual(expected.CompletionTime, actual.CompletionTime);
                Assert.AreEqual(expected.CopyId, actual.CopyId);
                Assert.AreEqual(expected.Source, actual.Source);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.TotalBytes, actual.TotalBytes);
            }
        }
    }
}
