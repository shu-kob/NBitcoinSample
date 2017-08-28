using System;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace NBitcoinFirst
{
	class Program
	{
		static void Main(string[] args)
		{
			var coin = new Coin(
				fromTxHash: new uint256("e2b308ea38112ae79ff2cc67d686c3dcd737214290802c1905b76e76c89783d4"),
				fromOutputIndex: 0,
				amount: Money.Satoshis(300000000),
				scriptPubKey: new Script(Encoders.Hex.DecodeData("76a9141c770f57182260756319ca5e0af31e19b37288b288ac"))
			);

			var issuance = new IssuanceCoin(coin);

			var nico = BitcoinAddress.Create("mwU3UJ1VXX3GxKKQHdscw1TvWSrMKdtymR");
			var bookKey = new BitcoinSecret("cRETrCgfbU273XmpFDQr4GhsqU4cXB7ECQuoGtCrRmgy1U2jLu1f");
			TransactionBuilder builder = new TransactionBuilder();

			var tx = builder
				.AddKeys(bookKey)
				.AddCoins(issuance)
				.IssueAsset(nico, new AssetMoney(issuance.AssetId, quantity: 21000000))
				.SendFees(Money.Coins(0.001m))
				.SetChange(bookKey.GetAddress())
				.BuildTransaction(true);

			Console.WriteLine(tx);
			Console.WriteLine(builder.Verify(tx));

			var client = new QBitNinjaClient(Network.TestNet);
			BroadcastResponse broadcastResponse = client.Broadcast(tx).Result;

			if (!broadcastResponse.Success)
			{
				Console.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
				Console.WriteLine("Error message: " + broadcastResponse.Error.Reason);
			}
			else
			{
				Console.WriteLine("Success!");
			}

		}
	}
}
