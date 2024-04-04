using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Explorer.API.Controllers.Tourist
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/tourist/touristEquipment")]
    public class TouristEquipmentController : BaseApiController
    {
        private readonly ITouristEquipmentService _touristEquipmentService;
        private readonly IEquipmentService _equipmentService;

        public TouristEquipmentController(ITouristEquipmentService equipmentService, IEquipmentService service)
        {
            _touristEquipmentService = equipmentService;
            _equipmentService = service;

        }

        /*
       [HttpGet("getTouristEquipment/{touristId:int}")]
       public ActionResult<TouristEquipmentDto> GetTouristEquipment(int touristId)
       {
           var result =  _touristEquipmentService.GetTouristEquipment(touristId); 
           return CreateResponse(result);
       }*/
        [HttpGet("getTouristEquipment/{touristId:int}")]
        public async Task<ActionResult> GetTouristEquipment(int touristId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string goServiceUrl = "http://host.docker.internal:8081/getTouristEquipment/" + touristId;
                    HttpResponseMessage response = await client.GetAsync(goServiceUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Čitanje odgovora
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return Ok(responseBody);
                    }
                    else
                    {

                        return BadRequest("Failed to get tourist equipment. Status code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "An error occurred: " + ex.Message);
                }
            }
        }
        /*
        [HttpPost("createTouristEquipment/{id:int}")]
        public ActionResult<TouristEquipmentDto> CreteTouristEquipment(int id)
        {
            var result = _touristEquipmentService.Create(id);
            return CreateResponse(result);
        }

        [HttpPut("addToMyEquipment/{touristId:int}/{equipmentId:int}")]
        public ActionResult<TouristEquipmentDto> AddToMyEquipment(int touristId, int equipmentId)
        {
            var result = _touristEquipmentService.AddToMyEquipment(touristId, equipmentId);
            return CreateResponse(result);
        }


        [HttpPut("deleteFromMyEquipment/{touristId:int}/{equipmentId:int}")]
        public ActionResult<TouristEquipmentDto> DeleteFromMyEquipment(int touristId, int equipmentId)
        {
            var result = _touristEquipmentService.DeleteFromMyEquipment(touristId, equipmentId);
            return CreateResponse(result);
        }*/

        [HttpPost("createTouristEquipment/{id:int}")]
        public async Task<ActionResult<TouristEquipmentDto>> CreteTouristEquipment(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsJsonAsync($"http://host.docker.internal:8081/touristEquipment/createTouristEquipment/{id}", "");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<TouristEquipmentDto>();
                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return BadRequest("An error occurred: " + ex.Message);
                }
            }
        }

        [HttpPut("addToMyEquipment/{touristId:int}/{equipmentId:int}")]
        public async Task<ActionResult<TouristEquipmentDto>> AddToMyEquipment(int touristId, int equipmentId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PutAsJsonAsync($"http://host.docker.internal:8081/touristEquipment/addToMyEquipment/{touristId}/{equipmentId}", "");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<TouristEquipmentDto>();
                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return BadRequest("An error occurred: " + ex.Message);
                }
            }
        }




        [HttpPut("deleteFromMyEquipment/{touristId:int}/{equipmentId:int}")]
        public async Task<ActionResult<TouristEquipmentDto>> DeleteFromMyEquipment(int touristId, int equipmentId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PutAsJsonAsync($"http://host.docker.internal:8081/touristEquipment/deleteFromMyEquipment/{touristId}/{equipmentId}", "");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<TouristEquipmentDto>();
                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return BadRequest("An error occurred: " + ex.Message);
                }
            }
        }
    }
}