using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Tours.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentResults;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;



namespace Explorer.API.Controllers.Administrator.Administration
{
    //[Authorize(Policy = "administratorPolicy")]
    [Route("api/administration/accounts")]
    public class AccountManagementController : BaseApiController
    {
        private readonly IAccountManagementService _accountManagementService;

        public AccountManagementController(IAccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }

        [HttpGet]
        /* public ActionResult<List<AccountDto>> GetAllAccounts()
         {
             var result = _accountManagementService.GetAllAccounts();
             return CreateResponse(result);
         }*/
        public async Task<ActionResult<List<AccountDto>>> GetAllAccounts()
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8083/");

            try
            {
                var response = await httpClient.GetAsync("users/getAll");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Result<List<AccountDto>> accounts = JsonConvert.DeserializeObject<List<AccountDto>>(content);
                    return CreateResponse(accounts);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve accounts from the other app.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while communicating with the other app: " + ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        /* public ActionResult<AccountDto> BlockOrUnblock([FromBody] AccountDto account)
         {
             var result = _accountManagementService.BlockOrUnblock(account);
             return CreateResponse(result);
         }*/
        public async Task<ActionResult<AccountDto>> BlockOrUnblock([FromBody] AccountDto account)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8083/");

            try
            {
                var jsonContent = JsonConvert.SerializeObject(account);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync("users/block", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    Result<AccountDto> updatedAccount = JsonConvert.DeserializeObject<AccountDto>(responseContent);

                    return CreateResponse(updatedAccount);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to block/unblock account in the other app.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while communicating with the other app: " + ex.Message);
            }
        }

    }
}

