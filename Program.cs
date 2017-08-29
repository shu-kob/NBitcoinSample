using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
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
            // 実際はAPIでJSONを受け取る           
            string jsonString = @"{""commandType"":""issuance"",""fromTxHash"":""ce56e1d60efe0f5a3d93b837ce208f559214a5ec10cb9715ac0357475ae72576"",""fromOutputIndex"":""1"",""amount"":""100000000"",""scriptPubkey"":""76a9146255517104577282389fdce86d4e9e67f796759e88ac"",""bitcoinAddress"":""mpUtirtqBzQXuH9MRw3u1YgMFhBqRhknqu"",""bitcoinSecret"":""cTxQtwjYch3uscAPDUyUd2ZkcuMDTY3dp7X2HEvXoVzsFZiLFKYX"",""quantity"":""100000000""}";
            // string jsonString = @"{""commandType"":""send"",""fromTxHash"":""5dd0250910238a134c19a6ae5867cb239754b4d69d1a0f5589b29afba55b8315"",""fromOutputIndex"":""0"",""amount"":""2730"",""scriptPubkey"":""76a9146255517104577282389fdce86d4e9e67f796759e88ac"",""issuranceAddress"":""mpUtirtqBzQXuH9MRw3u1YgMFhBqRhknqu"",""balanceQuantity"":""100000000"",""bitcoinAddress"":""myMmSWRcRvrKPiQioF6QLfhkNkn1Krsz4J"",""bitcoinSecret"":""cTxQtwjYch3uscAPDUyUd2ZkcuMDTY3dp7X2HEvXoVzsFZiLFKYX"",""feeFromTxHash"":""5dd0250910238a134c19a6ae5867cb239754b4d69d1a0f5589b29afba55b8315"",""feeFromOutputIndex"":""1"",""feeAmount"":""99987270"",""feeScriptPubkey"":""76a9146255517104577282389fdce86d4e9e67f796759e88ac"",""quantity"":""1""}";
            // string jsonString = @"{""commandType"":""other""}";

            JsonHandler.PersonData pd = (JsonHandler.PersonData)JsonHandler.getObjectFromJson(
                jsonString,
                typeof(JsonHandler.PersonData)
            );

            Console.WriteLine(jsonString);

            if (pd.commandType.Equals("issuance"))
            {
                var coin = new Coin(
                    fromTxHash: new uint256(pd.fromTxHash),
                    fromOutputIndex: pd.fromOutputIndex,
                    amount: Money.Satoshis(pd.amount),
                    scriptPubKey: new Script(Encoders.Hex.DecodeData(pd.scriptPubkey)));

                var issuance = new IssuanceCoin(coin);

                var receiveAddress = BitcoinAddress.Create(pd.bitcoinAddress);
                var bookKey = new BitcoinSecret(pd.bitcoinSecret);
                TransactionBuilder builder = new TransactionBuilder();

                var tx = builder
                    .AddKeys(bookKey)
                    .AddCoins(issuance)
                    .IssueAsset(receiveAddress, new AssetMoney(issuance.AssetId, quantity: pd.quantity))
                    .SendFees(Money.Coins(0.0001m))
                    .SetChange(bookKey.GetAddress())
                    .BuildTransaction(true);

                System.Diagnostics.Debug.WriteLine(tx);
                Console.WriteLine(tx);

                System.Diagnostics.Debug.WriteLine(builder.Verify(tx));
                Console.WriteLine(builder.Verify(tx));

                System.Diagnostics.Debug.WriteLine(issuance.AssetId);
                Console.WriteLine(issuance.AssetId);

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

                /* 連結ではbitcoindでのブロードキャストを行う
                using (var node = Node.ConnectToLocal(Network.TestNet)) //Connect to the node
                {
                    node.VersionHandshake(); //Say hello
                                             //Advertize your transaction (send just the hash)
                    node.SendMessage(new InvPayload(InventoryType.MSG_TX, tx.GetHash()));
                    //Send it
                    node.SendMessage(new TxPayload(tx));
                    Thread.Sleep(500); //Wait a bit
                }
                */

            }
            else if (pd.commandType.Equals("send"))
            {
                var booka = BitcoinAddress.Create(pd.issuranceAddress);
                // System.Diagnostics.Debug.WriteLine(booka.ToColoredAddress());
                // Console.WriteLine(booka.ToColoredAddress());
                var assetId = new AssetId(booka).GetWif(Network.TestNet);
                System.Diagnostics.Debug.WriteLine(assetId);
                Console.WriteLine(assetId);

                var coin = new Coin(
                    fromTxHash: new uint256(pd.fromTxHash),
                    fromOutputIndex: pd.fromOutputIndex,
                    amount: Money.Satoshis(2730),
                    scriptPubKey: new Script(Encoders.Hex.DecodeData(pd.scriptPubkey)));
                // BitcoinAssetId assetId = new BitcoinAssetId(assetId);
                ColoredCoin colored = coin.ToColoredCoin(assetId, pd.balanceQuantity);

                var book = BitcoinAddress.Create(pd.bitcoinAddress);
                var sendSecret = new BitcoinSecret(pd.bitcoinSecret);
                var sendAddress = sendSecret.GetAddress();

                var forFees = new Coin(
                    fromTxHash: new uint256(pd.feeFromTxHash),
                    fromOutputIndex: pd.feeFromOutputIndex,
                    amount: Money.Satoshis(pd.feeAmount),
                    scriptPubKey: new Script(Encoders.Hex.DecodeData(pd.feeScriptPubkey)));

                TransactionBuilder builder = new TransactionBuilder();
                var tx = builder
                    .AddKeys(sendSecret)
                    .AddCoins(colored, forFees)
                    .SendAsset(book, new AssetMoney(assetId, pd.quantity))
                    .SetChange(sendAddress)
                    .SendFees(Money.Coins(0.0001m))
                    .BuildTransaction(true);
                System.Diagnostics.Debug.WriteLine(tx);
                Console.WriteLine(tx);
                System.Diagnostics.Debug.WriteLine(builder.Verify(tx));
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

                /* 連結ではbitcoindでのブロードキャストを行う
                using (var node = Node.ConnectToLocal(Network.TestNet)) //Connect to the node
                {
                    node.VersionHandshake(); //Say hello
                                             //Advertize your transaction (send just the hash)
                    node.SendMessage(new InvPayload(InventoryType.MSG_TX, tx.GetHash()));
                    //Send it
                    node.SendMessage(new TxPayload(tx));
                    Thread.Sleep(500); //Wait a bit
                }
                */
            }
            else
            {
                Console.WriteLine("CommandTypeError");
            }

            Console.ReadKey();

        }
        public class JsonHandler
        {
            /// <summary>
            /// JSON値をobjectにして返却
            /// </summary>
            /// <param name="jsonString"></param>
            /// <param name="t"></param>
            /// <returns></returns>
            public static object getObjectFromJson(string jsonString, Type t)
            {
                var serializer = new DataContractJsonSerializer(t);
                var jsonBytes = Encoding.Unicode.GetBytes(jsonString);
                var sr = new MemoryStream(jsonBytes);
                return serializer.ReadObject(sr);
            }

            /// <summary>
            /// JSONから受け取ったPersonDataの構造体
            /// </summary>
            [DataContract]
            public class PersonData
            {
                [DataMember]
                public string commandType { get; set; }
                [DataMember]
                public string fromTxHash { get; set; }
                [DataMember]
                public uint fromOutputIndex { get; set; }
                [DataMember]
                public int amount { get; set; }
                [DataMember]
                public string scriptPubkey { get; set; }
                [DataMember]
                public string bitcoinAddress { get; set; }
                [DataMember]
                public string bitcoinSecret { get; set; }
                [DataMember]
                public int quantity { get; set; }
                [DataMember]
                public string issuranceAddress { get; set; }
                [DataMember]
                public uint balanceQuantity { get; set; }
                [DataMember]
                public string feeFromTxHash { get; set; }
                [DataMember]
                public uint feeFromOutputIndex { get; set; }
                [DataMember]
                public int feeAmount { get; set; }
                [DataMember]
                public string feeScriptPubkey { get; set; }
            }
        }
    }
}
