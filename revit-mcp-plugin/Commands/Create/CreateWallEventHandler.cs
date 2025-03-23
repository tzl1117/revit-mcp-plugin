using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using revit_mcp_plugin.Commands.Interfaces;
using revit_mcp_plugin.Models;
using revit_mcp_plugin.Utils;

namespace revit_mcp_plugin.Commands.Create
{
    public class CreateWallEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
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
        public List<LineBasedComponent> CreatedInfo { get; private set; }

        public string _levelName = "F01_";
        public string _wallName = "常规 - ";
        public bool _structural = false;

        /// <summary>
        /// 设置创建的参数
        /// </summary>
        public void SetParameters(List<LineBasedComponent> data)
        {
            CreatedInfo = data;
            _resetEvent.Reset();
        }
        public void Execute(UIApplication uiapp)
        {
            this.uiApp = uiapp;

            try
            {
                foreach (var data in CreatedInfo)
                {
                    // 1. 创建标高
                    Level level = null;
                    using (Transaction tran = new Transaction(doc, "创建标高"))
                    {
                        tran.Start();
                        level = doc.CreateOrGetLevel(data.BaseLevel / 304.8, $"{_levelName}{CreatedInfo.IndexOf(data) + 1}");
                        tran.Commit();
                    }
                    if (level == null)
                        continue;


                    // 2. 创建或获取墙体类型
                    WallType wallType = null;
                    // 先查找是否存在指定Id的建筑墙类型
                    using (Transaction tran = new Transaction(doc, "创建墙类型"))
                    {
                        tran.Start();
                        wallType = CreateOrGetWallType(doc, data.TypeId, data.Thickness);
                        tran.Commit();
                    }
                    if (wallType == null)
                        throw new Exception("墙体类型为空");

                    // 3. 批量创建墙体
                    Wall wall = null;
                    using (Transaction tran = new Transaction(doc, "创建墙体"))
                    {
                        tran.Start();

                        Line curve = JZLine.ToLine(data.LocationLine);

                        wall = Wall.Create
                        (
                          doc,
                          curve,
                          wallType.Id,
                          level.Id,
                          data.Height / 304.8,
                          data.BaseOffset / 304.8,
                          false,  // 翻转方向
                          _structural   // 设置为建筑墙（非结构墙）
                        );

                        tran.Commit();
                        //doc.Refresh(0);
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"创建墙时出错: {ex.Message}");
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
            return "创建墙";
        }

        /// <summary>
        /// 创建或获取指定厚度的墙体类型
        /// </summary>
        private WallType CreateOrGetWallType(Document doc, int typeId, double width)
        {
            // Case1 如果有指定有效的类型
            if (typeId != -1 && typeId != 0)
            {
                WallType wallType = null;
                ElementId typeELeId = new ElementId(typeId);
                if (typeELeId != null)
                {
                    Element typeEle = doc.GetElement(typeELeId);
                    if (typeEle != null && typeEle is WallType)
                    {
                        wallType = typeEle as WallType;
                        return wallType;
                    }
                }
            }

            // Case2 如果没有有效的类型
            // 先查找是否存在指定厚度的建筑墙类型
            WallType existingType = new FilteredElementCollector(doc)
                                    .OfClass(typeof(WallType))
                                    .Cast<WallType>()
                                    .FirstOrDefault(w => w.Name == $"{_wallName}{width}mm");
            if (existingType != null)
                return existingType;

            // 不存在则创建新的墙体类型，基于基本墙
            WallType baseWallType = new FilteredElementCollector(doc)
                                    .OfClass(typeof(WallType))
                                    .Cast<WallType>()
                                    .FirstOrDefault(w => w.Name.Contains("常规")); ;
            if (baseWallType == null)
            {
                baseWallType = new FilteredElementCollector(doc)
                                    .OfClass(typeof(WallType))
                                    .Cast<WallType>()
                                    .FirstOrDefault(); ;
            }

            if (baseWallType == null)
                throw new InvalidOperationException("未找到可用的基础墙类型");

            // 复制墙体类型
            WallType newWallType = null;
            newWallType = baseWallType.Duplicate($"{_wallName}{width}mm") as WallType;

            // 设置墙厚
            CompoundStructure cs = newWallType.GetCompoundStructure();
            if (cs != null)
            {
                // 获取原始层的材料ID
                ElementId materialId = cs.GetLayers().First().MaterialId;

                // 创建新的单层结构
                CompoundStructureLayer newLayer = new CompoundStructureLayer(
                    width / 304.8,  // 宽度（转换为英尺）
                    MaterialFunctionAssignment.Structure,  // 功能分配
                    materialId  // 材料ID
                );

                // 创建新的复合结构
                IList<CompoundStructureLayer> newLayers = new List<CompoundStructureLayer> { newLayer };
                cs.SetLayers(newLayers);

                // 应用新的复合结构
                newWallType.SetCompoundStructure(cs);
            }
            return newWallType;
        }
    }
}
