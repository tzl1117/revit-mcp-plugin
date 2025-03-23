using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using revit_mcp_plugin.Commands.Interfaces;
using revit_mcp_plugin.Models;
using revit_mcp_plugin.Utils;

namespace revit_mcp_plugin.Commands.Create
{
    public class CreateDoorEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
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
        public List<PointBasedComponent> CreatedInfo { get; private set; }

        public string _levelName = "F01_";
        public bool _structural = true;

        /// <summary>
        /// 设置创建楼板的参数
        /// </summary>
        public void SetParameters(List<PointBasedComponent> data)
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

                    // 2. 创建或获取门类型
                    FamilySymbol doorSymbol = null;
                    using (Transaction tran = new Transaction(doc, "获取门"))
                    {
                        tran.Start();
                        doorSymbol = CreateOrGetDoorType(doc, data.TypeId, data.Width == -1 ? 600 : data.Width);
                        tran.Commit();
                    }
                    if (doorSymbol == null)
                        continue;

                    // 3. 获取所有墙
                    List<Wall> walls = new FilteredElementCollector(doc)
                        .OfClass(typeof(Wall))
                        .Cast<Wall>()
                        .ToList();

                    // 4. 批量创建门
                    using (Transaction tran = new Transaction(doc, "创建门"))
                    {
                        tran.Start();

                        if (!doorSymbol.IsActive)
                            doorSymbol.Activate();
                        // 计算插入点
                        XYZ insertPoint = JZPoint.ToXYZ(data.LocationPoint);
                        // 更新插入点的Z坐标为对应标高和偏移
                        insertPoint = new XYZ(insertPoint.X, insertPoint.Y, data.BaseLevel + data.BaseOffset);

                        // 找到最近的墙和其方向
                        var wallInfo = FindNearestWallAndDirection(walls, insertPoint);
                        if (wallInfo.wall == null) continue;

                        // 创建门
                        FamilyInstance door = doc.Create.NewFamilyInstance(
                            insertPoint,
                            doorSymbol,
                            wallInfo.wall,
                            level,
                            StructuralType.NonStructural);

                        if (door != null)
                        {
                            // 设置门的尺寸
                            door.Symbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).Set(data.Width / 304.8);
                            door.Symbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).Set(data.Height / 304.8);

                            // 根据墙的方向旋转门
                            RotateInstanceToMatchWall(doc, door, wallInfo.direction, insertPoint);

                            // 翻转门确保正常剪切
                            door.flipFacing();
                            doc.Regenerate();
                            door.flipFacing();
                            doc.Regenerate();

                            tran.Commit();
                            //doc.Refresh(0);
                        }
                    }
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
            return "创建门";
        }

        /// <summary>
        /// 创建或获取指定门类型
        /// </summary>
        private FamilySymbol CreateOrGetDoorType(Document doc, int typeId = -1, double widthInMM = 600)
        {
            FamilySymbol doorSymbol = null;
            // Case1 如果有指定有效的门类型
            if (typeId != -1 && typeId != 0)
            {
                ElementId typeELeId = new ElementId(typeId);
                if (typeELeId != null)
                {
                    Element typeEle = doc.GetElement(typeELeId);
                    if (typeEle != null && typeEle is FamilySymbol)
                    {
                        doorSymbol = typeEle as FamilySymbol;
                        return doorSymbol;
                    }
                }
            }

            // Case2 如果没有有效的门类型
            // 门类型是FamilySymbol，其类别是BuiltInCategory.OST_Doors
            doorSymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs => fs.IsActive); // 获取激活的类型作为默认类型
            if (doorSymbol == null)
            {
                doorSymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .Cast<FamilySymbol>()
                .FirstOrDefault();
            }
            if (doorSymbol == null)
                throw new InvalidOperationException("未找到可用的门模型");
            //// 调整门的宽度
            //if (doorSymbol.get_Parameter(BuiltInParameter.FURNITURE_WIDTH) != null)
            //{
            //    doorSymbol.get_Parameter(BuiltInParameter.FURNITURE_WIDTH).Set(widthInMM / 304.8);
            //}
            //else if (doorSymbol.LookupParameter("宽度") != null)
            //{
            //    doorSymbol.LookupParameter("宽度").Set(widthInMM / 304.8);
            //}

            return doorSymbol;
        }

        /// <summary>
        /// 查找最近的墙和其方向
        /// </summary>
        private (Wall wall, XYZ direction) FindNearestWallAndDirection(List<Wall> walls, XYZ point)
        {
            Wall nearestWall = null;
            XYZ wallDirection = null;
            double minDistance = double.MaxValue;

            foreach (Wall wall in walls)
            {
                LocationCurve locationCurve = wall.Location as LocationCurve;
                if (locationCurve == null) continue;

                Curve curve = locationCurve.Curve;
                double distance = curve.Distance(point);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestWall = wall;

                    // 获取墙的方向
                    XYZ startPoint = curve.GetEndPoint(0);
                    XYZ endPoint = curve.GetEndPoint(1);
                    wallDirection = (endPoint - startPoint).Normalize();
                }
            }

            return (nearestWall, wallDirection);
        }

        /// <summary>
        /// 旋转族实例以匹配墙的方向
        /// </summary>
        private void RotateInstanceToMatchWall(Document doc, FamilyInstance instance, XYZ wallDirection, XYZ basePoint)
        {
            // 获取族实例的当前朝向（假设默认朝向是Y轴正方向）
            XYZ instanceDirection = new XYZ(0, 1, 0);

            // 计算旋转角度
            double angle = instanceDirection.AngleTo(wallDirection);

            // 如果角度不为0，进行旋转
            if (Math.Abs(angle) > 0.001)
            {
                // 创建旋转轴（垂直于地面）
                XYZ rotationAxis = XYZ.BasisZ;

                // 判断是否需要额外旋转180度
                XYZ cross = instanceDirection.CrossProduct(wallDirection);
                if (cross.Z < 0)
                {
                    angle = 2 * Math.PI - angle;
                }

                // 执行旋转
                Line axis = Line.CreateBound(basePoint, basePoint + rotationAxis);
                ElementTransformUtils.RotateElement(doc, instance.Id, axis, angle);
            }
        }

    }
}
