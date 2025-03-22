using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using revit_mcp_plugin.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace revit_mcp_plugin.Commands.Access
{
    public class GetAvailableFamilyTypesEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        // 执行结果
        public List<FamilyTypeInfo> ResultFamilyTypes { get; private set; }

        // 状态同步对象
        public bool TaskCompleted { get; private set; }
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        // 过滤条件
        public List<string> CategoryList { get; set; }
        public string FamilyNameFilter { get; set; }
        public int? Limit { get; set; }

        // 执行时间，略微比调用超时更短一些
        public bool WaitForCompletion(int timeoutMilliseconds = 12500)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;

                // 获取所有族类型
                var collector = new FilteredElementCollector(doc);
                var familySymbols = collector.OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>();

                // 应用过滤条件
                if (CategoryList != null && CategoryList.Any())
                {
                    familySymbols = familySymbols.Where(fs => CategoryList.Contains(fs.Category?.Name));
                }

                if (!string.IsNullOrEmpty(FamilyNameFilter))
                {
                    familySymbols = familySymbols.Where(fs => fs.FamilyName.IndexOf(FamilyNameFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // 限制返回数量
                if (Limit.HasValue && Limit.Value > 0)
                {
                    familySymbols = familySymbols.Take(Limit.Value);
                }

                // 转换为FamilyTypeInfo列表
                ResultFamilyTypes = familySymbols.Select(fs => new FamilyTypeInfo
                {
                    Id = fs.Id.IntegerValue,
                    UniqueId = fs.UniqueId,
                    FamilyName = fs.FamilyName,
                    TypeName = fs.Name,
                    Category = fs.Category?.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "获取族类型失败: " + ex.Message);
            }
            finally
            {
                TaskCompleted = true;
                _resetEvent.Set();
            }
        }

        public string GetName()
        {
            return "获取可用族类型";
        }
    }

    public class FamilyTypeInfo
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string Category { get; set; }
    }
}