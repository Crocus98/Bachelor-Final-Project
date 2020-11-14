﻿using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Oracle888730.Contracts.Oracle888730.ContractDefinition;
using System;
using System.Threading;
using System.Threading.Tasks;
using Oracle888730.Utility;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Oracle888730.Classes
{
    class RequestListener : GenericListener
    {
        public RequestListener(Web3 _web3, Config _config) : base(_web3, _config)
        {
            message = "[RequestListener]";
        }

        protected override void Listener()
        {
            try
            {
                Event requestEvent = GetRequestEvent();
                HexBigInteger latestBlock = RetrieveLatestBlockToRead(requestEvent);
                StringWriter.Enqueue(message + " Listener started");
                //INIZIO LOOP 
                while (true)
                {
                    var changes = requestEvent.GetFilterChanges<RequestEventEventDTO>(latestBlock);
                    changes.Wait();
                    if (changes.Result.Count > 0)
                    {
                        changes.Result.ForEach(request =>
                        {
                            new RequestHandler(web3, config, request).Start();
                        });
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        private HexBigInteger RetrieveLatestBlockToRead(Event _requestEvent)
        {
            HexBigInteger latestBlock;
            if (config.Oracle.LatestBlock == null)
            {
                var filter = _requestEvent.CreateFilterAsync();
                filter.Wait();
                latestBlock = filter.Result;
                config.Oracle.LatestBlock = latestBlock.HexValue;
                config.Save();
            }
            else
            {
                latestBlock = new HexBigInteger(config.Oracle.LatestBlock);
            }
            return latestBlock;
        }

        private Event GetRequestEvent()
        {
            StringWriter.Enqueue(message + " Listener setup started...");
            var contract = web3.Eth.GetContract(
                config.Oracle.Abi,
                config.Oracle.ContractAddress
            );
            Event requestEvent = contract.GetEvent("RequestEvent");
            return requestEvent;
        }

        protected override void ExceptionHandler(Task _taskRequestListener)
        {
            var exception = _taskRequestListener.Exception;
            Console.WriteLine(message+"[ERROR] " + exception.Message + ". Program closing...");
            Config.Exit();
        }
    }

}