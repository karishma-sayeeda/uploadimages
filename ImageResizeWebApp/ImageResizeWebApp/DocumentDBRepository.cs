﻿using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoAzureStorage
{
	public static class DocumentDBRepository<T> where T : class
	{
		private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
		private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
		private static DocumentClient client;

		public static void Initialize()
		{
			client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
			CreateDatabaseIfNotExistsAsync().Wait();
			CreateCollectionIfNotExistsAsync().Wait();
		}

		private static async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
				}
				else
				{
					throw;
				}
			}
		}
		public static async Task<T> GetItemAsync(string Id)
		{
			try
			{
				Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, Id));
				return (T)(dynamic)document;
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}
				else
				{
					throw;
				}
			}
		}
		private static async Task CreateCollectionIfNotExistsAsync()
		{
			try
			{
				await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await client.CreateDocumentCollectionAsync(
						UriFactory.CreateDatabaseUri(DatabaseId),
						new DocumentCollection { Id = CollectionId },
						new RequestOptions { OfferThroughput = 1000 });
				}
				else
				{
					throw;
				}
			}
		}
	}
}