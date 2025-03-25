using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using revit_mcp_plugin.Commands.Interfaces;
using revit_mcp_plugin.Models;
using revit_mcp_plugin.Utils;

namespace revit_mcp_plugin.Commands.Create
{
    public class CreateFloorEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        private UIApplication uiApp;
        private UIDocument uiDoc => uiApp.ActiveUIDocument;
        private Document doc => uiDoc.Document;
        private Autodesk.Revit.ApplicationServices.Application app => uiApp.Application;
        /// <summary>
        /// 事件等待对象
        /// </summary>
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        /// <summary>
        /// 楼板数据
        /// </summary>
        public List<ShellComponent> CreatedInfo { get; private set; }

        public string _floorName = "常规 - ";
        public string _levelName = "F01_";
        public bool _structural = true;

        /// <summary>
        /// 设置创建楼板的参数
        /// </summary>
        public void SetParameters(List<ShellComponent> data)
        {
            CreatedInfo = data;
            _resetEvent.Reset();
        }
        public void Execute(UIApplication uiapp)
        {
            this.uiApp = uiapp;

            try
            {
                foreach (var floorData in CreatedInfo)
                {
                    // 1. 创建标高
                    Level level = null;
                    using (Transaction tran = new Transaction(doc, "创建标高"))
                    {
                        tran.Start();
                        level = doc.CreateOrGetLevel(floorData.BaseLevel / 304.8, $"{_levelName}{CreatedInfo.IndexOf(floorData) + 1}");
                        tran.Commit();
                    }
                    if (level == null)
                        continue;

                    // 2. 创建或获取楼板类型
                    FloorType floorType = null;
                    using (Transaction tran = new Transaction(doc, "创建楼板类型"))
                    {
                        tran.Start();
                        floorType = GetOrCreateFloorType(doc, floorData.TypeId, floorData.Thickness);
                        tran.Commit();
                    }

                    if (floorType == null)
                        continue;

                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"创建楼板时出错: {ex.Message}");
            }
            finally
            {
                _resetEvent.Set(); // 通知等待线程操作已完成
            }
        }

        /// <summary>
        /// 等待创建完成
        /// </summary>
        /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
        /// <returns>操作是否在超时前完成</returns>
        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        /// <summary>
        /// IExternalEventHandler.GetName 实现
        /// </summary>
        public string GetName()
        {
            return "创建楼板";
        }

        /// <summary>
        /// 获取或创建指定厚度的楼板类型
        /// </summary>
        /// <param name="thicknessInMM">目标厚度（毫米）</param>
        /// <returns>符合厚度要求的楼板类型</returns>
        private FloorType GetOrCreateFloorType(Document doc, int typeId, double thicknessInMM = 200)
        {
            // Case1 如果有指定有效的楼板类型
            if (typeId != -1 && typeId != 0)
            {
                FloorType floorType = null;
                ElementId typeELeId = new ElementId(typeId);
                if (typeELeId != null)
                {
                    Element typeEle = doc.GetElement(typeELeId);
                    if (typeEle != null && typeEle is FloorType)
                    {
                        floorType = typeEle as FloorType;
                        return floorType;
                    }
                }
            }

            // Case2 如果没有有效的楼板类型
            // 将毫米转换为英尺
            double thicknessInFeet = thicknessInMM / 304.8;

            // 查找匹配厚度的楼板类型
            FloorType existingType = new FilteredElementCollector(doc)
                                     .OfClass(typeof(FloorType))                    // 仅获取FloorType类
                                     .OfCategory(BuiltInCategory.OST_Floors)        // 仅获取楼板类别
                                     .Cast<FloorType>()                            // 转换为FloorType类型
                                     .FirstOrDefault(w => w.Name == $"{_floorName}{thicknessInMM}mm");
            if (existingType != null)
                return existingType;
            // 如果没有找到匹配的楼板类型，创建新的
            FloorType baseFloorType = existingType = new FilteredElementCollector(doc)
                                     .OfClass(typeof(FloorType))                    // 仅获取FloorType类
                                     .OfCategory(BuiltInCategory.OST_Floors)        // 仅获取楼板类别
                                     .Cast<FloorType>()                            // 转换为FloorType类型
                                     .FirstOrDefault(w => w.Name.Contains("常规"));
            if (existingType != null)
            {
                baseFloorType = existingType = new FilteredElementCollector(doc)
                                     .OfClass(typeof(FloorType))                    // 仅获取FloorType类
                                     .OfCategory(BuiltInCategory.OST_Floors)        // 仅获取楼板类别
                                     .Cast<FloorType>()                            // 转换为FloorType类型
                                     .FirstOrDefault();
            }

            // 复制楼板类型
            FloorType newFloorType = null;
            newFloorType = baseFloorType.Duplicate($"{_floorName}{thicknessInMM}mm") as FloorType;

            // 设置新楼板类型的厚度
            // 获取构造层设置
            CompoundStructure cs = newFloorType.GetCompoundStructure();
            if (cs != null)
            {
                // 获取所有层
                IList<CompoundStructureLayer> layers = cs.GetLayers();
                if (layers.Count > 0)
                {
                    // 计算当前总厚度
                    double currentTotalThickness = cs.GetWidth();

                    // 按比例调整每层厚度
                    for (int i = 0; i < layers.Count; i++)
                    {
                        CompoundStructureLayer layer = layers[i];
                        double newLayerThickness = thicknessInFeet;
                        cs.SetLayerWidth(i, newLayerThickness);
                    }

                    // 应用修改后的构造层设置
                    newFloorType.SetCompoundStructure(cs);
                }
            }
            return newFloorType;
        }

    }
}
