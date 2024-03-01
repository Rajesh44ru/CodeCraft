using LogicLync.DTO;
using LogicLync.Service.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LogicLync.Service
{
    public class HttpClientService:IHttpClientService
    {
        public HttpClientService()
        {

        }


        public async Task<bool> CallPostMethodAPI(HttpClientDTO model)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.Token);
                var json = JsonConvert.SerializeObject(model.Model);
                //var stringContent = new StringContent(json);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(model.ApiWebHost + model.APIUrl, stringContent);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

// Export TO Excel

        public List<Acc_AccountDTO> get()
        {
            List<Acc_AccountDTO> accounts = (from a in _accountRepository.GetAll(a =>a.IsDeleted != true && a.IsActive != false,a => a.AccountTypes).ToList()
                                         select new Acc_AccountDTO()
                                         {
                                             Id = a.Id,
                                             Name = a.Name,
                                             Description = a.Description,
                                             IsActive = a.IsActive ?? false,
                                             IsVisible = a.IsVisible,
                                             AccountTypes = (from at in a.AccountTypes
                                                             where at.IsDeleted != true
                                                             select new Acc_AccountTypeDTO()
                                                             {
                                                                 Id = at.Id,
                                                                 Name = at.Name,
                                                                 Description = at.Description,
                                                                 EnableSubAccount = at.EnableSubAccount,
                                                                 EnableAccountNumber = at.EnableAccountNumber,
                                                                 EnableCurrency = at.EnableCurrency
                                                             }).ToList()
                                         }).ToList();
            return accounts;
        }
