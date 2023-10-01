// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Linq;
using Azure;
using AzureSDKSpike;

// local emulator  "https://127.0.0.1:10000/devstoreaccount1"


var spike = new AsyncEnumeratorSpike();
await spike.ExecuteAsync();

var storageAccountUri = Environment.GetEnvironmentVariable("StorageAccountUri") ?? string.Empty;

// Configuration Settings by DI.
var services = new ServiceCollection();
services.AddAzureClients(x =>
{
    x.AddBlobServiceClient(new Uri(storageAccountUri));
    x.UseCredential(new DefaultAzureCredential());
});

// Configuration Settings by Client
var blobServiceClient = new BlobServiceClient(
    new Uri(storageAccountUri),
    new DefaultAzureCredential());

if (blobServiceClient == null)
{
    return;
}

var containers = blobServiceClient.GetBlobContainersAsync();
await foreach(var container in containers)
{
    Console.WriteLine($"container: {container.Name} is already exists.");
}

string containerName = "test";

var blobContainerClient = new BlobContainerClient(new Uri($"{storageAccountUri}/{containerName}"), new DefaultAzureCredential());
await blobContainerClient.CreateIfNotExistsAsync();

try
{
    // this method can throw an exception if the container already exists.
    var result = await blobServiceClient.CreateBlobContainerAsync("test");
} 
catch (RequestFailedException ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}
