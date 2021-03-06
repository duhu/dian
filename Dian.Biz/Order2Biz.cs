﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Dian.Common.Entity;
using Dian.Common.Interface;
using Dian.Dao;
using CSN.DotNetLibrary.EntityExpressions.Entitys;
using System.Transactions;

namespace Dian.Biz
{
    public class Order2Biz : System.MarshalByRefObject, IOrder2
    {
        private DianManual _manual_dao = null;
        public DianManual manual_dao
        {
            get
            {
                return _manual_dao == null ? _manual_dao = new DianManual() : _manual_dao;
            }
        }

        #region 扩展方法

        public DataTable GetOrderData(int orderId)
        {
            return manual_dao.GetOrderData(orderId);
        }

        public int CreateOrder(int restaurantId, int tableId, decimal price, List<OrderListEntity2> listOrderList)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                var orderMainEntity = new OrderMainEntity2();
                orderMainEntity.RESTAURANT_ID = restaurantId;
                orderMainEntity.TABLE_ID = tableId;
                orderMainEntity.PRICE = price;
                orderMainEntity.ORDER_FLAG = "1";
                var orderId = InsertOrderMainEntity(orderMainEntity);

                foreach (var orderList in listOrderList)
                {
                    orderList.ORDER_ID = orderId;
                    orderList.ORDER_FLAG = "1";
                    orderList.ORDER_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss");
                    InsertOrderListEntity(orderList);
                }

                ts.Complete();

                return orderId;
            }
        }

        public void UpdateOrder(int orderId, decimal price, string foodOp, OrderListEntity2 entity)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                //更新主表价格
                var orderMainEntity = new OrderMainEntity2();
                orderMainEntity.ORDER_ID = orderId;
                orderMainEntity.PRICE = price;
                UpdateOrderMainEntity(orderMainEntity);

                //更新子表明细
                var dt = manual_dao.GetUnConfirmOrderData(orderId, entity.FOOD_ID);
                if (foodOp == "add")
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var condition = new OrderListEntity2();
                        condition.LIST_ID = int.Parse(dt.Rows[0]["LIST_ID"].ToString());
                        condition.COUNT = int.Parse(dt.Rows[0]["COUNT"].ToString()) + 1;
                        UpdateOrderListEntity(condition);
                    }
                    else
                    {
                        entity.ORDER_ID = orderId;
                        entity.COUNT = 1;
                        entity.ORDER_FLAG = "1";
                        entity.ORDER_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss");
                        InsertOrderListEntity(entity);
                    }
                }
                else if (foodOp == "cut")
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var count = int.Parse(dt.Rows[0]["COUNT"].ToString());
                        if (count <= 1)
                        {
                            var condition = new OrderListEntity2();
                            condition.LIST_ID = int.Parse(dt.Rows[0]["LIST_ID"].ToString());
                            DeleteOrderListEntity(condition);
                        }
                        else
                        {
                            var condition = new OrderListEntity2();
                            condition.LIST_ID = int.Parse(dt.Rows[0]["LIST_ID"].ToString());
                            condition.COUNT = count - 1;
                            UpdateOrderListEntity(condition);
                        }
                    }
                }
                else if (foodOp == "remark")
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var condition = new OrderListEntity2();
                        condition.LIST_ID = int.Parse(dt.Rows[0]["LIST_ID"].ToString());
                        condition.TASTE = entity.TASTE;
                        condition.REMARK = entity.REMARK;
                        UpdateOrderListEntity(condition);
                    }
                }

                ts.Complete();
            }

        }

        public void ClearCart(int orderId)
        {
            var dt = manual_dao.GetUnConfirmOrderData(orderId);
            if (dt != null && dt.Rows.Count > 0)
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var condition = new OrderListEntity2();
                        condition.LIST_ID = int.Parse(dr["LIST_ID"].ToString());
                        DeleteOrderListEntity(condition);
                    }
                    ts.Complete();
                }
            }
        }

        public void BatchProcessOrder(int orderId, string opreation)
        {
            manual_dao.BatchProcessOrder(orderId, opreation);
        }


        #endregion

        #region  基础方法

        public DataTable GetOrderMainDataTable(int? restaurantId = null, string type = null)
        {
            return manual_dao.GetOrderMainDataTable(restaurantId, type);
        }

        public List<OrderMainEntity2> GetOrderMainEntityList(OrderMainEntity2 condition_entity)
        {
            GenericWhereEntity<OrderMainEntity2> where_entity = new GenericWhereEntity<OrderMainEntity2>();
            if (condition_entity.ORDER_ID != null)
                where_entity.Where(n => (n.ORDER_ID == condition_entity.ORDER_ID));
            if (condition_entity.RESTAURANT_ID != null)
                where_entity.Where(n => (n.RESTAURANT_ID == condition_entity.RESTAURANT_ID));
            if (condition_entity.TABLE_ID != null)
                where_entity.Where(n => (n.TABLE_ID == condition_entity.TABLE_ID));
            if (condition_entity.ORDER_FLAG != null)
                where_entity.Where(n => (n.ORDER_FLAG == condition_entity.ORDER_FLAG));
            return DianDao.ReadEntityList(where_entity);
        }

        public int InsertOrderMainEntity(OrderMainEntity2 condition_entity)
        {
            var result = DianDao.InsertEntityWithIdentity(condition_entity);
            return int.Parse(result.ToString());
        }

        public void UpdateOrderMainEntity(OrderMainEntity2 condition_entity)
        {
            DianDao.UpdateEntity(condition_entity);
        }

        public void DeleteOrderMainEntity(OrderMainEntity2 condition_entity)
        {
            DianDao.DeleteEntity(condition_entity);
        }

        public OrderMainEntity2 GetOrderMainEntity(int? id)
        {
            return DianDao.ReadEntity2<OrderMainEntity2>(n => n.ORDER_ID == id);
        }



        public DataTable GetOrderListDataTable(int? restaurantId = null)
        {
            return manual_dao.GetOrderListDataTable();
        }

        public List<OrderListEntity2> GetOrderListEntityList(OrderListEntity2 condition_entity)
        {
            GenericWhereEntity<OrderListEntity2> where_entity = new GenericWhereEntity<OrderListEntity2>();
            if (condition_entity.LIST_ID != null)
                where_entity.Where(n => (n.LIST_ID == condition_entity.LIST_ID));
            if (condition_entity.ORDER_ID != null)
                where_entity.Where(n => (n.ORDER_ID == condition_entity.ORDER_ID));
            if (condition_entity.FOOD_ID != null)
                where_entity.Where(n => (n.FOOD_ID == condition_entity.FOOD_ID));
            return DianDao.ReadEntityList(where_entity);
        }

        public void InsertOrderListEntity(OrderListEntity2 condition_entity)
        {
            DianDao.InsertEntity(condition_entity);
        }

        public void UpdateOrderListEntity(OrderListEntity2 condition_entity)
        {
            DianDao.UpdateEntity(condition_entity);
        }

        public void DeleteOrderListEntity(OrderListEntity2 condition_entity)
        {
            DianDao.DeleteEntity(condition_entity);
        }


        public OrderListEntity2 GetOrderListEntity(int? id)
        {
            return DianDao.ReadEntity2<OrderListEntity2>(n => n.LIST_ID == id);
        }

        #endregion

        public int TestCallAble()
        {
            throw new NotImplementedException();
        }

    }
}
