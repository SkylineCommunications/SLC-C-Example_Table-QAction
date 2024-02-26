namespace Skyline.Protocol
{
	using System;

	using Skyline.DataMiner.Scripting;

	namespace OrderHelper
	{
		public class OrderHelper
		{
			public enum Article
			{
				Tea = 1,
				Coffee = 2,
				Soup = 3
			}

			public enum UserRequest
			{
				Add = 1,
				OneOfEach = 2,
				DeleteArticles = 3
			}

			public static OrderData ReadNewOrderFromProtocol(SLProtocol protocol)
			{
				uint[] pids = new uint[]
				{
					Parameter.selectedarticle_11,
					Parameter.selectednumberofarticles_12,
				};

				object[] data = (object[])protocol.GetParameters(pids);

				if (Convert.ToInt32(data[0]) == 0 || Convert.ToInt32(data[1]) == 0)
				{
					throw new ArgumentException("Selected order data is not valid.");
				}

				OrderData order = new OrderData
				{
					Article = ((Article)Convert.ToInt32(data[0])).ToString(),
					NumberOfArticles = Convert.ToInt32(data[1]),
				};

				return order;
			}

			public static Article ReadArticleNameToDeleteFromProtocol(SLProtocol protocol)
			{
				int article = Convert.ToInt32(protocol.GetParameter(Parameter.selectedarticletodelete_21));
				if (article == 0)
				{
					throw new ArgumentException("Article name to delete is not filled in.");
				}

				return (Article)article;
			}
		}
	}
}