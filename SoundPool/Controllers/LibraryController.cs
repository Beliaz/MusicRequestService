using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundPool.Data;
using SoundPool.Data.EFCore;

namespace SoundPool.Controllers
{
    [Route("soundpool/api/[controller]")]
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

        [HttpGet("Songs/{id}")]
        public ActionResult<IEnumerable<string>> GetSong(string id)
        {
            return Ok(_dbContext
                .Songs.Where(s => s.Id.Equals(id))
                .Include(s => s.Artist)
                .First());
        }

        [HttpGet("Artists")]
        public ActionResult<IEnumerable<string>> GetArtists()
        {
            return Ok(_dbContext.Artists);
        }

        [HttpGet("Artists/{id}")]
        public ActionResult<IEnumerable<string>> GetArtist(string id)
        {
            return Ok(_dbContext.Artists
                .First(s => s.Id.Equals(id)));
        }

        [HttpPost("Songs")]
        public ActionResult PostSong(CreateSongRequest request)
        {
            var artist = _dbContext.Artists.FirstOrDefault(a => a.Name.Equals(request.Artist));
            var exisitingSongs = _dbContext.Songs.Where(s => request.Title.Equals(s.Title));

            if (exisitingSongs.Any() && artist != null)
            {
                var song = exisitingSongs
                    .Include(s => s.Artist)
                    .FirstOrDefault(s => s.Artist.Equals(artist));

                if (song != null)
                    return Ok(song.Id);
            }

            if (artist is null)
            {
                artist = new Artist {Name = request.Artist};
                _dbContext.Artists.Add(artist);
            }

            var newSong = new Song {Title = request.Title, Artist = artist};

            _dbContext.Songs.Add(newSong);
            _dbContext.SaveChanges();

            return Ok(newSong.Id);
        }
    }
}