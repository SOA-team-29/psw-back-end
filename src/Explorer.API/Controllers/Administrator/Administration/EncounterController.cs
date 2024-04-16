using Azure;
using Explorer.Blog.API.Dtos;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Encounters.API;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Explorer.Encounters.Core.Domain;
using Explorer.Encounters.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.Core.Domain.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Explorer.API.Controllers.Administrator.Administration;

//[Authorize(Policy = "administratorPolicy")]
[Route("api/encounters")]
public class EncounterController : BaseApiController
{
    private readonly IEncounterService _encounterService;
    private readonly IHiddenLocationEncounterService _hiddenLocationEncounterService;
    private readonly ISocialEncounterService _socialEncounterService;

   
    private readonly HttpClient _httpClient = new HttpClient();

    public EncounterController(IEncounterService encounterService, ISocialEncounterService socialEncounterService, IHiddenLocationEncounterService hiddenLocationEncounterService)
    {
        _encounterService = encounterService;
        _socialEncounterService = socialEncounterService;
        _hiddenLocationEncounterService = hiddenLocationEncounterService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EncounterDtoDB>>> GetAllEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        
        var response = await _httpClient.GetAsync($"http://localhost:4000/encounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            List<EncounterDtoDB> encounters = JsonConvert.DeserializeObject<List<EncounterDtoDB>>(responseContent);
            PagedResult<EncounterDtoDB> pagedResult = new PagedResult<EncounterDtoDB>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpGet("social")]
    public async Task<ActionResult<PagedResult<SocialEncounterDtoDB>>> GetAllSocialEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        var response = await _httpClient.GetAsync($"http://localhost:4000/socialEncounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            List<SocialEncounterDtoDB> encounters = JsonConvert.DeserializeObject<List<SocialEncounterDtoDB>>(responseContent);

            PagedResult<SocialEncounterDtoDB> pagedResult = new PagedResult<SocialEncounterDtoDB>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpGet("hiddenLocation")]
    public async Task<ActionResult<PagedResult<HiddenLocationEncounterDtoDB>>> GetAllHiddenLocationEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        var response = await _httpClient.GetAsync($"http://localhost:4000/hiddenLocationEncounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            List<HiddenLocationEncounterDtoDB> encounters = JsonConvert.DeserializeObject<List<HiddenLocationEncounterDtoDB>>(responseContent);

            PagedResult<HiddenLocationEncounterDtoDB> pagedResult = new PagedResult<HiddenLocationEncounterDtoDB>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<EncounterDtoDB>> Create([FromBody] EncounterDtoDB encounter)
    {
        string json = JsonConvert.SerializeObject(encounter);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

            if (response.IsSuccessStatusCode)
            {
 
                string responseContent = await response.Content.ReadAsStringAsync();

                EncounterDtoDB createdEncounter = JsonConvert.DeserializeObject<EncounterDtoDB>(responseContent);

                return Ok(createdEncounter);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error occurred while creating encounter.");
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }

    [HttpPost("hiddenLocation")]
    public async Task<ActionResult<WholeHiddenLocationEncounterDtoDB>> Create([FromBody] WholeHiddenLocationEncounterDtoDB wholeEncounter)
    {
        var encounterDto = new EncounterDtoDB
        {
            Name = wholeEncounter.Name,
            Description = wholeEncounter.Description,
            XpPoints = wholeEncounter.XpPoints,
            Status = wholeEncounter.Status,
            Type = wholeEncounter.Type,
            Longitude = wholeEncounter.Longitude,
            Latitude = wholeEncounter.Latitude,
            ShouldBeApproved = wholeEncounter.ShouldBeApproved
        };
        var baseEncounterResponse = await _httpClient.PostAsJsonAsync("http://localhost:4000/encounters/create", encounterDto);

        if (!baseEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating encounter.");
        }

        var createdEncounter = await baseEncounterResponse.Content.ReadFromJsonAsync<EncounterDtoDB>();

        var hiddenLocationEncounterDto = new HiddenLocationEncounterDtoDB
        {
            EncounterId = createdEncounter.Id.ToString(),
            ImageLatitude = wholeEncounter.ImageLatitude,
            ImageLongitude = wholeEncounter.ImageLongitude,
            ImageURL = wholeEncounter.ImageURL,
            DistanceTreshold = wholeEncounter.DistanceTreshold
        };

        var hiddenLocationEncounterResponse = await _httpClient.PostAsJsonAsync("http://localhost:4000/encounters/createHiddenLocationEncounter", hiddenLocationEncounterDto);

        if (!hiddenLocationEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating hidden location encounter.");
        }

        var createdHiddenLocationEncounter = await hiddenLocationEncounterResponse.Content.ReadFromJsonAsync<WholeHiddenLocationEncounterDtoDB>();

        return StatusCode((int)HttpStatusCode.Created, createdHiddenLocationEncounter);
    }

    private async Task<ActionResult<EncounterDtoDB>> CreateBaseEncounterAsync(EncounterDtoDB encounterDto)
    {
        string json = JsonConvert.SerializeObject(encounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                EncounterDtoDB createdEncounter = JsonConvert.DeserializeObject<EncounterDtoDB>(responseContent);
                return Ok(createdEncounter);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error occurred while creating encounter.");
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }

    //SOCIAL ENCOUNTER CEO
    [HttpPost("social")]
    public async Task<ActionResult<WholeSocialEncounterDto>> CreateSocialEncounter([FromBody] WholeSocialEncounterDto socialEncounter)
    {
        EncounterDtoDB encounterDto = new EncounterDtoDB
        {
            Name = socialEncounter.Name,
            Description = socialEncounter.Description,
            XpPoints = socialEncounter.XpPoints,
            Status = socialEncounter.Status,
            Type = socialEncounter.Type,
            Longitude = socialEncounter.Longitude,
            Latitude = socialEncounter.Latitude,
            ShouldBeApproved = socialEncounter.ShouldBeApproved
        };


        var baseEncounterResponse = await CreateBaseEncounterAsync(encounterDto);

        var baseEncounter = (OkObjectResult)baseEncounterResponse.Result;
        var createdEncounter = (EncounterDtoDB)baseEncounter.Value;

        SocialEncounterDtoDB socialEncounterDto = new SocialEncounterDtoDB
        {
            EncounterId = createdEncounter.Id,
            TouristsRequiredForCompletion = socialEncounter.TouristsRequiredForCompletion,
            DistanceTreshold = socialEncounter.DistanceTreshold,
            TouristIDs = socialEncounter.TouristIDs
        };

      
        var result = await CreateSocialEncounterAsync(socialEncounterDto);

        var wholeSocialEncounterDto = new WholeSocialEncounterDtoDB
        {
            EncounterId = createdEncounter.Id,
            Name = socialEncounter.Name,
            Description = socialEncounter.Description,
            XpPoints = socialEncounter.XpPoints,
            Status = socialEncounter.Status,
            Type = socialEncounter.Type,
            Latitude = socialEncounter.Latitude,
            Longitude = socialEncounter.Longitude,
            TouristsRequiredForCompletion = socialEncounter.TouristsRequiredForCompletion,
            DistanceTreshold = socialEncounter.DistanceTreshold,
            TouristIDs = socialEncounter.TouristIDs,
            ShouldBeApproved = socialEncounter.ShouldBeApproved
        };

        return StatusCode((int)HttpStatusCode.Created, wholeSocialEncounterDto);
    }

    private async Task<ActionResult<SocialEncounterDtoDB>> CreateSocialEncounterAsync(SocialEncounterDtoDB socialEncounterDto)
    {
        string json = JsonConvert.SerializeObject(socialEncounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/createSocialEncounter", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                SocialEncounterDtoDB createdSocialEncounter = JsonConvert.DeserializeObject<SocialEncounterDtoDB>(responseContent);
                return Ok(createdSocialEncounter);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error occurred while creating social encounter.");
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }

    [HttpPut]
    public async Task<ActionResult<EncounterDtoDB>> Update([FromBody] EncounterDtoDB encounter)
    {
        string json = JsonConvert.SerializeObject(encounter);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PutAsync("http://localhost:4000/encounters/update", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                EncounterDtoDB updatedEncounter = JsonConvert.DeserializeObject<EncounterDtoDB>(responseContent);

                return Ok(updatedEncounter);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error occurred while updating encounter.");
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }

    [HttpPut("hiddenLocation")]
    public async Task<ActionResult<HiddenLocationEncounterDtoDB>> Update([FromBody] WholeHiddenLocationEncounterDtoDB wholeEncounter)
    {
        var encounterDto = new EncounterDtoDB
        {
            Id = wholeEncounter.EncounterId,
            Name = wholeEncounter.Name,
            Description = wholeEncounter.Description,
            XpPoints = wholeEncounter.XpPoints,
            Status = wholeEncounter.Status,
            Type = wholeEncounter.Type,
            Longitude = wholeEncounter.Longitude,
            Latitude = wholeEncounter.Latitude,
            ShouldBeApproved = wholeEncounter.ShouldBeApproved
        };

        try
        {
            string encounterJson = JsonConvert.SerializeObject(encounterDto);
            HttpContent encounterContent = new StringContent(encounterJson, Encoding.UTF8, "application/json");

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", encounterContent);

            if (!encounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)encounterResponse.StatusCode, "Error occurred while updating encounter.");
            }

            var hiddenLocationEncounterDto = new HiddenLocationEncounterDtoDB
            {
                Id = wholeEncounter.Id,
                EncounterId = encounterDto.Id,
                ImageLatitude = wholeEncounter.ImageLatitude,
                ImageLongitude = wholeEncounter.ImageLongitude,
                ImageURL = wholeEncounter.ImageURL,
                DistanceTreshold = wholeEncounter.DistanceTreshold
            };

            string hiddenLocationEncounterJson = JsonConvert.SerializeObject(hiddenLocationEncounterDto);
            HttpContent hiddenLocationEncounterContent = new StringContent(hiddenLocationEncounterJson, Encoding.UTF8, "application/json");

            HttpResponseMessage hiddenLocationEncounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/updateHiddenLocationEncounter", hiddenLocationEncounterContent);

            if (!hiddenLocationEncounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)hiddenLocationEncounterResponse.StatusCode, "Error occurred while updating hidden location encounter.");
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }
    [HttpPut("social")]
    public async Task<ActionResult<SocialEncounterDtoDB>> UpdateSocial([FromBody] WholeSocialEncounterDtoDB wholeEncounter)
    {
        var encounterDto = new EncounterDtoDB
        {
            Id = wholeEncounter.EncounterId,
            Name = wholeEncounter.Name,
            Description = wholeEncounter.Description,
            XpPoints = wholeEncounter.XpPoints,
            Status = wholeEncounter.Status,
            Type = wholeEncounter.Type,
            Longitude = wholeEncounter.Longitude,
            Latitude = wholeEncounter.Latitude,
            ShouldBeApproved = wholeEncounter.ShouldBeApproved
        };

        try
        {
            string encounterJson = JsonConvert.SerializeObject(encounterDto);
            HttpContent encounterContent = new StringContent(encounterJson, Encoding.UTF8, "application/json");

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", encounterContent);

            if (!encounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)encounterResponse.StatusCode, "Error occurred while updating encounter.");
            }

            var socialEncounterDto = new SocialEncounterDtoDB
            {
                Id = wholeEncounter.Id,
                EncounterId = encounterDto.Id,
                TouristsRequiredForCompletion = wholeEncounter.TouristsRequiredForCompletion,
                DistanceTreshold = wholeEncounter.DistanceTreshold,
                TouristIDs = wholeEncounter.TouristIDs
            };

            string socialEncounterJson = JsonConvert.SerializeObject(socialEncounterDto);
            HttpContent socialEncounterContent = new StringContent(socialEncounterJson, Encoding.UTF8, "application/json");

            HttpResponseMessage socialEncounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/updateSocialEncounter", socialEncounterContent);

            if (!socialEncounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)socialEncounterResponse.StatusCode, "Error occurred while updating social encounter.");
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }



  

    [HttpDelete("{baseEncounterId}")]
    public async Task<ActionResult> DeleteEncounter(string baseEncounterId)
    {
        var baseEncounterResponse = await DeleteEncounterAsync(baseEncounterId);


        return CreateResponse(baseEncounterResponse);
    }

    private async Task<HttpResponseMessage> DeleteEncounterAsync(string baseEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteEncounter/{baseEncounterId}");
    }
    private async Task<HttpResponseMessage> GetSocialEncounterIdAsync(string baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getSocialEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetHiddenLocationEncounterIdAsync(string baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getHiddenLocationEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteSocialEncounterAsync(string socialEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteSocialEncounter/{socialEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteHiddenLocationEncounterAsync(string hiddenLocationEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteHiddenLocationEncounter/{hiddenLocationEncounterId}");
    }

    private ActionResult CreateResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return Ok("Encounter deleted successfully.");
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Failed to delete encounter.");
        }
    }


}