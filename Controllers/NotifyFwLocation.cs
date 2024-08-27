using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IX.DS.FW_Mockup.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class NotifyFwLocation : ControllerBase
    {


        // POST api/<NotifyFwLocation>
        [HttpPost(Name = "InsertFWConnInfo")]
        public void Post([FromBody] string value)
        {

        }

    }
}
