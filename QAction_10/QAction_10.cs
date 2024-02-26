using System;
using System.Collections.Generic;
using System.Linq;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.OrderHelper;
using Skyline.Protocol.ProtocolHelpers;

/// <summary>
/// DataMiner QAction Class: Orders Table - Add or Delete Articles.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocolExt protocol)
	{
		bool useProtocolExtended = false;

		try
		{
			int selectedButtonParamId = Convert.ToInt32(protocol.GetTriggerParameter());
			OrderHelper.UserRequest userRequest = (OrderHelper.UserRequest)Convert.ToInt32(protocol.GetParameter(selectedButtonParamId));

			switch (userRequest)
			{
				case OrderHelper.UserRequest.Add:
					var order = OrderHelper.ReadNewOrderFromProtocol(protocol);
					AddArticle(protocol, order, useProtocolExtended);
					break;
				case OrderHelper.UserRequest.OneOfEach:
					AddOneOfEach(protocol, useProtocolExtended);
					break;
				case OrderHelper.UserRequest.DeleteArticles:
					var article = OrderHelper.ReadArticleNameToDeleteFromProtocol(protocol);
					DeleteArticles(protocol, article, useProtocolExtended);
					break;
				default:
					protocol.Log($"QA{protocol.QActionID}|Run|Unexpected userRequest '{userRequest}'", LogType.Error, LogLevel.NoLogging);
					break;
			}
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
			protocol.ShowInformationMessage($"Exception: {ex.Message}");
		}
	}

	/// <summary>
	/// Add an article to the orders.
	/// </summary>
	/// <param name="protocol">Link to SLProtocol Process.</param>
	/// <param name="orderData">The new article item to be added.</param>
	/// <param name="useProtocolExtended">Indication to use the extended protocol methods or not.</param>
	public static void AddArticle(SLProtocolExt protocol, OrderData orderData, bool useProtocolExtended)
	{
		OrdersQActionRow myOrder = new OrdersQActionRow
		{
			Ordersinstance_1001 = Guid.NewGuid().ToString(),
			Ordersarticlename_1002 = orderData.Article,
			Ordersarticlenumber_1003 = orderData.NumberOfArticles,
			Orderstimeoforder_1004 = DateTime.Now.ToOADate(),
		};

		if (!useProtocolExtended)
		{
			if (!protocol.Exists(Parameter.Orders.tablePid, myOrder.Key))
			{
				// Add the order row via Protocol using the table parameter Id.
				protocol.AddRow(Parameter.Orders.tablePid, myOrder.ToObjectArray());
			}
			else
			{
				// Update the row via Protocol using the table parameter Id.
				protocol.SetRow(Parameter.Orders.tablePid, myOrder.Key, myOrder.ToObjectArray());
			}
		}
		else
		{
			// Add or Update row via Protocol Extended.
			protocol.orders.SetRow(myOrder, true);
		}
	}

	/// <summary>
	/// Add one of each article to the orders.
	/// </summary>
	/// <param name="protocol">Link to SLProtocol Process.</param>
	/// <param name="useProtocolExtended">Indication to use the extended protocol methods or not.</param>
	public static void AddOneOfEach(SLProtocolExt protocol, bool useProtocolExtended)
	{
		if (!useProtocolExtended)
		{
			AddOneOfEach(protocol);
		}
		else
		{
			AddOneOfEachExtended(protocol);
		}
	}

	/// <summary>
	/// Delete all articles using a specific article name.
	/// </summary>
	/// <param name="protocol">Link to SLProtocol.</param>
	/// <param name="article">The article name that needs to be deleted.</param> 
	/// <param name="useProtocolExtended">Indication to use the extended protocol methods or not.</param>
	public static void DeleteArticles(SLProtocolExt protocol, OrderHelper.Article article, bool useProtocolExtended)
	{
		var matchingRowKeys = protocol
			.GetCellByValue(Parameter.Orders.tablePid, Parameter.Orders.Idx.ordersinstance_1001, Parameter.Orders.Idx.ordersarticlename_1002, article.ToString())
			.ToArray();

		if (!matchingRowKeys.Any())
		{
			protocol.Log($"QA{protocol.QActionID}|DeleteArticles|No '{article}' order to delete.", LogType.DebugInfo, LogLevel.NoLogging);
			return;
		}

		if (!useProtocolExtended)
		{
			// Delete rows via Protocol.
			protocol.DeleteRow(Parameter.Orders.tablePid, matchingRowKeys);
		}
		else
		{
			// Delete rows via Protocol Extended.
			protocol.orders.DeleteRow(matchingRowKeys);
		}
	}

	private static void AddOneOfEach(SLProtocol protocol)
	{
		List<object[]> allOrders = new List<object[]>
		{
			FillOrder(OrderHelper.Article.Tea),
			FillOrder(OrderHelper.Article.Coffee),
			FillOrder(OrderHelper.Article.Soup),
		};

		// Append the orders table with this info.
		protocol.FillArray(Parameter.Orders.tablePid, allOrders, NotifyProtocol.SaveOption.Partial);

		// Overwrite the full orders table.
		////protocol.FillArray(Parameter.Orders.tablePid, allOrders, NotifyProtocol.SaveOption.Full);
	}

	private static void AddOneOfEachExtended(SLProtocolExt protocol)
	{
		List<OrdersQActionRow> allOrders = new List<OrdersQActionRow>
		{
			FillOrder(OrderHelper.Article.Tea),
			FillOrder(OrderHelper.Article.Coffee),
			FillOrder(OrderHelper.Article.Soup),
		};

		// Append the orders table with this info.
		protocol.orders.FillArrayNoDelete(allOrders.ToArray());

		// Overwrite the full orders table.
		////protocol.orders.FillArray(allOrders.ToArray());
	}

	private static OrdersQActionRow FillOrder(OrderHelper.Article article)
	{
		OrdersQActionRow order = new OrdersQActionRow
		{
			Ordersinstance_1001 = Guid.NewGuid().ToString(),
			Ordersarticlename_1002 = article.ToString(),
			Ordersarticlenumber_1003 = 1,
			Orderstimeoforder_1004 = DateTime.Now.ToOADate(),
		};

		return order;
	}
}