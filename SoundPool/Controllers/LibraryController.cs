using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundPool.Data;

namespace SoundPool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly LibraryContext _dbContext;

        public LibraryController(LibraryContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET api/values
        [HttpGet("Songs")]
        public ActionResult<IEnumerable<string>> GetSongs()
        {
            return Ok(_dbContext
                .Songs
                .Include(s => s.Artist));
        }
    }
}