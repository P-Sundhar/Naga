using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StockAudit.Models;

namespace StockAudit.Services
{
    public class OrderService
    {
        private RequestProvider requestProvider;
        public OrderService(RequestProvider _requestProvider)
        {
            requestProvider = _requestProvider;
            requestProvider.SetBaseUrl(Globals.ServerUrl);
        }
        public async Task<ResponseModel> Login(string url, LoginModel lgnreq)
        {
            var data = await requestProvider.PostAsync(url, lgnreq);
            return await Task.FromResult(JsonConvert.DeserializeObject<ResponseModel>(data));
        }

        public async Task<PlantResponse> GetPlantDetails(string url)
        {
            var data = await requestProvider.GetAsync(url);
            return await Task.FromResult(JsonConvert.DeserializeObject<PlantResponse>(data));
        }

        public async Task<ResponseModel> ValidateCrate(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<ResponseModel>(data));
        }

        public async Task<ResponseModel> ValidateBarcode(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<ResponseModel>(data));
        }

        public async Task<StockAuditResponse> GetScannedDetails(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<StockAuditResponse>(data));
        }

        public async Task<ResponseModel> SaveStockAudit(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<ResponseModel>(data));
        }
    }
}
