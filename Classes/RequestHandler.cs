﻿using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Oracle888730.Utility;
using System;
using System.Threading.Tasks;
using Oracle888730.Contracts.Oracle888730.ContractDefinition;
using Oracle888730.OracleEF;
using Oracle888730.OracleEF.Models;
using System.Numerics;

namespace Oracle888730.Classes
{
    class RequestHandler : GenericHandler<RequestEventEventDTO>
    {
 
        public RequestHandler(Web3 _web3, Config _config, EventLog<RequestEventEventDTO> _eventLog) : base (_web3, _config,  _eventLog)
        {
            message = "[RequestHandler]";
        }

        protected override void Handler()
        {
            int requestType = (int)handledEventLog.Event.RequestType;
            string address = handledEventLog.Event.Sender;
            var value = callApiHelper.GetWantedValue(requestType);
            value.Wait();

            //Using the same instance of Web3 across services, there is a nonce manager (in memory) that will ensure that your transactions are in the right order (safe thread too)
            var res = contractService.SendResponseRequestAndWaitForReceiptAsync(
                    clientAddress: address,
                    value: value.Result,
                    requestType: requestType
                );
            StringWriter.Enqueue(message + " Richiesta: " + requestType + " Da: " + address + " completata!");
        }

        protected override void ExceptionHandler(Task _taskHandler)
        {
            var exception = _taskHandler.Exception;
            Console.WriteLine(message + "[ERROR] " + exception.Message);
        }
    }
}
