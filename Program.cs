using System;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace NBitcoin
{
    class Program
    {
        static void Main(string[] args)
        {
			var coin = new Coin(
	        fromTxHash: new uint256("c8271ba7118308d6b3205475048d103887fe797b63393ebc1f7910abb9907d29"),
	        fromOutputIndex: 1,
	        amount: Money.Satoshis(12998727),
	        scriptPubKey: new Script(Encoders.Hex.DecodeData("76a914879d64086263c36d8bc1739471786b440e4e8b7488ac")));

			var issuance = new IssuanceCoin(coin);

			var nico = BitcoinAddress.Create("mst24ennQABZLfgr5HJC3GgzBbn5v4NinL");
			var bookKey = new BitcoinSecret("cU8oQX6oazd9RFZ8t4EppdQX8BMj6icfCEG6CfVLXnMPFGTQmPZi");
			TransactionBuilder builder = new TransactionBuilder();

			var tx = builder
				.AddKeys(bookKey)
				.AddCoins(issuance)
				.IssueAsset(nico, new AssetMoney(issuance.AssetId, quantity: 25000000))
				.SendFees(Money.Coins(0.0001m))
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
