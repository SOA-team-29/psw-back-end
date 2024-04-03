using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Explorer.Encounters.Core.UseCases;
using Explorer.Stakeholders.Core.Domain;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Explorer.API.Controllers.Tourist.EncounterExecution
{
    //[Authorize(Policy = "touristPolicy")]
    [Route("api/encounterExecution")]
    public class EncounterExecutionController : BaseApiController
    {
        private readonly IEncounterExecutionService _encounterExecutionService;
        private readonly ISocialEncounterService _socialEncounterService;
        private readonly IHiddenLocationEncounterService _hiddenLocationEncounterService;

        private readonly HttpClient _httpClient = new HttpClient();

        public EncounterExecutionController(IEncounterExecutionService service, ISocialEncounterService socialEncounterService, IHiddenLocationEncounterService hiddenLocationEncounterService)
        {
            _encounterExecutionService = service;
            _socialEncounterService = socialEncounterService;
            _hiddenLocationEncounterService = hiddenLocationEncounterService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EncounterExecutionDto>>> GetPaged([FromQuery] int page, [FromQuery] int pageSize)
        {
            try
            {
                string url = $"http://localhost:4000/encounterExecution";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    List<EncounterExecutionDto> encounters = JsonConvert.DeserializeObject<List<EncounterExecutionDto>>(responseContent);
                    PagedResult<EncounterExecutionDto> pagedResult = new PagedResult<EncounterExecutionDto>(encounters, encounters.Count);

                    return Ok(pagedResult);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while fetching data.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EncounterExecutionDto>> Create([FromBody] EncounterExecutionDto encounterExecution)
        {
            string json = JsonConvert.SerializeObject(encounterExecution);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounterExecution/create", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    EncounterExecutionDto createdEncounter = JsonConvert.DeserializeObject<EncounterExecutionDto>(responseContent);

                    return Ok(createdEncounter);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while creating encounter execution.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EncounterExecutionDto>> Update([FromBody] EncounterExecutionDto encounterExecution, int id)
        {
            string json = JsonConvert.SerializeObject(encounterExecution);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync($"http://localhost:4000/encounterExecution/update/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    EncounterExecutionDto createdEncounter = JsonConvert.DeserializeObject<EncounterExecutionDto>(responseContent);

                    return Ok(createdEncounter);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while updating encounter execution.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                string url = $"http://localhost:4000/encounterExecution/delete/{id}";
                HttpResponseMessage response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    return Ok(responseContent);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while deleting data.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }

        [HttpGet("checkSocialEncounter/{encounterId:int}")]
        public PagedResult<EncounterExecutionDto> CheckSocialEncounter(int encounterId)
        {
            _socialEncounterService.CheckSocialEncounter(encounterId);

            List<EncounterExecutionDto> result = new List<EncounterExecutionDto>();
            result = _encounterExecutionService.GetAllExecutionsByEncounter(encounterId);

            return new PagedResult<EncounterExecutionDto>(result, result.Count);
        }

        [HttpGet("getActive/{userId:int}")]
        public async Task<ActionResult<EncounterExecutionDto>> GetActiveByUser(int userId)
        {
            try
            {
                string url = $"http://localhost:4000/encounterExecution/getActive/{userId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    EncounterExecutionDto result = JsonConvert.DeserializeObject<EncounterExecutionDto>(responseContent);
                    return Ok(result);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while fetching data.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }

        [HttpGet("checkHidden/{executionId:int}/{encounterId:int}")]
        public ActionResult<bool> GetBooleanValue(int executionId, int encounterId)
        {
            var result = _hiddenLocationEncounterService.CheckHiddenLocationEncounter(executionId, encounterId);

            return Ok(result);
        }

        [HttpGet("completeExecution/{userId:int}")]
        public async Task<ActionResult<EncounterExecutionDto>> CompleteExecution(int userId)
        {
            try
            {
                string url = $"http://localhost:4000/encounterExecution/completeExecution/{userId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    EncounterExecutionDto result = JsonConvert.DeserializeObject<EncounterExecutionDto>(responseContent);
                    return Ok(result);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error occured while fetching data.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error occured while sending request: {ex.Message}");
            }
        }
    }
}