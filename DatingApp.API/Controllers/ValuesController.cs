using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DatingApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        private readonly DataContext context;

        public ValuesController(DataContext _context)
        {
            context = _context;
        }

        [HttpGet]
        public async Task<IActionResult> GetValues()
        {
            var values = await context.Values.ToListAsync();

            return Ok(values);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetValue(int id)
        {
            var value = await context.Values.FirstOrDefaultAsync(x => x.ID == id);
            return Ok(value);
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        [HttpPost("{id}")]
        public void Post(int id, [FromBody] string value)
        {

        }

        [HttpDelete]
        public void Delete(int id)
        {

        }
    }
}