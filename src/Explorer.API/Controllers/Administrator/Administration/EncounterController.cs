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
    public async Task<ActionResult<PagedResult<EncounterDto>>> GetAllEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        
        var response = await _httpClient.GetAsync($"http://host.docker.internal:4000/encounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            List<EncounterDto> encounters = JsonConvert.DeserializeObject<List<EncounterDto>>(responseContent);
            PagedResult<EncounterDto> pagedResult = new PagedResult<EncounterDto>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpGet("social")]
    public async Task<ActionResult<PagedResult<SocialEncounterDto>>> GetAllSocialEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        var response = await _httpClient.GetAsync($"http://host.docker.internal:4000/socialEncounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            List<SocialEncounterDto> encounters = JsonConvert.DeserializeObject<List<SocialEncounterDto>>(responseContent);

            PagedResult<SocialEncounterDto> pagedResult = new PagedResult<SocialEncounterDto>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpGet("hiddenLocation")]
    public async Task<ActionResult<PagedResult<HiddenLocationEncounterDto>>> GetAllHiddenLocationEncounters([FromQuery] int page, [FromQuery] int pageSize)
    {
        var response = await _httpClient.GetAsync($"http://host.docker.internal:4000/hiddenLocationEncounters?page={page}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            List<HiddenLocationEncounterDto> encounters = JsonConvert.DeserializeObject<List<HiddenLocationEncounterDto>>(responseContent);

            PagedResult<HiddenLocationEncounterDto> pagedResult = new PagedResult<HiddenLocationEncounterDto>(encounters, encounters.Count);

            return Ok(pagedResult);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching encounters.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<EncounterDto>> Create([FromBody] EncounterDto encounter)
    {
        string json = JsonConvert.SerializeObject(encounter);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://host.docker.internal:4000/encounters/create", content);

            if (response.IsSuccessStatusCode)
            {
 
                string responseContent = await response.Content.ReadAsStringAsync();

                EncounterDto createdEncounter = JsonConvert.DeserializeObject<EncounterDto>(responseContent);

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
    public async Task<ActionResult<WholeHiddenLocationEncounterDto>> Create([FromBody] WholeHiddenLocationEncounterDto wholeEncounter)
    {
        var encounterDto = new EncounterDto
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
        var baseEncounterResponse = await _httpClient.PostAsJsonAsync("http://host.docker.internal:4000/encounters/create", encounterDto);

        if (!baseEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating encounter.");
        }

        var createdEncounter = await baseEncounterResponse.Content.ReadFromJsonAsync<EncounterDto>();

        var hiddenLocationEncounterDto = new HiddenLocationEncounterDto
        {
            EncounterId = createdEncounter.Id,
            ImageLatitude = wholeEncounter.ImageLatitude,
            ImageLongitude = wholeEncounter.ImageLongitude,
            ImageURL = wholeEncounter.ImageURL,
            DistanceTreshold = wholeEncounter.DistanceTreshold
        };

        var hiddenLocationEncounterResponse = await _httpClient.PostAsJsonAsync("http://host.docker.internal:4000/encounters/createHiddenLocationEncounter", hiddenLocationEncounterDto);

        if (!hiddenLocationEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating hidden location encounter.");
        }

        var createdHiddenLocationEncounter = await hiddenLocationEncounterResponse.Content.ReadFromJsonAsync<WholeHiddenLocationEncounterDto>();

        return StatusCode((int)HttpStatusCode.Created, createdHiddenLocationEncounter);
    }

    private async Task<ActionResult<EncounterDto>> CreateBaseEncounterAsync(EncounterDto encounterDto)
    {
        string json = JsonConvert.SerializeObject(encounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://host.docker.internal:4000/encounters/create", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                EncounterDto createdEncounter = JsonConvert.DeserializeObject<EncounterDto>(responseContent);
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
        EncounterDto encounterDto = new EncounterDto
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
        var createdEncounter = (EncounterDto)baseEncounter.Value;

        SocialEncounterDto socialEncounterDto = new SocialEncounterDto
        {
            EncounterId = createdEncounter.Id,
            TouristsRequiredForCompletion = socialEncounter.TouristsRequiredForCompletion,
            DistanceTreshold = socialEncounter.DistanceTreshold,
            TouristIDs = socialEncounter.TouristIDs
        };

      
        var result = await CreateSocialEncounterAsync(socialEncounterDto);

        var wholeSocialEncounterDto = new WholeSocialEncounterDto
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

    private async Task<ActionResult<SocialEncounterDto>> CreateSocialEncounterAsync(SocialEncounterDto socialEncounterDto)
    {
        string json = JsonConvert.SerializeObject(socialEncounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://host.docker.internal:4000/encounters/createSocialEncounter", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                SocialEncounterDto createdSocialEncounter = JsonConvert.DeserializeObject<SocialEncounterDto>(responseContent);
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
    public async Task<ActionResult<EncounterDto>> Update([FromBody] EncounterDto encounter)
    {
        string json = JsonConvert.SerializeObject(encounter);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PutAsync("http://host.docker.internal:4000/encounters/update", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                EncounterDto updatedEncounter = JsonConvert.DeserializeObject<EncounterDto>(responseContent);

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
    public async Task<ActionResult<HiddenLocationEncounterDto>> Update([FromBody] WholeHiddenLocationEncounterDto wholeEncounter)
    {
        var encounterDto = new EncounterDto
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

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://host.docker.internal:4000/encounters/update", encounterContent);

            if (!encounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)encounterResponse.StatusCode, "Error occurred while updating encounter.");
            }

            var hiddenLocationEncounterDto = new HiddenLocationEncounterDto
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

            HttpResponseMessage hiddenLocationEncounterResponse = await _httpClient.PutAsync("http://host.docker.internal:4000/encounters/updateHiddenLocationEncounter", hiddenLocationEncounterContent);

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
    public async Task<ActionResult<WholeSocialEncounterDto>> UpdateSocialEncounter([FromBody] WholeSocialEncounterDto socialEncounter)
    {
        var encounterDto = new EncounterDto
        {
            Id = socialEncounter.EncounterId,
            Name = socialEncounter.Name,
            Description = socialEncounter.Description,
            XpPoints = socialEncounter.XpPoints,
            Status = socialEncounter.Status,
            Type = socialEncounter.Type,
            Longitude = socialEncounter.Longitude,
            Latitude = socialEncounter.Latitude,
            ShouldBeApproved = socialEncounter.ShouldBeApproved
        };

        try
        {
            string json = JsonConvert.SerializeObject(encounterDto);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://host.docker.internal:4000/encounters/update", content);

            if (!encounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)encounterResponse.StatusCode, "Error occurred while updating encounter.");
            }

            string encounterResponseContent = await encounterResponse.Content.ReadAsStringAsync();

            EncounterDto updatedEncounter = JsonConvert.DeserializeObject<EncounterDto>(encounterResponseContent);

            var socialEncounterDto = new SocialEncounterDto
            {
                Id = socialEncounter.Id,
                EncounterId = updatedEncounter.Id,
                TouristsRequiredForCompletion = socialEncounter.TouristsRequiredForCompletion,
                DistanceTreshold = socialEncounter.DistanceTreshold,
                TouristIDs = socialEncounter.TouristIDs
            };

            HttpResponseMessage socialEncounterResponse = await _httpClient.PutAsync("http://host.docker.internal:4000/encounters/updateSocialEncounter", content);

            if (!socialEncounterResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)socialEncounterResponse.StatusCode, "Error occurred while updating social encounter.");
            }

            return StatusCode((int)HttpStatusCode.NoContent, socialEncounterDto);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error occurred while sending request: {ex.Message}");
        }
    }


    [HttpDelete("{baseEncounterId:int}")]
    public async Task<ActionResult> DeleteEncounter(int baseEncounterId)
    {
        var baseEncounterResponse = await DeleteEncounterAsync(baseEncounterId);

        if (baseEncounterResponse.IsSuccessStatusCode || baseEncounterResponse.StatusCode == HttpStatusCode.NoContent)
        {
            var socialEncounterIdResponse = await GetSocialEncounterIdAsync(baseEncounterId);

            string jsonResponse1 = await socialEncounterIdResponse.Content.ReadAsStringAsync();
            JObject jsonObject1 = JObject.Parse(jsonResponse1);
            int socialEncounterId = (int)jsonObject1["socialEncounterId"];

            var hiddenLocationEncounterIdResponse = await GetHiddenLocationEncounterIdAsync(baseEncounterId);

            string jsonResponse2 = await hiddenLocationEncounterIdResponse.Content.ReadAsStringAsync();
            JObject jsonObject2 = JObject.Parse(jsonResponse2);
            int hiddenLocationEncounterId = (int)jsonObject2["hiddenLocationEncounterId"];

            if (socialEncounterId != -1)
            {
                var socialEncounterResponse = await DeleteSocialEncounterAsync(socialEncounterId);
                return CreateResponse(socialEncounterResponse);
            }
            else if (hiddenLocationEncounterId != -1)
            {
                var hiddenLocationEncounterResponse = await DeleteHiddenLocationEncounterAsync(hiddenLocationEncounterId);
                return CreateResponse(hiddenLocationEncounterResponse);
            }
        }

        return CreateResponse(baseEncounterResponse);
    }

    private async Task<HttpResponseMessage> DeleteEncounterAsync(int baseEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://host.docker.internal:4000/encounters/deleteEncounter/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetSocialEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://host.docker.internal:4000/encounters/getSocialEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetHiddenLocationEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://host.docker.internal:4000/encounters/getHiddenLocationEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteSocialEncounterAsync(long socialEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://host.docker.internal:4000/encounters/deleteSocialEncounter/{socialEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteHiddenLocationEncounterAsync(long hiddenLocationEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://host.docker.internal:4000/encounters/deleteHiddenLocationEncounter/{hiddenLocationEncounterId}");
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