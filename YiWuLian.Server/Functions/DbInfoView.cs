using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Quick.Fields;
using System.ComponentModel;
using System.Reflection;
using YiQiDong.Core;
using YiQiDong.Protocol.V1.Model;

namespace YiWuLian.Server.Functions
{
    public class DbInfoView : AbstractFunction
    {
        public override string Name => "数据库结构";
        private Func<DbContext> getDbContextFunc { get; set; }
        
        public class DbTable
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public IEnumerable<DbTableColumn> Properties { get; set; }
        }

        public class DbTableColumn
        {
            public string Name { get; set; }
            public bool IsPrimaryKey { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }
        private DbTable[] tables;

        public DbInfoView(Func<DbContext> getDbContextFunc)
        {
            this.getDbContextFunc = getDbContextFunc;
        }

        public override List<FieldForGet> Execute(FunctionRequest request)
        {
            if (tables == null)
            {
                IEntityType[] entityTypes = null;
                using (var dbContext = getDbContextFunc())
                    entityTypes = dbContext.Model.GetEntityTypes().ToArray();

                if (entityTypes == null)
                    throw new ApplicationException($"entityTypes为空！");
                tables = entityTypes.Select(t => new DbTable()
                {
                    Name = t.GetTableName(),
                    DisplayName = t.ClrType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? t.ClrType.GetCustomAttribute<CommentAttribute>()?.Comment,
                    Properties = t.GetProperties().Select(p => new DbTableColumn()
                    {
                        Name = p.Name,
                        IsPrimaryKey = p.IsPrimaryKey(),
                        DisplayName = p.PropertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.PropertyInfo.GetCustomAttribute<CommentAttribute>()?.Comment,
                        Description = p.PropertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                    })
                }).ToArray();
            }
            List<FieldForGet> list = new List<FieldForGet>();
            foreach (var table in tables)
            {
                var tableBodyRowList = new List<FieldForGet>();
                foreach (var item in table.Properties)
                {
                    tableBodyRowList.Add(new FieldForGet()
                    {
                        Type = FieldType.ContainerTableTr,
                        Children =
                        [
                            new FieldForGet(){ Type = FieldType.ContainerTableTd, Value=item.Name },
                            new FieldForGet(){ Type = FieldType.ContainerTableTd, Value=item.DisplayName },
                            new FieldForGet()
                            {
                                Type = FieldType.ContainerTableTd,
                                Children = [
                                    new FieldForGet(){ Type = FieldType.HtmlPre, Value=item.Description },
                                ]
                            }
                        ]
                    });
                }
                list.Add(new FieldForGet()
                {
                    Id = table.Name,
                    Type = FieldType.ContainerGroup,
                    Name = $"{table.Name} - {table.DisplayName}",
                    Children = [                    
                        new FieldForGet()
                        {
                            Type = FieldType.ContainerTable,
                            ContainerTable_Bordered = true,
                            Children = [
                                new()
                                {
                                    Type = FieldType.ContainerTableHead,
                                    Theme = FieldTheme.Light,
                                    Children = [
                                        new ()
                                        {
                                            Type = FieldType.ContainerTableTr,
                                            Children = [
                                                new FieldForGet(){ Type = FieldType.ContainerTableTh, Value="字段名" },
                                                new FieldForGet(){ Type = FieldType.ContainerTableTh, Value="显示名" },
                                                new FieldForGet(){ Type = FieldType.ContainerTableTh, Value="描述" },
                                            ]
                                        }
                                    ]
                                },
                                new()
                                {
                                    Type = FieldType.ContainerTableBody,
                                    Children = tableBodyRowList
                                }
                            ]
                        }
                    ]
                });
            }
            return [ new FieldForGet() { Type = FieldType.ContainerTab, Children = list } ];
        }
    }
}
