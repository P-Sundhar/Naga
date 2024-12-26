using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SalesReturn.Models;

namespace SalesReturn.Services
{
    public class OrderService
    {
        private RequestProvider requestProvider;
        public OrderService(RequestProvider _requestProvider)
        {
            requestProvider = _requestProvider;
            requestProvider.SetBaseUrl(Globals.ServerUrl);
        }
        public async Task<PlantResponse> Login(string url, LoginModel lgnreq)
        {
            var data = await requestProvider.PostAsync(url, lgnreq);
            return await Task.FromResult(JsonConvert.DeserializeObject<PlantResponse>(data));
        }

        public async Task<PlantResponse> GetPlantDetails(string url)
        {
            var data = await requestProvider.GetAsync(url);
            return await Task.FromResult(JsonConvert.DeserializeObject<PlantResponse>(data));
        }

        public async Task<OutletResponse> GetOutlet(string url)
        {
            var data = await requestProvider.GetAsync(url);
            return await Task.FromResult(JsonConvert.DeserializeObject<OutletResponse>(data));
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

        public async Task<SalesReturnResponse> GetScannedDetails(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<SalesReturnResponse>(data));
        }

        public async Task<ResponseModel> SaveSalesReturn(string url, BarcodeValidate reqdtls)
        {
            var data = await requestProvider.PostAsync(url, reqdtls);
            return await Task.FromResult(JsonConvert.DeserializeObject<ResponseModel>(data));
        }
    }
}
