using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
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
        string cacheId = string.Empty;
        static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["CacheConnection"]);
        //static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
        //    return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["CacheConnection"]);
        //});
        //ConnectionMultiplexer connection = lazyConnection.Value;
        static IDatabase cache = connection.GetDatabase();
        
        public AzRedisTokenCache(string userId)
        {
            this.cacheId = userId;

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();        
        }

        public void Load()
        {
            lock (fileLock)
            {
                Debug.Assert(cache != null && cache.IsConnected(cacheId));
                var userIdTokenCache = cache.StringGet(cacheId);
                if (userIdTokenCache.HasValue)
                {
                    //JsonConvert.DeserializeObject<AzRedisTokenCache>(userIdTokenCache);
                    //this.Deserialize(Encoding.UTF8.GetBytes(userIdTokenCache.ToString()));
                    this.Deserialize((byte[])userIdTokenCache);
                }
            }
        }

        public void Persist()
        {
            // reflect changes in the persistent store
            //cache.StringSet(this.userId, JsonConvert.SerializeObject(this));
            //cache.StringSet(this.cacheId, Encoding.UTF8.GetString(this.Serialize()));
            cache.StringSet(this.cacheId, this.Serialize());
            // once the write operation took place, restore the HasStateChanged bit to false
            this.HasStateChanged = false;
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            cache.KeyDelete(cacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist(); 
        }

        // Triggered right before ADAL needs to access the cache.        
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            // Reload the cache from the persistent store, if there is a case where it could have changed since the last access, e.g. like NaiveCache.cs
            Load();

            // or If its deemed more efficient check if in-memory and persistent store versions are the same and only reload from persistent store when 
            // that is not the case, e.g. DbTokenCache.cs
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