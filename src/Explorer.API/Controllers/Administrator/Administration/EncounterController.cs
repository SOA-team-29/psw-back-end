using Azure;
using Explorer.Blog.API.Dtos;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Encounters.API;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.Core.Domain.Tours;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Explorer.Encounters.Core.Domain;
using Explorer.Encounters.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Net.Http.Json;


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
        var response = await _httpClient.GetAsync($"http://localhost:4000/encounters?page={page}&pageSize={pageSize}");

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
        var response = await _httpClient.GetAsync($"http://localhost:4000/socialEncounters?page={page}&pageSize={pageSize}");

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
        var response = await _httpClient.GetAsync($"http://localhost:4000/hiddenLocationEncounters?page={page}&pageSize={pageSize}");

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

            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

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


        var baseEncounterResponse = await _httpClient.PostAsJsonAsync("http://localhost:4000/encounters/create", encounterDto);

        if (!baseEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating encounter.");
        }

        var createdEncounter = await baseEncounterResponse.Content.ReadFromJsonAsync<EncounterDto>();

        //NA OSNOVU OBICNOG KREIRAJU HIDDEN LOCATION ENCOUNTER
        var hiddenLocationEncounterDto = new HiddenLocationEncounterDto
        {
            EncounterId = createdEncounter.Id,
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
        var createdHiddenLocationEncounter = await hiddenLocationEncounterResponse.Content.ReadFromJsonAsync<WholeHiddenLocationEncounterDto>();

        return StatusCode((int)HttpStatusCode.Created, createdHiddenLocationEncounter);

    }

    private async Task<ActionResult<EncounterDto>> CreateBaseEncounterAsync(EncounterDto encounterDto)
    {
        string json = JsonConvert.SerializeObject(encounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

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

        //kreiranje encountera
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

        // Pozivamo mikroservis za kreiranje socijalnog sastanka (SocialEncounter)
        var result = await CreateSocialEncounterAsync(socialEncounterDto);

        /*
        if (result.Value == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating social encounter."); // Vraćamo BadRequest ako je rezultat null
        }
        */

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
            //Id = result.Value.Id,
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
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/createSocialEncounter", content);
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
            HttpResponseMessage response = await _httpClient.PutAsync("http://localhost:4000/encounters/update", content);

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

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", encounterContent);

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
    /*
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var result = _encounterService.Delete(id);
        return CreateResponse(result);
    }
    [HttpDelete("hiddenLocation/{baseEncounterId:int}/{hiddenLocationEncounterId:int}")]
    public ActionResult DeleteHiddenLocationEncounter(int baseEncounterId, int hiddenLocationEncounterId)
    {
        var baseEncounter = _encounterService.Delete(baseEncounterId);
        var result = _hiddenLocationEncounterService.Delete(hiddenLocationEncounterId);
        return CreateResponse(result);
    }


    [HttpDelete("social/{baseEncounterId:int}/{socialEncounterId:int}")]
    public ActionResult Delete(int baseEncounterId, int socialEncounterId)
    {
        var baseEncounter = _encounterService.Delete(baseEncounterId);
        var result = _socialEncounterService.Delete(socialEncounterId);
        return CreateResponse(result);
    }
    */

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

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", content);

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

            HttpResponseMessage socialEncounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/updateSocialEncounter", content);

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

    [HttpGet("getEncounter/{encounterId:int}")]
    public async Task<ActionResult<EncounterDto>> GetEncounter(int encounterId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:4000/encounters/getEncounterById/{encounterId}");

        if (response.IsSuccessStatusCode)
        {
            //procita odgovor http zahteva kao json i odmah ga deserijalizuje u odgovarajuci objekat
            var encounter = await response.Content.ReadAsAsync<EncounterDto>();
            return encounter;
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve data from microservice. Status code: {response.StatusCode}");
        }
    }

    private async Task<HttpResponseMessage> DeleteEncounterAsync(int baseEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteEncounter/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetSocialEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getSocialEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetHiddenLocationEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getHiddenLocationEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteSocialEncounterAsync(long socialEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteSocialEncounter/{socialEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteHiddenLocationEncounterAsync(long hiddenLocationEncounterId)
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
    [HttpGet("hiddenLocation/{encounterId:int}")]
    public async Task<ActionResult<HiddenLocationEncounterDto>> GetHiddenLocationEncounterByEncounterId(int encounterId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:4000/encounters/getHiddenLocationEncounter/{encounterId}");

        if (response.IsSuccessStatusCode)
        {
            var hiddenLocationEncounter = await response.Content.ReadAsAsync<HiddenLocationEncounterDto>();
            return hiddenLocationEncounter;
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve data from microservice. Status code: {response.StatusCode}");
        }
    }
}
    






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
        var response = await _httpClient.GetAsync($"http://localhost:4000/encounters?page={page}&pageSize={pageSize}");

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
        var response = await _httpClient.GetAsync($"http://localhost:4000/socialEncounters?page={page}&pageSize={pageSize}");

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
        var response = await _httpClient.GetAsync($"http://localhost:4000/hiddenLocationEncounters?page={page}&pageSize={pageSize}");

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

            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

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


        var baseEncounterResponse = await _httpClient.PostAsJsonAsync("http://localhost:4000/encounters/create", encounterDto);

        if (!baseEncounterResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating encounter.");
        }

        var createdEncounter = await baseEncounterResponse.Content.ReadFromJsonAsync<EncounterDto>();

        //NA OSNOVU OBICNOG KREIRAJU HIDDEN LOCATION ENCOUNTER
        var hiddenLocationEncounterDto = new HiddenLocationEncounterDto
        {
            EncounterId = createdEncounter.Id,
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
        var createdHiddenLocationEncounter = await hiddenLocationEncounterResponse.Content.ReadFromJsonAsync<WholeHiddenLocationEncounterDto>();

        return StatusCode((int)HttpStatusCode.Created, createdHiddenLocationEncounter);

    }

    private async Task<ActionResult<EncounterDto>> CreateBaseEncounterAsync(EncounterDto encounterDto)
    {
        string json = JsonConvert.SerializeObject(encounterDto);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/create", content);

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

        //kreiranje encountera
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

        // Pozivamo mikroservis za kreiranje socijalnog sastanka (SocialEncounter)
        var result = await CreateSocialEncounterAsync(socialEncounterDto);

        /*
        if (result.Value == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred while creating social encounter."); // Vraćamo BadRequest ako je rezultat null
        }
        */

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
            //Id = result.Value.Id,
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
            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:4000/encounters/createSocialEncounter", content);
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
            HttpResponseMessage response = await _httpClient.PutAsync("http://localhost:4000/encounters/update", content);

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

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", encounterContent);

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
    /*
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var result = _encounterService.Delete(id);
        return CreateResponse(result);
    }
    [HttpDelete("hiddenLocation/{baseEncounterId:int}/{hiddenLocationEncounterId:int}")]
    public ActionResult DeleteHiddenLocationEncounter(int baseEncounterId, int hiddenLocationEncounterId)
    {
        var baseEncounter = _encounterService.Delete(baseEncounterId);
        var result = _hiddenLocationEncounterService.Delete(hiddenLocationEncounterId);
        return CreateResponse(result);
    }


    [HttpDelete("social/{baseEncounterId:int}/{socialEncounterId:int}")]
    public ActionResult Delete(int baseEncounterId, int socialEncounterId)
    {
        var baseEncounter = _encounterService.Delete(baseEncounterId);
        var result = _socialEncounterService.Delete(socialEncounterId);
        return CreateResponse(result);
    }
    */

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

            HttpResponseMessage encounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/update", content);

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

            HttpResponseMessage socialEncounterResponse = await _httpClient.PutAsync("http://localhost:4000/encounters/updateSocialEncounter", content);

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

    [HttpGet("getEncounter/{encounterId:int}")]
    public async Task<ActionResult<EncounterDto>> GetEncounter(int encounterId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:4000/encounters/getEncounterById/{encounterId}");

        if (response.IsSuccessStatusCode)
        {
            //procita odgovor http zahteva kao json i odmah ga deserijalizuje u odgovarajuci objekat
            var encounter = await response.Content.ReadAsAsync<EncounterDto>();
            return encounter;
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve data from microservice. Status code: {response.StatusCode}");
        }
    }

    private async Task<HttpResponseMessage> DeleteEncounterAsync(int baseEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteEncounter/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetSocialEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getSocialEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> GetHiddenLocationEncounterIdAsync(int baseEncounterId)
    {
        return await _httpClient.GetAsync($"http://localhost:4000/encounters/getHiddenLocationEncounterId/{baseEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteSocialEncounterAsync(long socialEncounterId)
    {
        return await _httpClient.DeleteAsync($"http://localhost:4000/encounters/deleteSocialEncounter/{socialEncounterId}");
    }

    private async Task<HttpResponseMessage> DeleteHiddenLocationEncounterAsync(long hiddenLocationEncounterId)
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
    [HttpGet("hiddenLocation/{encounterId:int}")]
    public async Task<ActionResult<HiddenLocationEncounterDto>> GetHiddenLocationEncounterByEncounterId(int encounterId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:4000/encounters/getHiddenLocationEncounter/{encounterId}");

        if (response.IsSuccessStatusCode)
        {
            var hiddenLocationEncounter = await response.Content.ReadAsAsync<HiddenLocationEncounterDto>();
            return hiddenLocationEncounter;
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve data from microservice. Status code: {response.StatusCode}");
        }
    }
}





