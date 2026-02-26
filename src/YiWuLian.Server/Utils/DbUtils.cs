using Quick.EntityFrameworkCore.Plus;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using YiQiDong.Agent;

namespace YiWuLian.Server.Utils
{
    public class DbUtils
    {
        private Dictionary<string, IDbContextConfigHandler> configHandlerDict;
        private Dictionary<string, JsonTypeInfo> configHandlerTypeInfoDict;
        
        public static DbUtils AppDbUtils{ get; private set; }
        public static DbUtils LogDbUtils{ get; private set; }

        public static void Init()
        {
            AppDbUtils = new();
            LogDbUtils = new();
        }
        
        public DbUtils()
        {
            AbstractDbContextConfigHandler.BackupFilePrefix = "数据库备份";
            configHandlerDict = new Dictionary<string, IDbContextConfigHandler>()
            {
                [typeof(Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler).FullName] =
                    new Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler() { DataSource = "Config.db" },
                [typeof(Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler).FullName] =
                    new Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler()
            };
            configHandlerTypeInfoDict = new Dictionary<string, JsonTypeInfo>()
            {
                [typeof(Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler).FullName] =
                    Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandlerSerializerContext.Default.SQLiteDbContextConfigHandler,
                [typeof(Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler).FullName] =
                    Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandlerSerializerContext.Default.MySqlDbContextConfigHandler
            };
        }

        public JsonNode SerializerConfigHandler(IDbContextConfigHandler configHandler)
        {
            var configHandlerTypeName = configHandler.GetType().FullName;
            if (!configHandlerTypeInfoDict.TryGetValue(configHandlerTypeName, out var configHandlerJsonTypeInfo))
                throw new ArgumentException($"AppDbType参数值错误，类型[{configHandlerTypeName}]未知！");
            return JsonSerializer.SerializeToNode(configHandler, configHandlerJsonTypeInfo);
        }

        private string GetDatabaseName(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = AgentContext.Container?.Id;
            if (string.IsNullOrEmpty(name))
                name = Assembly.GetEntryAssembly().GetName().Name;
            return name;
        }

        public Dictionary<string, string> GetDbTypeDict() => configHandlerDict.ToDictionary(t => t.Key, t => t.Value.Name);

        public IDbContextConfigHandler GetDbContextConfigHandler(string dbType, JsonNode dbConfig = null)
        {
            if (string.IsNullOrEmpty(dbType))
                dbType = configHandlerDict.FirstOrDefault().Key;

            if (configHandlerDict.TryGetValue(dbType, out var configHandler))
            {
                if (dbConfig != null)
                {
                    if (!configHandlerTypeInfoDict.TryGetValue(dbType, out var configHandlerJsonTypeInfo))
                        throw new ArgumentException($"AppDbType参数值错误，类型[{dbType}]未知！");
                    configHandler = (IDbContextConfigHandler)JsonSerializer.Deserialize(dbConfig, configHandlerJsonTypeInfo);
                    configHandlerDict[dbType] = configHandler;
                }
            }
            else
            {
                var item = configHandlerDict.FirstOrDefault();
                dbType = item.Key;
                configHandler = item.Value;
                if (!configHandlerTypeInfoDict.TryGetValue(dbType, out var configHandlerJsonTypeInfo))
                    throw new ArgumentException($"AppDbType参数值错误，类型[{dbType}]未知！");
            }
            return configHandler;
        }
    }
}
