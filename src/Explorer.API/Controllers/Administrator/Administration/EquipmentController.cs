using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace Explorer.API.Controllers.Administrator.Administration
{


    [Route("api/administration/equipment")]

    public class EquipmentController : BaseApiController
    {
        private readonly IEquipmentService _equipmentService;

        public EquipmentController(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        /*
        [HttpGet]
        public ActionResult<PagedResult<EquipmentDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _equipmentService.GetPaged(page, pageSize);
            return CreateResponse(result);
        }*/


        [HttpGet]
        public async Task<ActionResult<PagedResult<EquipmentDto>>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://host.docker.internal:8081/equipment" + "?page=" + page + "&pageSize=" + pageSize;

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<List<EquipmentDto>>();
                        var pagedResult = new PagedResult<EquipmentDto>(responseData, responseData.Count);

                        return Ok(pagedResult);
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
        /*
        [HttpGet("getTouristEquipment")]
        public ActionResult<ObservableCollection<EquipmentDto>> GetTouristEquipment([FromQuery]List<int> ids)
        {
            var result = _equipmentService.GetTouristEquipment(ids);
            return CreateResponse(result);
        }*/


        [HttpGet("getTouristEquipment")]
        public async Task<ActionResult<ObservableCollection<EquipmentDto>>> GetTouristEquipment([FromQuery] List<int> ids)
        {
            try
            {
                string url = "http://host.docker.internal:8081/equipment/tourist/getEquipment/?";
                if (ids != null && ids.Count > 0)
                {
                    url += "ids=" + string.Join(",", ids);
                }

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<ObservableCollection<EquipmentDto>>();
                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return BadRequest("An error occurred: " + ex.Message);
            }
        }


        /*
        [HttpGet("getOtherEquipment")]
        public ActionResult<ObservableCollection<EquipmentDto>> GetOtherEquipment([FromQuery] List<int> ids)
        {
            var result = _equipmentService.GetOtherEquipment(ids);
            return CreateResponse(result);
        }*/
        [HttpGet("getOtherEquipment")]
        public async Task<ActionResult<ObservableCollection<EquipmentDto>>> GetOtherEquipment([FromQuery] List<int> ids)
        {
            try
            {
                string url = "http://host.docker.internal:8081/equipment/getOtherEquipment?";
                if (ids != null && ids.Count > 0)
                {
                    url += "ids=" + string.Join(",", ids);
                }

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<ObservableCollection<EquipmentDto>>();
                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return BadRequest("An error occurred: " + ex.Message);
            }
        }


        [HttpPost]
        public ActionResult<EquipmentDto> Create([FromBody] EquipmentDto equipment)
        {
            var result = _equipmentService.Create(equipment);
            return CreateResponse(result);
        }

        [HttpPut("{id:int}")]
        public ActionResult<EquipmentDto> Update([FromBody] EquipmentDto equipment)
        {
            var result = _equipmentService.Update(equipment);
            return CreateResponse(result);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _equipmentService.Delete(id);
            return CreateResponse(result);
        }
    }
}
