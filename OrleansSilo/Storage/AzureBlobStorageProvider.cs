﻿using System;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace OrleansSilo.Storage
{
    /// <summary>
    /// The blob storage provider.
    /// </summary>
    public class AzureBlobStorageProvider : IStorageProvider
    {

        private JsonSerializerSettings settings;

        private CloudBlobContainer container;

        public Logger Log { get; set; }

        public string Name { get; set; }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Log = providerRuntime.GetLogger(this.GetType().Name);

            try
            {
                this.ConfigureJsonSerializerSettings(config);

                if (!config.Properties.ContainsKey("DataConnectionString"))
                {
                    throw new BadProviderConfigException(
                      "The DataConnectionString setting has not been configured in the cloud role. Please add a DataConnectionString setting with a valid Azure Storage connection string.");
                }
                else
                {
                    var account = CloudStorageAccount.Parse(config.Properties["DataConnectionString"]);
                    var blobClient = account.CreateCloudBlobClient();
                    var containerName = config.Properties.ContainsKey("ContainerName") ? config.Properties["ContainerName"] : "grainstate";
                    this.container = blobClient.GetContainerReference(containerName);
                    await this.container.CreateIfNotExistsAsync();
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(0, ex.ToString(), ex);
                throw;
            }
        }

        private void ConfigureJsonSerializerSettings(IProviderConfiguration config)
        {
            // By default, use automatic type name handling, simple assembly names, and no JSON formatting
            this.settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                Formatting = Formatting.None
            };

            if (config.Properties.ContainsKey("SerializeTypeNames"))
            {
                bool serializeTypeNames;
                var serializeTypeNamesValue = config.Properties["SerializeTypeNames"];
                bool.TryParse(serializeTypeNamesValue, out serializeTypeNames);
                if (serializeTypeNames)
                {
                    this.settings.TypeNameHandling = TypeNameHandling.All;
                }
            }

            if (config.Properties.ContainsKey("UseFullAssemblyNames"))
            {
                bool useFullAssemblyNames;
                var useFullAssemblyNamesValue = config.Properties["UseFullAssemblyNames"];
                bool.TryParse(useFullAssemblyNamesValue, out useFullAssemblyNames);
                if (useFullAssemblyNames)
                {
                    this.settings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
                }
            }

            if (config.Properties.ContainsKey("IndentJSON"))
            {
                bool indentJson;
                var indentJsonValue = config.Properties["IndentJSON"];
                bool.TryParse(indentJsonValue, out indentJson);
                if (indentJson)
                {
                    this.settings.Formatting = Formatting.Indented;
                }
            }
        }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainId, GrainState grainState)
        {
            try
            {
                var blobName = GetBlobName(grainType, grainId);
                var blob = this.container.GetBlockBlobReference(blobName);

                var exists = await blob.ExistsAsync();
                if (!exists)
                {
                    return;
                }

                var text = await blob.DownloadTextAsync();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                var data = JsonConvert.DeserializeObject(text, grainState.GetType(), this.settings);
                var dict = ((GrainState)data).AsDictionary();
                grainState.SetAll(dict);
                grainState.Etag = blob.Properties.ETag;
            }
            catch (Exception ex)
            {
                this.Log.Error(0, ex.ToString());
            }
        }

        private static string GetBlobName(string grainType, GrainReference grainId)
        {
            return $"{grainType}-{grainId.ToKeyString()}.json";
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainId, GrainState grainState)
        {
            try
            {
                var blobName = GetBlobName(grainType, grainId);
                var grainStateDictionary = grainState.AsDictionary();
                var storedData = JsonConvert.SerializeObject(grainStateDictionary, this.settings);
                this.Log.Verbose("Serialized grain state is: {0}.", storedData);

                var blob = this.container.GetBlockBlobReference(blobName);
                blob.Properties.ContentType = "application/json";
                await
                  blob.UploadTextAsync(
                    storedData,
                    Encoding.UTF8,
                    AccessCondition.GenerateIfMatchCondition(grainState.Etag),
                    null,
                    null);
                grainState.Etag = blob.Properties.ETag;
            }
            catch (Exception ex)
            {
                this.Log.Error(0, ex.ToString());
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainId, GrainState grainState)
        {
            try
            {
                var blobName = GetBlobName(grainType, grainId);
                var blob = this.container.GetBlockBlobReference(blobName);
                await
                  blob.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.None,
                    AccessCondition.GenerateIfMatchCondition(grainState.Etag),
                    null,
                    null);
                grainState.Etag = blob.Properties.ETag;
            }
            catch (Exception ex)
            {
                this.Log.Error(0, ex.ToString());
            }
        }
    }
}
