namespace Skyline.Protocol
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;

	using SLNetMessages = Skyline.DataMiner.Net.Messages;

	namespace ProtocolHelpers
	{
		public static class ProtocolExtensions
		{
			public static IEnumerable<string> GetCellByValue(this SLProtocolExt protocol, int tablePid, uint columnIdxToReturn, uint columnIdxToCompare, string compareValue)
			{
				var columnsIdx = new[]
				{
					columnIdxToReturn,
					columnIdxToCompare,
				};

				object[] columnsRawData = protocol.GetColumns(tablePid, columnsIdx);
				object[] col1Data = (object[])columnsRawData[0];
				object[] col2Data = (object[])columnsRawData[1];

				for (int rowNumber = 0; rowNumber < col1Data.Length; rowNumber++)
				{
					string col1CellValue = Convert.ToString(col1Data[rowNumber]);
					string col2CellValue = Convert.ToString(col2Data[rowNumber]);
					if (col2CellValue.Equals(compareValue))
					{
						yield return col1CellValue;
					}
				}
			}

			private static object[] GetColumns(this SLProtocol protocol, int tablePid, uint[] columnsIdx)
			{
				object[] columnsRawData = (object[])protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, tablePid, columnsIdx);
				if (columnsRawData == null || columnsRawData.Length != columnsIdx.Length)
				{
					throw new ArgumentException($"Retrieving data from table {tablePid} failed: Invalid data returned from SLProtocol.");
				}

				return columnsRawData;
			}
		}
	}
}