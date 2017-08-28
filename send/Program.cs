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
                fromTxHash: new uint256("668c63369dfb6f6ebcd07aa36a4f1e9f252f8b3f976fcc84d540e03e54cdcc0a"),
	            fromOutputIndex: 0,
	            amount: Money.Satoshis(2730),
	            scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914aef515f8874638687187da27bd008c3e7f5c68c188ac")));
            BitcoinAssetId assetId = new BitcoinAssetId("oPvD7Nn3eKHkWkwwG4i2HRjVJEhv4d6SEW");
			ColoredCoin colored = coin.ToColoredCoin(assetId, 21000000);

			var book = BitcoinAddress.Create("mi7TqrUCAKZ3pFuRaLZvFWhjrktbcyZf54");
			var nicoSecret = new BitcoinSecret("cUQo9VhkwZ8zMUJA8cFEY5MLHiQJMhcGKk9SUbRVDCB3yveUq66z");
			var nico = nicoSecret.GetAddress(); //mwU3UJ1VXX3GxKKQHdscw1TvWSrMKdtymR

			var forFees = new Coin(
				fromTxHash: new uint256("a0c35a09e52af1a116d1e1233b952511ca7cae3aef5e0ea888b77167cb22e0fc"),
				fromOutputIndex: 0,
				amount: Money.Satoshis(300000000),
				scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914aef515f8874638687187da27bd008c3e7f5c68c188ac")));

			TransactionBuilder builder = new TransactionBuilder();
			var tx = builder
				.AddKeys(nicoSecret)
				.AddCoins(colored, forFees)
				.SendAsset(book, new AssetMoney(assetId, 100000))
				.SetChange(nico)
				.SendFees(Money.Coins(0.001m))
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
