using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundPool.Data.EFCore;

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

        [HttpGet("Songs")]
        public ActionResult<IEnumerable<string>> GetSongs()
        {
            return Ok(_dbContext
                .Songs
                .Include(s => s.Artist));
        }

        [HttpGet("Artists")]
        public ActionResult<IEnumerable<string>> GetArtists()
        {
            return Ok(_dbContext
                .Songs
                .Include(s => s.Artist));
        }
    }
}