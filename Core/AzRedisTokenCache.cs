using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

// derived from the following aka.ms/aadSamples provided TokenCache implementations
// https://github.com/Azure-Samples/active-directory-dotnet-native-headless.git | TodoListClient | FileCache.cs
// https://github.com/Azure-Samples/active-directory-dotnet-webapi-onbehalfof.git | TodoListService | DAL | DbTokenCache.cs
// https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect.git | TodoListWebApp | Utils | NaiveSessionCache.cs

namespace MyUsrn.Dnx.Core
{

    public class AzRedisTokenCache : TokenCache
    {
        static readonly object fileLock = new object();
        string userId = string.Empty;
        string cacheId = string.Empty;
        static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["CacheConnection"]);
        //static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
        //    return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["CacheConnection"]);
        //});
        //ConnectionMultiplexer connection = lazyConnection.Value;
        static IDatabase cache = connection.GetDatabase();
        
        public AzRedisTokenCache(string userId)
        {
            this.userId = userId;

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            //Debug.Assert(cache.IsConnected());
            var userIdTokenCache = cache.StringGet(userId);
            var userIdTokenCacheBytes = Encoding.UTF8.GetBytes(cache.StringGet(userIdTokenCache.ToString()));
            this.Deserialize(userIdTokenCacheBytes);
            //this.Deserialize((userIdTokenCacheBytes == null) ? null : userIdTokenCacheBytes);
        }

        //public void Load()
        //{
        //    lock (fileLock)
        //    {
        //        this.Deserialize((byte[])HttpContext.Current.Session[cacheId]);
        //    }
        //}

        public void Persist()
        {
            lock (fileLock)
            {
                // reflect changes in the persistent store
                HttpContext.Current.Session[cacheId] = this.Serialize();
                // once the write operation took place, restore the HasStateChanged bit to false
                this.HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            System.Web.HttpContext.Current.Session.Remove(cacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist(); 
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            //Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                Persist();                  
            }
        }
    }
}