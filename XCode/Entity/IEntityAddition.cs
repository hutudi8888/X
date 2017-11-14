﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NewLife.Reflection;

namespace XCode
{
    /// <summary>实体累加接口。实现Count=Count+123的效果</summary>
    public interface IEntityAddition
    {
        #region 属性
        /// <summary>实体对象</summary>
        IEntity Entity { get; set; }
        #endregion

        #region 累加
        /// <summary>设置累加字段</summary>
        /// <param name="names">字段集合</param>
        void Set(IEnumerable<String> names);

        IDictionary<String, Object[]> Get();

        void Reset(IDictionary<String, Object[]> value);

        ///// <summary>删除累加字段。</summary>
        ///// <param name="name">字段名称</param>
        ///// <param name="restore">是否恢复数据</param>
        ///// <returns>是否成功删除累加字段</returns>
        //Boolean RemoveField(String name, Boolean restore = false);

        ///// <summary>尝试获取累加数据</summary>
        ///// <param name="name">字段名称</param>
        ///// <param name="value">累加数据</param>
        ///// <param name="sign">正负</param>
        ///// <returns>是否获取指定字段的累加数据</returns>
        //Boolean TryGetValue(String name, out Object value, out Boolean sign);

        ///// <summary>清除累加字段数据。Update后调用该方法</summary>
        //void ClearValues();
        #endregion
    }

    /// <summary>实体累加接口。实现Count+=1的效果</summary>
    class EntityAddition : IEntityAddition
    {
        #region 属性
        /// <summary>实体对象</summary>
        public IEntity Entity { get; set; }
        #endregion

        #region 累加
        [NonSerialized]
        private ConcurrentDictionary<String, Object> _Data;

        /// <summary>设置累加字段</summary>
        /// <param name="names">字段集合</param>
        public void Set(IEnumerable<String> names)
        {
            // 检查集合是否为空
            if (_Data == null) _Data = new ConcurrentDictionary<String, Object>();

            foreach (var item in names)
            {
                _Data.TryAdd(item, Entity[item]);
            }
        }

        public IDictionary<String, Object[]> Get()
        {
            var dic = new Dictionary<String, Object[]>();

            var df = _Data;
            if (df == null) return dic;

            foreach (var item in df)
            {
                var vs = new Object[2];
                dic[item.Key] = vs;

                vs[0] = Entity[item.Key];
                vs[1] = item.Value;
            }

            return dic;
        }

        public void Reset(IDictionary<String, Object[]> value)
        {
            if (value == null || value.Count == 0) return;

            var df = _Data;
            if (df == null) return;

            foreach (var item in df)
            {
                var vs = value[item.Key];
                if (vs != null && vs.Length > 0) df[item.Key] = vs[0];
            }
        }

        ///// <summary>删除累加字段。</summary>
        ///// <param name="name">字段名称</param>
        ///// <param name="restore">是否恢复数据</param>
        ///// <returns>是否成功删除累加字段</returns>
        //public Boolean RemoveField(String name, Boolean restore = false)
        //{
        //    var df = _Additions;
        //    if (df == null) return false;
        //    if (!df.TryGetValue(name, out var obj)) return false;

        //    if (restore) Entity[name] = obj;

        //    return true;
        //}

        ///// <summary>尝试获取累加数据</summary>
        ///// <param name="name">字段名称</param>
        ///// <param name="value">累加数据绝对值</param>
        ///// <param name="sign">正负</param>
        ///// <returns>是否获取指定字段的累加数据</returns>
        //public Boolean TryGetValue(String name, out Object value, out Boolean sign)
        //{
        //    value = null;
        //    sign = true;

        //    var df = _Additions;
        //    if (df == null) return false;
        //    if (!df.TryGetValue(name, out value)) return false;

        //    // 如果原始值是0，不使用累加，因为可能原始数据字段是NULL，导致累加失败
        //    if (Convert.ToInt64(value) == 0) return false;

        //    // 计算累加数据
        //    var current = Entity[name];
        //    var type = current.GetType();
        //    var code = Type.GetTypeCode(type);
        //    switch (code)
        //    {
        //        case TypeCode.Char:
        //        case TypeCode.Byte:
        //        case TypeCode.SByte:
        //        case TypeCode.Int16:
        //        case TypeCode.Int32:
        //        case TypeCode.Int64:
        //        case TypeCode.UInt16:
        //        case TypeCode.UInt32:
        //        case TypeCode.UInt64:
        //            {
        //                var v = Convert.ToInt64(current) - Convert.ToInt64(value);
        //                if (v < 0)
        //                {
        //                    v *= -1;
        //                    sign = false;
        //                }
        //                //value = Convert.ChangeType(v, type);
        //                value = v;
        //            }
        //            break;
        //        case TypeCode.Single:
        //            {
        //                var v = (Single)current - (Single)value;
        //                if (v < 0)
        //                {
        //                    v *= -1;
        //                    sign = false;
        //                }
        //                value = v;
        //            }
        //            break;
        //        case TypeCode.Double:
        //            {
        //                var v = (Double)current - (Double)value;
        //                if (v < 0)
        //                {
        //                    v *= -1;
        //                    sign = false;
        //                }
        //                value = v;
        //            }
        //            break;
        //        case TypeCode.Decimal:
        //            {
        //                var v = (Decimal)current - (Decimal)value;
        //                if (v < 0)
        //                {
        //                    v *= -1;
        //                    sign = false;
        //                }
        //                value = v;
        //            }
        //            break;
        //        default:
        //            break;
        //    }

        //    return true;
        //}

        ///// <summary>清除累加字段数据。Update后调用该方法</summary>
        //public void ClearValues()
        //{
        //    var df = _Additions;
        //    if (df == null) return;

        //    foreach (var item in df.Keys.ToArray())
        //    {
        //        df[item] = Entity[item];
        //    }
        //}
        #endregion

        #region 静态
        public static IList<IEntity> SetField(IList<IEntity> list)
        {
            if (list == null || list.Count < 1) return list;

            var entityType = list[0].GetType();
            var factory = EntityFactory.CreateOperate(entityType);
            var fs = factory.AdditionalFields;
            if (fs.Count > 0)
            {
                foreach (EntityBase entity in list)
                {
                    if (entity != null) entity.Addition.Set(fs);
                }
            }

            return list;
        }

        public static void SetField(EntityBase entity)
        {
            if (entity == null) return;

            var factory = EntityFactory.CreateOperate(entity.GetType());
            var fs = factory.AdditionalFields;
            if (fs.Count > 0) entity.Addition.Set(fs);
        }

        //public static void ClearValues(EntityBase entity)
        //{
        //    if (entity == null) return;

        //    entity.Addition.ClearValues();
        //}

        ///// <summary>尝试获取累加数据</summary>
        ///// <param name="entity">实体对象</param>
        ///// <param name="name">字段名称</param>
        ///// <param name="value">累加数据绝对值</param>
        ///// <param name="sign">正负</param>
        ///// <returns>是否获取指定字段的累加数据</returns>
        //public static Boolean TryGetValue(EntityBase entity, String name, out Object value, out Boolean sign)
        //{
        //    value = null;
        //    sign = false;

        //    if (entity == null) return false;

        //    return entity.Addition.TryGetValue(name, out value, out sign);
        //}
        #endregion
    }
}